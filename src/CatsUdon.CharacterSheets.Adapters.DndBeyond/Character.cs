using CatsUdon.CharacterSheets;

namespace CatsUdon.CharacterSheets.Adapters.DndBeyond;

internal class Character
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public Die[] HitDice { get; set; } = [];

    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int TemporaryHp { get; set; }
    public int ArmorClass { get; set; }
    public int Inspiration { get; set; }
    public int PassivePerception { get; set; }

    public int StrengthScore { get; set; }
    public int StrengthModifier { get; set; }
    public int DexterityScore { get; set; }
    public int DexterityModifier { get; set; }
    public int ConstitutionScore { get; set; }
    public int ConstitutionModifier { get; set; }
    public int IntelligenceScore { get; set; }
    public int IntelligenceModifier { get; set; }
    public int WisdomScore { get; set; }
    public int WisdomModifier { get; set; }
    public int CharismaScore { get; set; }
    public int CharismaModifier { get; set; }

    public int AcrobaticsModifier { get; set; }
    public int AnimalHandlingModifier { get; set; }
    public int ArcanaModifier { get; set; }
    public int AthleticsModifier { get; set; }
    public int DeceptionModifier { get; set; }
    public int HistoryModifier { get; set; }
    public int InsightModifier { get; set; }
    public int IntimidationModifier { get; set; }
    public int InvestigationModifier { get; set; }
    public int MedicineModifier { get; set; }
    public int NatureModifier { get; set; }
    public int PerceptionModifier { get; set; }
    public int PerformanceModifier { get; set; }
    public int PersuasionModifier { get; set; }
    public int ReligionModifier { get; set; }
    public int SleightOfHandModifier { get; set; }
    public int StealthModifier { get; set; }
    public int SurvivalModifier { get; set; }

    public int StrengthSavingThrowModifier { get; set; }
    public int DexteritySavingThrowModifier { get; set; }
    public int ConstitutionSavingThrowModifier { get; set; }
    public int IntelligenceSavingThrowModifier { get; set; }
    public int WisdomSavingThrowModifier { get; set; }
    public int CharismaSavingThrowModifier { get; set; }

    public List<Attack> Attacks { get; set; } = [];
}

internal enum Abilities
{
    Strength = 1,
    Dexterity = 2,
    Constitution = 3,
    Intelligence = 4,
    Wisdom = 5,
    Charisma = 6
}

internal class Attack
{
    public required string Name { get; set; }
    public Modifier? AttackBonus { get; set; }
    public Die Damage { get; set; }
}
