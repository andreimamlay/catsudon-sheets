using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class Modifier
{
    public int? FixedValue { get; set; }
    public required string Type { get; set; }
    public required string SubType { get; set; }
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