namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class SpellModifier
{
    public required string Type { get; set; }
    public required string SubType { get; set; }
    public required Dice Die { get; set; }
    public required AtHigherLevelsTable AtHigherLevels { get; set; }
}

internal class AtHigherLevelsTable
{
    public required HigherLevelDefinition[] HigherLevelDefinitions { get; set; }
}

internal class HigherLevelDefinition
{
    public int Level { get; set; }
    public HigherLevelDefinitionTypeIds TypeId { get; set; }
    public Dice? Dice { get; set; }
    public int? Value { get; set; }
}

internal enum HigherLevelDefinitionTypeIds
{
    AdditionalDamage = 15
}
