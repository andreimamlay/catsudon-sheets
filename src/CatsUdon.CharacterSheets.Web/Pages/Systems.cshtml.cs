using CatsUdon.CharacterSheets.Adapters.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CatsUdon.CharacterSheets.Web.Pages;

public class SystemsModel(IEnumerable<ICharacterSheetAdapter> adapters) : PageModel
{
    public IGrouping<(string providerName, Uri homepageUrl), GameSystemInfo>[] SupportedSystems { get; set; } = [];

    public void OnGet()
    {
        SupportedSystems = adapters.SelectMany(adapter => adapter.SupportedGameSystems)
            .GroupBy(s => (s.ProviderName, s.ProviderHomePageUrl))
            .ToArray(); 
    }
}
