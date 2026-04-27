namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class Class
{
    public required int Level { get; set; }
    public bool IsStartingClass { get; set; }
    public required ClassDefinition Definition { get; set; }
}
