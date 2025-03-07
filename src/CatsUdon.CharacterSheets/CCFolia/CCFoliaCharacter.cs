namespace CatsUdon.CharacterSheets.CCFolia;

public class CCFoliaCharacter
{
    public string Kind => "character";
    public CCFoliaCharacterData Data { get; set; } = new();
}
