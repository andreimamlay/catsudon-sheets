namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class SpellDefinition
{
    public required string Name { get; set; }
    public int Level { get; set; }
    public SpellModifier[] Modifiers { get; set; } = [];
    public StatIds? SaveDcAbilityId { get; set; }
    public bool RequiresSavingThrow { get; set; }
    public bool RequiresAttackRoll { get; set; }
}
