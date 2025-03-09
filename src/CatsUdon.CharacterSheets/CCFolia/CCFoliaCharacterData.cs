namespace CatsUdon.CharacterSheets.CCFolia;

public class CCFoliaCharacterData
{
    public string Name { get; set; } = string.Empty;
    public int Initiative { get; set; }
    public string ExternalUrl { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string Commands { get; set; } = string.Empty;
    public List<CCFoliaStatus> Status { get; set; } = [];
    public List<CCFoliaParameter> Params { get; set; } = [];
}
