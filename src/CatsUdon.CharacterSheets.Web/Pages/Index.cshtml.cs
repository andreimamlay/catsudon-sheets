using CatsUdon.CharacterSheets.Adapters.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CatsUdon.CharacterSheets.Web.Pages;

public class IndexModel(IEnumerable<ICharacterSheetAdapter> adapters, JsonSerializerOptions jsonSerializerOptions) : PageModel
{
    [BindProperty(Name = "url", SupportsGet = true)]
    public string? CharacterSheetUrl { get; set; }

    public string? CharacterSheetJson { get; set; }
    public List<string>? AdditionalTextSheets { get; set; }


    public async Task OnGet()
    {
        if (!String.IsNullOrWhiteSpace(CharacterSheetUrl))
        {
            foreach (var adapter in adapters)
            {
                if (adapter.CanConvert(CharacterSheetUrl))
                {
                    var character = await adapter.Convert(CharacterSheetUrl);
                    CharacterSheetJson = JsonSerializer.Serialize(character.CCFoliaCharacter, jsonSerializerOptions);
                    AdditionalTextSheets = character.AdditionalTextSheets;
                    
                    break;
                }
            }
        }
    }
}
