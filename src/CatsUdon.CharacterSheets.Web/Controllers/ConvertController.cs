using CatsUdon.CharacterSheets.Adapters.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CatsUdon.CharacterSheets.Web.Controllers;

[ApiController]
public class ConvertController(IEnumerable<ICharacterSheetAdapter> adapters) : ControllerBase
{
    [HttpGet("/api/convert/")]
    public async Task<IActionResult> Convert([FromQuery] string url)
    {
        foreach (var adapter in adapters)
        {
            if (adapter.CanConvert(url))
            {
                var characterSheet = await adapter.Convert(url);
                return Ok(characterSheet);
            }
        }

        return Ok(":(");
    }

    [HttpGet("/api/convert/sakura/{id}")]
    public async Task<IActionResult> Convert(int id)
    {
        var url = $"https://dndjp.sakura.ne.jp/OUTPUT.php?ID={id}";

        foreach (var adapter in adapters)
        {
            if (adapter.CanConvert(url))
            {
                var characterSheet = await adapter.Convert(url);
                return Ok(characterSheet.CCFoliaCharacter);
            }
        }

        return Ok(":(");
    }
}
