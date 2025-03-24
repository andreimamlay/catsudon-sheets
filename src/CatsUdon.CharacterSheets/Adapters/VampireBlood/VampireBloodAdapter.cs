using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CatsUdon.CharacterSheets.Adapters.Abstractions;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters.VampireBlood;

public partial class VampireBloodAdapter(HttpClient httpClient) : ICharacterSheetAdapter
{
    [GeneratedRegex(@"^https://charasheet\.vampire\-blood\.net/(?<id>[\w\d]+)$")]
    private static partial Regex UrlMatchRegex { get; }

    [GeneratedRegex(@"^https://charasheet\.vampire\-blood\.net/list_(?<systemName>.+)$")]
    private static partial Regex SystemNameRegex { get; }

    private const string HeaderCharacterListCssSelector = "li.nav-link:nth-child(2) > a:nth-child(1)";

    private static Lazy<GameSystemInfo[]> supportedSystems = new([
        new GameSystemInfo()
        {
            ProviderName = "キャラクター保管所",
            ProviderHomePageUrl = new Uri("https://charasheet.vampire-blood.net/"),
            GameSystemName = " 永い後日談のネクロニカ",
            MainPageUrl = new Uri("https://charasheet.vampire-blood.net/list_nechro.html"),
            CharacterCreationUrl = new Uri("https://charasheet.vampire-blood.net/nechro_pc_making.html"),
        }
    ]);
    public GameSystemInfo[] SupportedGameSystems => supportedSystems.Value;

    public bool CanConvert(string url) => UrlMatchRegex.IsMatch(url);

    public async Task<CharacterSheet> Convert(string url)
    {
        if (!CanConvert(url))
        {
            throw new ArgumentException("URL is not supported", nameof(url));
        }

        var match = UrlMatchRegex.Match(url);
        var id = match.Groups["id"].Value;

        var getHtmlResponse = await httpClient.GetAsync($"https://charasheet.vampire-blood.net/{id}");
        getHtmlResponse.EnsureSuccessStatusCode();

        var parser = new HtmlParser();
        var document = parser.ParseDocument(await getHtmlResponse.Content.ReadAsStreamAsync());

        var characterListElement = document.QuerySelector(HeaderCharacterListCssSelector);
        if (characterListElement is null || characterListElement is not IHtmlAnchorElement anchorElement)
        {
            throw new InvalidOperationException("Could not find character sheet link");
        }

        var href = anchorElement.Href;
        if (string.IsNullOrWhiteSpace(href))
        {
            throw new InvalidOperationException("Could not find character sheet link");
        }

        var systemNameMatch = SystemNameRegex.Match(href);
        if (!systemNameMatch.Success)
        {
            throw new InvalidOperationException("Could not find system name");
        }

        var systemName = systemNameMatch.Groups["systemName"].Value;

        switch (systemName)
        {
            case "nechro":
                return NechronicaConverter.Convert(document);
            default:
                throw new InvalidOperationException($"System {systemName} not supported");
        }
    }
}
