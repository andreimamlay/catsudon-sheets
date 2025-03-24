namespace CatsUdon.CharacterSheets;
public class GameSystemInfo
{
    public required string ProviderName { get; set; }
    public required Uri ProviderHomePageUrl { get; set; }
    public required string GameSystemName { get; set; }
    public required Uri MainPageUrl { get; set; }
    public Uri? CharacterCreationUrl { get; set; }
}
