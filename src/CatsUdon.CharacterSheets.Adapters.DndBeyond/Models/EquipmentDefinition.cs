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
    public StealthCheck? StealthCheck { get; set; }
    public ArmorType? ArmorTypeId { get; set; }
    public AttackType? AttackType { get; set; }
    public CategoryId? CategoryId { get; set; }
    public Dice? Damage { get; set; }
    public required EquipmentProperty[] Properties { get; set; }
}

internal enum StealthCheck
{
    NoDisadvantage = 1,
    Disadvantage = 2
}

internal enum ArmorType
{
    Light = 1,
    Medium = 2,
    Heavy = 3,
    Shield = 4
}

internal enum AttackType
{
    MeleeWeapon = 1,
    RangedWeapon = 2,
    MeleeSpellAttack = 3,
    RangedSpellAttack = 4
}

internal enum CategoryId
{
    Simple = 1,
    Martial = 2
}