namespace CatsUdon.CharacterSheets.Adapters.DndJp;
public static class CssSelectors
{
    public const string CharacterName = "table.DD5ePage:nth-child(5) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(1) > div:nth-child(2)";
    public const string Initiative = "table.DD5ePage:nth-child(6) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(1)";
    public const string MaxHp = "td.LBC:nth-child(1)";
    public const string CurrentHp = "td.LBC:nth-child(2)";
    public const string TempHp = "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(1)";
    public const string HitDice = "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(2)";
    public const string ArmorClass = "table.DD5ePage:nth-child(6) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(1)";
    public const string Inspiration = "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2)";
    public const string ProficiencyBonus = "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(4)";
    public const string AttacksExtra = "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(15) > td:nth-child(1) > div:nth-child(1)";
    public const string PassivePerception = "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(22) > td:nth-child(2)";

    public static readonly (Ability ability, string selector)[] AbilityScores = [
        (Ability.Strength, ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(3)"),
        (Ability.Dexterity, ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(3) > td:nth-child(3)"),
        (Ability.Constitution, ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(4) > td:nth-child(3)"),
        (Ability.Intelligence, ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(5) > td:nth-child(3)"),
        (Ability.Wisdom, ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(6) > td:nth-child(3)"),
        (Ability.Charisma, ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(3)")
    ];

    public static readonly (Ability ability, string selector)[] SavingThrows = [
        (Ability.Strength, "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(2)"),
        (Ability.Dexterity, "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(3) > td:nth-child(2)"),
        (Ability.Constitution, "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(4) > td:nth-child(2)"),
        (Ability.Intelligence, "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(5) > td:nth-child(2)"),
        (Ability.Wisdom, "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(6) > td:nth-child(2)"),
        (Ability.Charisma, "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(2)")
    ];

    public static readonly string[] Attacks = [
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(3)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(5)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(9)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(11)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(13)",
    ];

    public static readonly (Skill skill, string selector)[] Skills = [
        (Skill.Intimidation, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(3) > td:nth-child(1)"),
        (Skill.Medicine, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(4) > td:nth-child(1)"),
        (Skill.Athletics, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(5) > td:nth-child(1)"),
        (Skill.Stealth, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(6) > td:nth-child(1)"),
        (Skill.Acrobatics, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(1)"),
        (Skill.Insight, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(8) > td:nth-child(1)"),
        (Skill.Performance, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(9) > td:nth-child(1)"),
        (Skill.Nature, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(10) > td:nth-child(1)"),
        (Skill.Religion, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(11) > td:nth-child(1)"),
        (Skill.Survival, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(12) > td:nth-child(1)"),
        (Skill.Persuasion, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(13) > td:nth-child(1)"),
        (Skill.Investigation, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(14) > td:nth-child(1)"),
        (Skill.Perception, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(15) > td:nth-child(1)"),
        (Skill.SleightOfHand, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(1)"),
        (Skill.AnimalHandling, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(17) > td:nth-child(1)"),
        (Skill.Deception, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(18) > td:nth-child(1)"),
        (Skill.Arcana, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(19) > td:nth-child(1)"),
        (Skill.History, "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(20) > td:nth-child(1)")
    ];

    public static readonly (string total, string used)[] Slots = [
        (
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(3)",
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(5)"
        ),
        (
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(33) > td:nth-child(3)",
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(33) > td:nth-child(5)"
        ),
        (
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(3)",
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(5)"
        ),
        (
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(19) > td:nth-child(3)",
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(19) > td:nth-child(5)"
        ),
        (
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(35) > td:nth-child(3)",
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(35) > td:nth-child(5)"
        ),
        (
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(3)",
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(5)"
        ),
        (
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(3)",
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(5)"
        ),
        (
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(28) > td:nth-child(3)",
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(28) > td:nth-child(5)"
        ),
        (
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(39) > td:nth-child(3)",
            "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(39) > td:nth-child(5)"
        )
    ];
}
