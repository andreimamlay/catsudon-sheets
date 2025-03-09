namespace CatsUdon.CharacterSheets.CCFolia;

public class CCFoliaCharacterClipboardData
{
    public string Kind => "character";
    public CCFoliaCharacter Data { get; set; } = new();
}
