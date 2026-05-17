using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class EquipmentDefinition
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public bool CanAttune { get; set; }
    public int? ArmorClass { get; set; }
    public StealthCheckTypes? StealthCheck { get; set; }
    public ArmorTypes? ArmorTypeId { get; set; }
    public AttackType? AttackType { get; set; }
    public WeaponCategories? CategoryId { get; set; }
    public Dice? Damage { get; set; }
    public required EquipmentProperty[] Properties { get; set; }
}

internal enum StealthCheckTypes
{
    None = 1,
    Disadvantage = 2
}

internal enum ArmorTypes
{
    Light = 1,
    Medium = 2,
    Heavy = 3,
    Shield = 4
}

internal enum AttackType
{
    Melee = 1,
    Ranged = 2
}

internal enum WeaponCategories
{
    Simple = 1,
    Martial = 2,
    Firearms = 3
}