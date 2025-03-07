namespace CatsUdon.CharacterSheets.CCFolia;

public class CharacterData
{
    public string Name { get; set; } = string.Empty;
    public int Initiative { get; set; }
    public string ExternalUrl { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string Commands { get; set; } = string.Empty;
    public List<Status> Status { get; set; } = [];
    public List<Parameter> Params { get; set; } = [];
}
