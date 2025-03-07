namespace CatsUdon.CharacterSheets.Adapters.DndJp;
public class Character
{
    public string Name { get; set; } = string.Empty;

    public Modifier Initiative { get; set; }
    public Die? HitDice { get; set; }
    public List<Attack> Attacks { get; set; } = [];

    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int TemporaryHp { get; set; }
    public int ArmorClass { get; set; }
    public int Inspiration { get; set; }
    public int PassivePerception { get; set; }

    public List<(Ability ability, Modifier modifier)> AbilityScores { get; set; } = [];
    public List<(Ability ability, Modifier modifier)> SavingThrows { get; set; } = [];
    public List<(Skill skill, Die die)> Skills { get; set; } = [];
    public Dictionary<int, (int used, int total)> SpellSlots { get; set; } = [];
    
}

public class Attack
{
    public string Name { get; set; } = string.Empty;
    public Modifier? AttackBonus { get; set; }
    public Die Damage { get; set; }
}

public enum Ability
{
    /// <summary>
    /// 筋力
    /// </summary>
    Strength,
    /// <summary>
    /// 敏捷力
    /// </summary>
    Dexterity,
    /// <summary>
    /// 耐久力
    /// </summary>
    Constitution,
    /// <summary>
    /// 知力
    /// </summary>
    Intelligence,
    /// <summary>
    /// 判断力
    /// </summary>
    Wisdom,
    /// <summary>
    /// 魅力
    /// </summary>
    Charisma
}

public enum Skill
{
    /// <summary>
    /// 軽業
    /// </summary>
    Acrobatics,
    /// <summary>
    /// 動物使い
    /// </summary>
    AnimalHandling,
    /// <summary>
    /// 魔法学
    /// </summary>
    Arcana,
    /// <summary>
    /// 運動
    /// </summary>
    Athletics,
    /// <summary>
    /// ペテン
    /// </summary>
    Deception,
    /// <summary>
    /// 歴史
    /// </summary>
    History,
    /// <summary>
    /// 看破
    /// </summary>
    Insight,
    /// <summary>
    /// 威圧
    /// </summary>
    Intimidation,
    /// <summary>
    /// 捜査
    /// </summary>
    Investigation,
    /// <summary>
    /// 医術
    /// </summary>
    Medicine,
    /// <summary>
    /// 自然
    /// </summary>
    Nature,
    /// <summary>
    /// 知覚
    /// </summary>
    Perception,
    /// <summary>
    /// 芸能
    /// </summary>
    Performance,
    /// <summary>
    /// 説得
    /// </summary>
    Persuasion,
    /// <summary>
    /// 宗教
    /// </summary>
    Religion,
    /// <summary>
    /// 手先の早業
    /// </summary>
    SleightOfHand,
    /// <summary>
    /// 隠密
    /// </summary>
    Stealth,
    /// <summary>
    /// 生存
    /// </summary>
    Survival
}
