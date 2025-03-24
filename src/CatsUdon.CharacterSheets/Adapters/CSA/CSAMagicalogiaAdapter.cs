using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.CCFolia;
using CatsUdon.CharacterSheets.Memo;
using CatsUdon.CharacterSheets.TextSheets;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters.CSA;

public partial class CSAMagicalogiaAdapter(HttpClient httpClient) : ICharacterSheetAdapter
{
    [GeneratedRegex(@"^https:\/\/character-sheets\.appspot\.com\/mglg\/edit\.html\?key=(?<key>[\w_\-]+)$")]
    private static partial Regex UrlMatchRegex { get; }

    [GeneratedRegex(@"^skills\.row(?<row>\d+)\.name(?<column>\d+)$")]
    private static partial Regex SkillRcRegex { get; }

    [GeneratedRegex(@"[ヶ々〆〇〻㐂-頻]+")]
    private static partial Regex SkillNameRegex { get; }

    private static Lazy<GameSystemInfo[]> supportedSystems = new([
        new GameSystemInfo()
        {
            ProviderName = "キャラクターシート倉庫",
            ProviderHomePageUrl = new Uri("https://character-sheets.appspot.com/"),
            GameSystemName = "マギカロギア",
            MainPageUrl = new Uri("https://character-sheets.appspot.com/mglg/"),
            CharacterCreationUrl = new Uri("https://character-sheets.appspot.com/mglg/edit.html"),
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

        var characterSheetData = await httpClient.GetAsync($"https://character-sheets.appspot.com/mglg/display?key={key}&ajax=1");
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

    private string CreateMemo(Character character, CharacterJson characterJson)
    {
        using var memoBuilder = new MemoBuilder();

        memoBuilder.BeginSize(12);
        memoBuilder.Header("蔵書");

        foreach (var librarySkill in characterJson.Library)
        {
            if (string.IsNullOrWhiteSpace(librarySkill.Name)) break;

            var isEmpty = string.IsNullOrWhiteSpace(librarySkill.Skill)
                && string.IsNullOrWhiteSpace(librarySkill.Target)
                && string.IsNullOrWhiteSpace(librarySkill.Cost)
                && string.IsNullOrWhiteSpace(librarySkill.Effect);

            if (isEmpty) continue;

            var skillNameMatch = SkillNameRegex.Match(librarySkill.Name);
            if (!skillNameMatch.Success)
            {
                memoBuilder.Text($"▷ 【{librarySkill.Name.Replace('\n', ' ').Trim()}】");
            }
            else
            {
                var nameParts = librarySkill.Name.Split(['\n', '/'], StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length == 1)
                {
                    memoBuilder.Text($"▷ {skillNameMatch.Value}");
                }
                else
                {
                    memoBuilder.Text($"▷ {skillNameMatch.Value} 【{nameParts[1].Trim()}】");
                }
            }


            memoBuilder
                .TextPrefixedOptional("  タイプ", librarySkill.Type)
                .IfNotEmpty(librarySkill.Skill, $"  特技：<b>{librarySkill.Skill}</b>")
                .TextPrefixedOptional("  目標", librarySkill.Target)
                .TextPrefixedOptional("  コスト", librarySkill.Cost);

            if (!string.IsNullOrWhiteSpace(librarySkill.Effect))
            {
                memoBuilder.NewLine().BeginMargin("2em").Text(librarySkill.Effect).EndMargin();
            }

            memoBuilder.NewLine(2);
        }

        memoBuilder.Header("関係");

        foreach (var (index, anchor) in characterJson.Anchor.Index())
        {
            if (string.IsNullOrWhiteSpace(anchor.Name)) continue;

            var nameParts = anchor.Name.Split('\n');
            if (nameParts.Length == 1)
            {
                memoBuilder.Text($"▷ {nameParts[0]}");
            }
            else
            {
                memoBuilder.Text($"▷ {nameParts[0]} 【{nameParts[1]}】");
            }

            memoBuilder.TextPrefixedOptional("  運命", anchor.Destiny)
                .TextPrefixedOptional("  属性", anchor.Attribute);

            if (!string.IsNullOrWhiteSpace(anchor.Memo))
            {
                memoBuilder.NewLine().BeginMargin("2em").Text(anchor.Memo).EndMargin();
            }

            memoBuilder.NewLine(2);
        }

        if (!string.IsNullOrWhiteSpace(characterJson.Base.Memo))
        {
            memoBuilder.Header("設定")
                .Text(characterJson.Base.Memo);
        }

        memoBuilder.EndSize();

        return memoBuilder.ToString();
    }

    private void ReadCharacter(Character character, CharacterJson characterJson)
    {
        character.CoverName = characterJson.Base.CoverName ?? string.Empty;
        character.MagicName = characterJson.Base.MagicName ?? string.Empty;
        if (int.TryParse(characterJson.Magic.Max, out var maxMagic)) character.MaxMagic = maxMagic;
        if (int.TryParse(characterJson.Magic.Temp, out var tempMagic)) character.TempMagic = tempMagic;
        if (int.TryParse(characterJson.Magic.Value, out var currentMagic)) character.CurrentMagic = currentMagic;
        if (int.TryParse(characterJson.Base.Attack, out var attack)) character.Attack = attack;
        if (int.TryParse(characterJson.Base.Defense, out var defense)) character.Defense = defense;
        if (int.TryParse(characterJson.Base.Source, out var source)) character.Source = source;

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
        character.Library = characterJson.Library;
        var domainIndex = characterJson.Base.Domain switch
        {
            "a" => 0,
            "ab" => 1,
            "bc" => 2,
            "cd" => 3,
            "de" => 4,
            "e" => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(characterJson.Base.Domain), characterJson.Base.Domain, "Unknown value")
        };

        character.Domain = domainIndex;
    }

    private List<string> CreateTextSheets(Character character)
    {
        var sheets = new List<string>();

        var grid = new Grid();
        grid.Fill(DomainsAndSkills);
        grid.Columns[character.Domain] = true;

        for (int c = 0; c < DomainsAndSkills[0].Length; c++)
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

    private CCFoliaCharacterClipboardData ConvertToCCFoliaCharacter(Character character, string memo)
    {
        var ccfoliaCharacter = new CCFoliaCharacter
        {
            Name = character.CoverName,
            Memo = memo
        };

        ccfoliaCharacter.Status.Add(new CCFoliaStatus { Label = "魔力", Value = character.CurrentMagic, Max = character.MaxMagic });
        ccfoliaCharacter.Status.Add(new CCFoliaStatus { Label = "ー時魔力", Value = character.TempMagic, Max = 0 });

        ccfoliaCharacter.Params.Add(new CCFoliaParameter { Label = "攻撃力 ", Value = character.Attack.ToString() });
        ccfoliaCharacter.Params.Add(new CCFoliaParameter { Label = "防御力 ", Value = character.Attack.ToString() });
        ccfoliaCharacter.Params.Add(new CCFoliaParameter { Label = "根源力 ", Value = character.Attack.ToString() });

        // Skip 緊急召喚
        foreach (var librarySkill in character.Library.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(librarySkill.Name)) break;
            var skillNameMatch = SkillNameRegex.Match(librarySkill.Name);
            if (!skillNameMatch.Success) continue;

            var isEmpty = string.IsNullOrWhiteSpace(librarySkill.Skill)
                && string.IsNullOrWhiteSpace(librarySkill.Target)
                && string.IsNullOrWhiteSpace(librarySkill.Cost)
                && string.IsNullOrWhiteSpace(librarySkill.Effect);

            if (isEmpty) continue;

            ccfoliaCharacter.Status.Add(new CCFoliaStatus { Label = $"{skillNameMatch.Value}:{librarySkill.Cost ?? "なし"}", Value = 0, Max = character.Source });
        }

        return new CCFoliaCharacterClipboardData() { Data = ccfoliaCharacter };
    }

    public class CharacterJson
    {
        public required CharacterBase Base { get; set; }
        public Anchor[] Anchor { get; set; } = [];
        public LearnedSkill[] Learned { get; set; } = [];
        public required Magic Magic { get; set; }
        public LibrarySkill[] Library { get; set; } = [];
    }

    public class CharacterBase
    {
        public required string Domain { get; set; }
        [JsonPropertyName("covername")]
        public string? CoverName { get; set; }
        [JsonPropertyName("magicname")]
        public string? MagicName { get; set; }
        public string? Attack { get; set; }
        public string? Defense { get; set; }
        public string? Source { get; set; }
        public string? Memo { get; set; }
    }

    public class Anchor
    {
        public string? Attribute { get; set; }
        public string? Destiny { get; set; }
        public string? Name { get; set; }
        public string? Memo { get; set; }
    }

    public class LearnedSkill
    {
        public string? Id { get; set; }
        public string? Judge { get; set; }
    }

    public class LibrarySkill
    {
        public string? Cost { get; set; }
        public string? Effect { get; set; }
        public string? Name { get; set; }
        public string? Skill { get; set; }
        public string? Target { get; set; }
        public string? Type { get; set; }
    }

    public class Character
    {
        public string CoverName { get; set; } = string.Empty;
        public string MagicName { get; set; } = string.Empty;
        public List<(int row, int col)> Skills { get; set; } = [];
        public int Domain { get; set; }
        public int MaxMagic { get; set; }
        public int TempMagic { get; set; }
        public int CurrentMagic { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Source { get; set; }
        public LibrarySkill[] Library { get; set; } = [];
    }

    public class Magic
    {
        public string? Max { get; set; }
        public string? Temp { get; set; }
        public string? Value { get; set; }
    }

    private static readonly string[][] DomainsAndSkills = [
        ["星", "獣", "力", "歌", "夢", "闇"],
        ["黄金", "肉", "重力", "物語", "追憶", "深淵"],
        ["大地", "蟲", "風", "旋律", "謎", "腐敗"],
        ["森", "花", "流れ", "涙", "嘘", "裏切り"],
        ["道", "血", "水", "別れ", "不安", "迷い"],
        ["海", "鱗", "波", "微笑み", "眠り", "怠惰"],
        ["静寂", "混沌", "自由", "想い", "偶然", "歪み"],
        ["雨", "牙", "衝撃", "勝利", "幻", "不幸"],
        ["嵐", "叫び", "雷", "恋", "狂気", "バカ"],
        ["太陽", "怒り", "炎", "情熱", "祈り", "悪意"],
        ["天空", "翼", "光", "癒し", "希望", "絶望"],
        ["異界", "エロス", "円環", "時", "未来", "死"]
    ];
}
