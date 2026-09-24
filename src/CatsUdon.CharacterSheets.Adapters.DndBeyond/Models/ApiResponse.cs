namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class ApiResponse<TData>
{
    public bool Success { get; set; }
    public TData? Data { get; set; }
}
