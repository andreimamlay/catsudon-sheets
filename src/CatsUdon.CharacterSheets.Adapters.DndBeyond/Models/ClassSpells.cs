namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class ClassSpells
{
    public required int CharacterClassId { get; set; }
    public required Spell[] Spells { get; set; }
}
