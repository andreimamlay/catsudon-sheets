using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.CCFolia;
using CatsUdon.CharacterSheets.TextSheets;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters.CSA;

public partial class CSAShinobigamiAdapter(HttpClient httpClient) : ICharacterSheetAdapter
{
    [GeneratedRegex(@"^https:\/\/character-sheets\.appspot\.com\/shinobigami\/edit\.html\?key=(?<key>[\w_\-]+)$")]
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

        var characterSheetData = await httpClient.GetAsync($"https://character-sheets.appspot.com/shinobigami/display?key={key}&ajax=1");
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
        foreach (var (index, ninjutsuSkill) in characterJson.Ninpou.Index())
        {
            parts.Clear();

            if (string.IsNullOrWhiteSpace(ninjutsuSkill.Name)) break;

            var isEmpty = string.IsNullOrWhiteSpace(ninjutsuSkill.Range)
                && string.IsNullOrWhiteSpace(ninjutsuSkill.TargetSkill)
                && string.IsNullOrWhiteSpace(ninjutsuSkill.Cost)
                && string.IsNullOrWhiteSpace(ninjutsuSkill.Effect);

            if (isEmpty) continue;
            
            parts.Add($"▷ 【{ninjutsuSkill.Name.Replace('\n', ' ').Trim()}】");
            parts.Add(Append("タイプ", ninjutsuSkill.Type));
            parts.Add(Append("指定特技", ninjutsuSkill.TargetSkill));
            parts.Add(Append("間合", ninjutsuSkill.Range));
            parts.Add(Append("コスト", ninjutsuSkill.Cost));

            if (!string.IsNullOrWhiteSpace(ninjutsuSkill.Effect)) parts.Add("\n" + ninjutsuSkill.Effect.Trim());

            if (firstLine)
            {
                firstLine = false;
                builder.Append("<size=16>■ 忍法</size>\n");
            }

            builder.Append(string.Join("  ", parts.Where(p => !string.IsNullOrWhiteSpace(p))));
            builder.Append('\n');
            if (index < characterJson.Ninpou.Length - 1)
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
        foreach (var (index, background) in characterJson.Background.Index())
        {
            parts.Clear();

            if (string.IsNullOrWhiteSpace(background.Name)) continue;

            parts.Add($"▷ {background.Name}");

            parts.Add(Append("種別", background.Type));
            parts.Add(Append("功績点", background.Point));
            if (!string.IsNullOrWhiteSpace(background.Effect)) parts.Add("\n" + background.Effect.Trim());

            if (firstLine)
            {
                firstLine = false;
                builder.Append("<size=16>■ 背景</size>\n");
            }

            builder.Append(string.Join("  ", parts.Where(p => !string.IsNullOrWhiteSpace(p))));
            builder.Append('\n');
            if (index < characterJson.Background.Length - 1)
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
        character.Name = characterJson.Base.Name ?? string.Empty;
        character.NameKana = characterJson.Base.NameKana ?? string.Empty;

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
        character.Ninpou = characterJson.Ninpou;
        var styleIndex = characterJson.Base.Upperstyle switch
        {
            "a" => 0,
            "ab" => 1,
            "bc" => 2,
            "cd" => 3,
            "de" => 4,
            "e" => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(characterJson.Base.Upperstyle), characterJson.Base.Upperstyle, "Unknown value")
        };

        character.Style = styleIndex;
    }

    private List<string> CreateTextSheets(Character character)
    {
        var sheets = new List<string>();

        var grid = new Grid();
        grid.Fill(StyleAndSkills);
        grid.Columns[character.Style] = true;

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

    private CCFoliaCharacterClipboardData ConvertToCCFoliaCharacter(Character character, string memo)
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

        return new CCFoliaCharacterClipboardData() { Data = ccfoliaCharacter };
    }

    public class CharacterJson
    {
        public required CharacterBase Base { get; set; }
        public LearnedSkill[] Learned { get; set; } = [];
        public NinjutsuSkill[] Ninpou { get; set; } = [];
        public Background[] Background {  get; set; } = [];
    }

    public class CharacterBase
    {
        public required string Upperstyle { get; set; }
        public string? Name { get; set; }
        public string? NameKana { get; set; }
        public string? Memo { get; set; }
    }

    public class LearnedSkill
    {
        public string? Id { get; set; }
        public string? Judge { get; set; }
    }

    public class NinjutsuSkill
    {
        public string? Cost { get; set; }
        public string? Effect { get; set; }
        public string? Name { get; set; }
        public string? Range { get; set; }
        public string? TargetSkill { get; set; }
        public string? Type { get; set; }
    }

    public class Background
    {
        public string? Name { get; set; }
        public string? Effect { get; set; }
        public string? Point { get; set; }
        public string? Type { get; set; }
    }

    public class Character
    {
        public string Name { get; set; } = string.Empty;
        public string NameKana { get; set; } = string.Empty;
        public List<(int row, int col)> Skills { get; set; } = [];
        public int Style { get; set; }
        public NinjutsuSkill[] Ninpou { get; set; } = [];
    }

    private static readonly string[][] StyleAndSkills = [
        ["器術", "体術", "忍術", "謀術", "戦術", "妖術"],
        ["絡繰術", "騎乗術", "生存術", "医術", "兵糧術", "異形化"],
        ["火術", "砲術", "潜伏術", "毒術", "鳥獣術", "召喚術"],
        ["水術", "手裏剣術", "遁走術", "罠術", "野戦術", "死霊術"],
        ["針術", "手練", "盗聴術", "調査術", "地の利", "結界術"],
        ["仕込み", "身体操術", "腹話術", "詐術", "意気", "封術"],
        ["衣装術", "歩法", "隠形術", "対人術", "用兵術", "言霊術"],
        ["縄術", "走法", "変装術", "遊芸", "記憶術", "幻術"],
        ["登術", "飛術", "香術", "九ノ一の術", "見敵術", "瞳術"],
        ["拷問術", "骨法術", "分身の術", "傀儡の術", "暗号術", "千里眼の術"],
        ["壊器術", "刀術", "隠蔽術", "流言の術", "伝達術", "憑依術"],
        ["掘削術", "怪力", "第六感", "経済力", "人脈", "呪術"]
    ];
}
