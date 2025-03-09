namespace CatsUdon.CharacterSheets.Adapters.DndJp;
public static class Translate
{
    public static string ToJapaneese(Ability ability)
    {
        return ability switch
        {
            Ability.Strength => "筋力",
            Ability.Dexterity => "敏捷力",
            Ability.Constitution => "耐久力",
            Ability.Intelligence => "知力",
            Ability.Wisdom => "判断力",
            Ability.Charisma => "魅力",
            _ => throw new ArgumentOutOfRangeException(nameof(ability), ability, null)
        };
    }
    public static string ToJapaneese(Skill skill)
    {
        return skill switch
        {
            Skill.Acrobatics => "軽業",
            Skill.AnimalHandling => "動物使い",
            Skill.Arcana => "魔法学",
            Skill.Athletics => "運動",
            Skill.Deception => "ペテン",
            Skill.History => "歴史",
            Skill.Insight => "看破",
            Skill.Intimidation => "威圧",
            Skill.Investigation => "捜査",
            Skill.Medicine => "医術",
            Skill.Nature => "自然",
            Skill.Perception => "知覚",
            Skill.Performance => "芸能",
            Skill.Persuasion => "説得",
            Skill.Religion => "宗教",
            Skill.SleightOfHand => "手先の早業",
            Skill.Stealth => "隠密",
            Skill.Survival => "生存",
            _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
        };
    }
}
