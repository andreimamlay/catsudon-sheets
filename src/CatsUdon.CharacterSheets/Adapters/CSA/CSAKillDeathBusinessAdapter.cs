using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.CCFolia;
using CatsUdon.CharacterSheets.Memo;
using CatsUdon.CharacterSheets.TextSheets;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters.CSA;

public partial class CSAKillDeathBusinessAdapter(HttpClient httpClient) : ICharacterSheetAdapter
{
    [GeneratedRegex(@"^https:\/\/character-sheets\.appspot\.com\/helltv\/edit\.html\?key=(?<key>[\w_\-]+)$")]
    private static partial Regex UrlMatchRegex { get; }

    [GeneratedRegex(@"^skills\.row(?<row>\d+)\.name(?<column>\d+)$")]
    private static partial Regex SkillRcRegex { get; }

    [GeneratedRegex(@"[ヶ々〆〇〻㐂-頻]+")]
    private static partial Regex AbilityNameRegex { get; }

    private static Lazy<GameSystemInfo[]> supportedSystems = new([
        new GameSystemInfo()
        {
            ProviderName = "キャラクターシート倉庫",
            ProviderHomePageUrl = new Uri("https://character-sheets.appspot.com/"),
            GameSystemName = "リアリティショーRPG キルデスビジネス",
            MainPageUrl = new Uri("https://character-sheets.appspot.com/helltv/"),
            CharacterCreationUrl = new Uri("https://character-sheets.appspot.com/helltv/edit.html"),
        }
    ]);
    public GameSystemInfo[] SupportedGameSystems => supportedSystems.Value;

    public bool CanConvert(string url) => UrlMatchRegex.IsMatch(url);

    public async Task<CharacterSheet> Convert(string url)
    {
        if (!CanConvert(url))
        {
            throw new ArgumentException("URL is not supported", nameof(url));
        }

        var match = UrlMatchRegex.Match(url);
        var key = match.Groups["key"].Value;

        var characterSheetData = await httpClient.GetAsync($"https://character-sheets.appspot.com/helltv/display?key={key}&ajax=1");
        characterSheetData.EnsureSuccessStatusCode();

        var characterJson = await characterSheetData.Content.ReadFromJsonAsync<CharacterJson>();
        if (characterJson is null)
        {
            throw new InvalidOperationException("Failed to read character sheet data");
        }

        var character = new Character();
        ReadCharacter(character, characterJson);
        var memo = CreateMemo(character, characterJson);

        return new CharacterSheet()
        {
            Character = ConvertToCCFoliaCharacter(character, memo),
            AdditionalTextSheets = CreateTextSheets(character),
        };
    }

    private static string CreateMemo(Character character, CharacterJson characterJson)
    {
        using var memoBuilder = new MemoBuilder();

        memoBuilder.BeginSize(12);
        memoBuilder.Header("アビリティ");

        var abilitiesCount = characterJson.Ability.Length;
        foreach (var (index, ability) in characterJson.Ability.Index())
        {
            if (string.IsNullOrWhiteSpace(ability.Name)) break;

            var isEmpty = string.IsNullOrWhiteSpace(ability.Level)
                && string.IsNullOrWhiteSpace(ability.Effect)
                && string.IsNullOrWhiteSpace(ability.TargetSkill);

            if (isEmpty) continue;

            var risk = abilitiesCount - index;
            
            var abilityNameMatch = AbilityNameRegex.Match(ability.Name);
            if (!abilityNameMatch.Success)
            {
                memoBuilder.Text($"▷ ＜{risk}＞【{ability.Name.Replace('\n', ' ').Trim()}】");
            }
            else
            {
                var nameParts = ability.Name.Split(['\n', '/'], StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length == 1)
                {
                    memoBuilder.Text($"▷ ＜{risk}＞ {abilityNameMatch.Value}");
                }
                else
                {
                    memoBuilder.Text($"▷ ＜{risk}＞ {abilityNameMatch.Value} 【{nameParts[1].Trim()}】");
                }
            }

            memoBuilder.TextPrefixedOptional("  レベル", ability.Level)
                .IfNotEmpty(ability.TargetSkill, $"  指定特技：<b>{ability.TargetSkill}</b>");

            if (!string.IsNullOrWhiteSpace(ability.Effect))
            {
                memoBuilder.NewLine().BeginMargin("2em").Text(ability.Effect).EndMargin();
            }

            memoBuilder.NewLine(2);
        }

        memoBuilder.NewLine();
        memoBuilder.Header("関係");
        if (characterJson.Relations.Length == 0)
        {
            memoBuilder.Text("なし");
        }

        foreach (var (index, relation) in characterJson.Relations.Index())
        {
            if (string.IsNullOrWhiteSpace(relation.Name)) break;

            memoBuilder.Text($"▷ {relation.Name}")
                .TextPrefixedOptional("  属性", relation.Attribute)
                .TextPrefixedOptional("  深度", relation.Depth);

            if (!string.IsNullOrWhiteSpace(relation.Notes))
            {
                memoBuilder.NewLine().BeginMargin("2em").Text(relation.Notes).EndMargin();
            }

            memoBuilder.NewLine(2);
        }

        memoBuilder.NewLine();
        memoBuilder.Header("ヘルサイバー");
        if (characterJson.Hellcyber.Length == 0)
        {
            memoBuilder.Text("なし");
        }

        foreach (var (index, background) in characterJson.Hellcyber.Index())
        {
            if (string.IsNullOrWhiteSpace(background.Name)) break;

            memoBuilder.Text($"▷ {background.Name}");

            if (!string.IsNullOrWhiteSpace(background.Effect))
            {
                memoBuilder.NewLine().BeginMargin("2em").Text(background.Effect).EndMargin();
            }

            memoBuilder.NewLine(2);
        }

        if (!string.IsNullOrWhiteSpace(characterJson.Base.Memo))
        {
            memoBuilder.NewLine();
            memoBuilder.Header("設定");
            memoBuilder.Text(characterJson.Base.Memo.Trim());
        }

        memoBuilder.NewLine().NewLine();
        memoBuilder.EndSize();

        return memoBuilder.ToString();
    }

    private static void ReadCharacter(Character character, CharacterJson characterJson)
    {
        character.Name = characterJson.Base.Name?.Replace("　", " ") ?? string.Empty;
        character.NameKana = characterJson.Base.NameKana?.Replace("　", " ") ?? string.Empty;

        if (int.TryParse(characterJson.Base.Soul, out var soul))
        {
            character.Soul = soul;
        }

        if (int.TryParse(characterJson.Base.Hellslug, out var hellslug))
        {
            character.Hellslug = hellslug;
        }

        var skills = new List<(int row, int column)>();

        foreach (var learnedSkill in characterJson.Learned)
        {
            if (string.IsNullOrWhiteSpace(learnedSkill.Id)) continue;

            var match = SkillRcRegex.Match(learnedSkill.Id);
            if (!match.Success) continue;

            skills.Add((int.Parse(match.Groups["row"].Value), int.Parse(match.Groups["column"].Value)));
        }

        var skillsSorted = skills.OrderBy(s => s.row).ThenBy(s => s.column);
        character.Skills = [.. skillsSorted];
        character.Abilities = characterJson.Ability;
    }

    private static List<string> CreateTextSheets(Character character)
    {
        var sheets = new List<string>();

        var grid = new Grid();
        grid.Fill(StyleAndSkills);

        for (int c = 0; c < StyleAndSkills[0].Length; c++)
        {
            grid.Cells[0, c].Pushed = true;
        }

        foreach (var (row, col) in character.Skills)
        {
            grid.Cells[row + 1, col].Pushed = true;
        }

        sheets.Add(grid.ToString());

        return sheets;
    }

    private static CCFoliaCharacterClipboardData ConvertToCCFoliaCharacter(Character character, string memo)
    {
        string characterName;
        if (!string.IsNullOrWhiteSpace(character.NameKana) && character.Name != character.NameKana)
        {
            characterName = $"{character.Name}（{character.NameKana}）";
        }
        else
        {
            characterName = character.Name;
        }

        var ccfoliaCharacter = new CCFoliaCharacter
        {
            Name = characterName,
            Memo = memo
        };

        ccfoliaCharacter.Status.Add(new CCFoliaStatus() { Label = "ソウル", Value = character.Soul, Max = 0 });
        ccfoliaCharacter.Status.Add(new CCFoliaStatus() { Label = "ヘルスラッグ", Value = character.Hellslug, Max = 0 });
        ccfoliaCharacter.Status.Add(new CCFoliaStatus() { Label = "チャージ", Value = 0, Max = 3 });
        ccfoliaCharacter.Status.Add(new CCFoliaStatus() { Label = "ブロック", Value = 0, Max = 3 });
        ccfoliaCharacter.Status.Add(new CCFoliaStatus() { Label = "死亡回数", Value = 0, Max = 6 });

        return new CCFoliaCharacterClipboardData() { Data = ccfoliaCharacter };
    }

    public class CharacterJson
    {
        public required CharacterBase Base { get; set; }
        public required LevelCounter Block { get; set; }
        public required LevelCounter Charge { get; set; }
        public required LevelCounter Deathcount { get; set; }
        public LearnedSkill[] Learned { get; set; } = [];
        public Hellcyber[] Hellcyber { get; set; } = [];
        public Ability[] Ability { get; set; } = [];
        public Relation[] Relations { get; set; } = [];
    }

    public class CharacterBase
    {
        public string? Name { get; set; }
        public string? NameKana { get; set; }
        public string? Memo { get; set; }
        public string? Soul { get; set; }
        public string? Hellslug { get; set; }
    }

    public class LevelCounter
    {
        public string? Level { get; set; }
    }

    public class LearnedSkill
    {
        public string? Id { get; set; }
        public string? Judge { get; set; }
    }

    public class Ability
    {
        public string? Effect { get; set; }
        public string? Level { get; set; }
        public string? Name { get; set; }
        public string? TargetSkill { get; set; }
    }

    public class Hellcyber
    {
        public string? Effect { get; set; }
        public string? Name { get; set; }
    }

    public class Relation
    {
        public string? Attribute { get; set; }
        public string? Depth { get; set; }
        public string? Name { get; set; }
        public string? Notes { get; set; }
    }

    public class Character
    {
        public string Name { get; set; } = string.Empty;
        public string NameKana { get; set; } = string.Empty;
        public int Soul { get; set; }
        public int Hellslug { get; set; }
        public List<(int row, int col)> Skills { get; set; } = [];
        public Ability[] Abilities { get; set; } = [];
    }

    private static readonly string[][] StyleAndSkills = [
        ["職業", "動作", "小道具", "衣装", "情動", "願望"],
        ["無職", "叫ぶ", "ピアス", "ネイキッド", "愛", "死"],
        ["芸術家", "閃く", "髪飾り", "アウトドア", "喜び", "復讐"],
        ["研究者", "斬る", "銃", "エスニック", "期待", "勝利"],
        ["家事手伝い", "振る", "ネックレス", "ヒップホップ", "焦り", "支配"],
        ["学生", "投げる", "ベルト", "ミリタリー", "自負", "獲得"],
        ["悪漢", "殴る", "眼鏡", "フォーマル", "怒り", "繁栄"],
        ["労働者", "蹴る", "帽子", "トラッド", "悲しみ", "強化"],
        ["探偵", "跳ぶ", "時計", "ゴシック", "嫉妬", "安全"],
        ["大物", "撃つ", "剣", "パンク", "恐怖", "健康"],
        ["医師", "掴む", "リング", "メタル", "恥", "長寿"],
        ["公務員", "待つ", "タトゥー", "アイドル", "嫌悪", "生"],
    ];
}
