using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.CCFolia;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters.CharacterSheetsAppspot;

public partial class CSAMagicalogiaAdapter(HttpClient httpClient) : ICharacterSheetAdapter
{
    [GeneratedRegex(@"^https:\/\/character-sheets\.appspot\.com\/mglg\/edit\.html\?key=(?<key>\w+)$")]
    private static partial Regex UrlMatchRegex { get; }

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

        return new CharacterSheet()
        {
            Character = ConvertToCCFoliaCharacter(character),
            AdditionalTextSheets = CreateTextSheets(character),
        };
    }

    private void ReadCharacter(Character character, CharacterJson characterJson)
    {
        character.CoverName = characterJson.Base.CoverName ?? string.Empty;
        character.MagicName = characterJson.Base.MagicName ?? string.Empty;
    }

    private List<string> CreateTextSheets(Character character)
    {
        throw new NotImplementedException();
    }

    private CCFoliaCharacterClipboardData ConvertToCCFoliaCharacter(Character character)
    {
        var ccfoliaCharacter = new CCFoliaCharacter();
        ccfoliaCharacter.Name = character.CoverName;


        return new CCFoliaCharacterClipboardData() { Data = ccfoliaCharacter };
    }

    public class CharacterJson
    {
        public required CharacterBase Base { get; set; }
        public Anchor[] Anchor { get; set; } = [];
        public LearnedSkill[] Learned { get; set; } = [];
    }

    public class CharacterBase
    {
        public required string Domain { get; set; }
        [JsonPropertyName("covername")]
        public string? CoverName { get; set; }
        [JsonPropertyName("magicname")]
        public string? MagicName { get; set; }
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

    public class Character
    {
        public string CoverName { get; set; } = string.Empty;
        public string MagicName { get; set; } = string.Empty;
    }

}
