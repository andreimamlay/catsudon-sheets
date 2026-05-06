namespace CatsUdon.CharacterSheets.CCFolia;

public class CCFoliaCharacter
{
    public string Name { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
    public int Initiative { get; set; }
    public string ExternalUrl { get; set; } = string.Empty;
    public List<CCFoliaStatus> Status { get; set; } = [];
    public List<CCFoliaParameter> Params { get; set; } = [];

    public string IconUrl { get; set; } = string.Empty;
    public string Commands { get; set; } = string.Empty;
}
