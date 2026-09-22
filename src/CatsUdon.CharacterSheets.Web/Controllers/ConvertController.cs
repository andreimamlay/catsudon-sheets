using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.CCFolia;
using Microsoft.AspNetCore.Mvc;

namespace CatsUdon.CharacterSheets.Web.Controllers;

[ApiController]
public class ConvertController(IEnumerable<ICharacterSheetAdapter> adapters) : ControllerBase
{
    [HttpGet("/api/convert/")]
    public async Task<ConvertCharacterSheetResponse> Convert([FromQuery] string url)
    {
        try
        {
            foreach (var adapter in adapters)
            {
                if (adapter.CanConvert(url))
                {
                    var characterSheet = await adapter.Convert(url);
                    return new ConvertCharacterSheetResponse()
                    {
                        Success = true,
                        Data = new CharacterSheetData()
                        {
                            CharacterSheet = characterSheet.Character,
                            AdditionalTextSheets = [.. characterSheet.AdditionalTextSheets]
                        }
                    };
                }
            }

            return new ConvertCharacterSheetResponse()
            {
                Success = false,
                ErrorCode = ErrorCodes.NotSupported,
                Error = "No converter found for the given URL."
            };
        }
        catch (Exception ex)
        {
            return new ConvertCharacterSheetResponse()
            {
                Success = false,
                ErrorCode = ErrorCodes.InternalError,
                Error = ex.Message
            };
        }
    }
}

public class ConvertCharacterSheetResponse
{
    public bool Success { get; set; }
    public ErrorCodes? ErrorCode { get; set; }
    public string? Error { get; set; }
    public CharacterSheetData? Data { get; set; }
}

public class CharacterSheetData
{
    public required CCFoliaCharacterClipboardData CharacterSheet { get; set; }
    public string[] AdditionalTextSheets { get; set; } = [];
}

public enum ErrorCodes
{
    InternalError = 1,
    NotSupported = 2
}