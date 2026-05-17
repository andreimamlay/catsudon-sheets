namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class SpellsTable
{
    public Spell[] Race { get; set; } = [];
    public Spell[] Class { get; set; } = [];
    public Spell[] Background { get; set; } = [];
    public Spell[] Item { get; set; } = [];
    public Spell[] Feat { get; set; } = [];
}
