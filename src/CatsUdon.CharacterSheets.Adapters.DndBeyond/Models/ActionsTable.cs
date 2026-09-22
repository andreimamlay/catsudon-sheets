namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class ActionsTable
{
    public CharacterAction[] Race { get; set; } = [];
    public CharacterAction[] Class { get; set; } = [];
    public CharacterAction[]? Background { get; set; } = [];
    public CharacterAction[]? Item { get; set; } = [];
    public CharacterAction[] Feat { get; set; } = [];
}
