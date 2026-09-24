using System.Diagnostics;

namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

[DebuggerDisplay("{Definition.Name} (Attuned: {IsAttuned}, Equipped: {Equipped})")]
internal class Equipment
{
    public required EquipmentDefinition Definition { get; set; }
    public bool IsAttuned { get; set; }
    public bool Equipped { get; set; }
}
