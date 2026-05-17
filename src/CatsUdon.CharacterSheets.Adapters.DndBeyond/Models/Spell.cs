namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class Spell
{
    public required SpellDefinition Definition { get; set; }
    public bool Prepared { get; set; }
    public bool CountsAsKnownSpell { get; set; }
}
