namespace CatsUdon.CharacterSheets.CCFolia;

public class Character
{
    public string Kind => "character";
    public CharacterData Data { get; set; } = new();
}
