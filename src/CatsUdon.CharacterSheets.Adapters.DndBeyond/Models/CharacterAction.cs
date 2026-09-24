namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class CharacterAction
{
    public required string Name { get; set; }
    public Dice? Dice { get; set; }
    public StatIds? SaveStatId { get; set; }
    public bool? DisplayAsAttack { get; set; }
}
