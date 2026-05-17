namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class CharacterModifier
{
    public int? FixedValue { get; set; }
    public required string Type { get; set; }
    public required string SubType { get; set; }
    public int ComponentId { get; set; }
}

public enum ModifierType
{
    Bonus,
    Set,
    SetBase,
    Proficiency,
    Immuntiy,
    Advantage,
    Language,
    Expertise,
    Resistance,
    Vulnerability,
    Disadvantage,
    Override
}