namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class ModifiersTable
{
    public CharacterModifier[] Race { get; set; } = [];
    public CharacterModifier[] Class { get; set; } = [];
    public CharacterModifier[] Background { get; set; } = [];
    public CharacterModifier[] Item { get; set; } = [];
    public CharacterModifier[] Feat { get; set; } = [];
}
