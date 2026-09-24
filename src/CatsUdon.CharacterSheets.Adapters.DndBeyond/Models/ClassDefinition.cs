namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class ClassDefinition
{
    public required string Name { get; set; }
    public required int HitDice { get; set; }
    public StatIds? SpellCastingAbilityId { get; set; }
    public required SpellRules SpellRules { get; set; }
}

internal class SpellRules
{
    public int MultiClassSpellSlotDivisor { get; set; }
    public MultiClassSpellSlotRounding MultiClassSpellSlotRounding { get; set; }
    public int[][] LevelSpellSlots { get; set; } = [];
}

internal enum MultiClassSpellSlotRounding
{
    RoundDown = 1,
    RoundUp = 2
}
