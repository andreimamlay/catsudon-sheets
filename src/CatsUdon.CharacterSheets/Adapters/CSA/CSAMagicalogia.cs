using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.CCFolia;
using CatsUdon.CharacterSheets.TextSheets;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters.CSA;

public partial class CSAMagicalogiaAdapter(HttpClient httpClient) : ICharacterSheetAdapter
{
    [GeneratedRegex(@"^https:\/\/character-sheets\.appspot\.com\/mglg\/edit\.html\?key=(?<key>[\w_\-]+)$")]
    private static partial Regex UrlMatchRegex { get; }

    [GeneratedRegex(@"^skills\.row(?<row>\d+)\.name(?<column>\d+)$")]
    private static partial Regex SkillRcRegex { get; }

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
        var builder = new StringBuilder();
        builder.Append("<size=12>");

        var parts = new List<string>();
        var firstLine = true;
        foreach (var (index, librarySkill) in characterJson.Library.Index())
        {
            parts.Clear();

            if (string.IsNullOrWhiteSpace(librarySkill.Name)) continue;

            var nameParts = librarySkill.Name.Split('\n');
            parts.Add($"▷ {nameParts[0]}");
            if (nameParts.Length > 1) parts.Add($"【{nameParts[1]}】");

            parts.Add(Append("特技", librarySkill.Skill));
            parts.Add(Append("目標", librarySkill.Target));
            parts.Add(Append("コスト", librarySkill.Cost));

            if (!string.IsNullOrWhiteSpace(librarySkill.Effect)) parts.Add("\n" + librarySkill.Effect.Trim());

            if (firstLine)
            {
                firstLine = false;
                builder.Append("<size=16>■ 蔵書</size>\n");
            }

            builder.Append(string.Join("  ", parts.Where(p => !string.IsNullOrWhiteSpace(p))));
            builder.Append('\n');
            if (index < characterJson.Library.Length - 1)
            {
                builder.Append('\n');
            }
        }

        if (!firstLine)
        {
            builder.Append('\n');
            builder.Append('\n');
        }

        firstLine = true;
        foreach (var (index, anchor) in characterJson.Anchor.Index())
        {
            parts.Clear();

            if (string.IsNullOrWhiteSpace(anchor.Name)) continue;

            var nameParts = anchor.Name.Split('\n');
            parts.Add($"▷ {nameParts[0]}");
            if (nameParts.Length > 1) parts.Add($"【{nameParts[1]}】");

            parts.Add(Append("運命", anchor.Destiny));
            parts.Add(Append("属性", anchor.Attribute));
            if (!string.IsNullOrWhiteSpace(anchor.Memo)) parts.Add("\n" + anchor.Memo.Trim());

            if (firstLine)
            {
                firstLine = false;
                builder.Append("<size=16>■ 関係</size>\n");
            }

            builder.Append(string.Join("  ", parts.Where(p => !string.IsNullOrWhiteSpace(p))));
            builder.Append('\n');
            if (index < characterJson.Library.Length - 1)
            {
                builder.Append('\n');
            }
        }

        if (!string.IsNullOrWhiteSpace(characterJson.Base.Memo))
        {
            builder.Append("<size=16>■ 設定</size>\n");
            builder.Append(characterJson.Base.Memo);
            builder.Append('\n');
        }

        builder.Append('\n');
        builder.Append('\n');

        builder.Append("</size>");

        return builder.ToString();

        static string Append(string prefix, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return $"{prefix}: {text}";
        }
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

        var grid = new Grid(DomainsAndSkills.Length, DomainsAndSkills[0].Length);
        grid.Fill(DomainsAndSkills);
        grid.Columns[character.Domain] = true;

        foreach (var skill in character.Skills)
        {
            grid.Cells[skill.row + 1, skill.col].Pushed = true;
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

        foreach (var librarySkill in character.Library)
        {
            var skillName = librarySkill.Name?.Split("\n")[0] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(skillName)) continue;
            if (librarySkill.Cost == "なし") continue;

            ccfoliaCharacter.Status.Add(new CCFoliaStatus { Label = skillName, Value = 0, Max = character.Source });
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
