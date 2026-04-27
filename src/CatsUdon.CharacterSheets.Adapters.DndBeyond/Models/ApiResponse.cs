using System.Text.Json.Serialization;

namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class ApiResponse
{
    public bool Success { get; set; }
    public Data? Data { get; set; }
}
