using CatsUdon.CharacterSheets.Adapters.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CatsUdon.CharacterSheets.Web.Pages;

public class IndexModel(IEnumerable<ICharacterSheetAdapter> adapters, JsonSerializerOptions jsonSerializerOptions) : PageModel
{
    private static readonly Lazy<string> CommitHashProvider = new(ReadCommitHash);

    [BindProperty(Name = "url", SupportsGet = true)]
    public string? CharacterSheetUrl { get; set; }


    public string? ErrorMessage { get; set; }

    public string? CharacterName { get; set; }
    public string? CharacterSheetJson { get; set; }
    public List<string>? AdditionalTextSheets { get; set; }


    public async Task OnGet()
    {
        ViewData["CommitHash"] = CommitHashProvider.Value;

        if (!String.IsNullOrWhiteSpace(CharacterSheetUrl))
        {
            try
            {
                var converted = false;
                foreach (var adapter in adapters)
                {
                    if (adapter.CanConvert(CharacterSheetUrl))
                    {
                        var character = await adapter.Convert(CharacterSheetUrl);

                        CharacterName = character.Character.Data.Name;
                        ViewData["Title"] = CharacterName;

                        CharacterSheetJson = JsonSerializer.Serialize(character.Character, jsonSerializerOptions);
                        AdditionalTextSheets = character.AdditionalTextSheets;
                        converted = true;

                        break;
                    }
                }

                if (!converted)
                {
                    ErrorMessage = "URLをサポートしていません。";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "例外発生しました。";
            }
        }
    }

    private static string ReadCommitHash()
    {
        if (!System.IO.File.Exists("commit_hash"))
            return "No Info";

        var hash = System.IO.File.ReadAllText("commit_hash");
        return hash switch
        {
            { Length: > 8 } => hash[0..8],
            _ => hash
        };
    }
}
