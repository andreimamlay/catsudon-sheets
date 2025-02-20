type ApiResponse = {
    id: number;
    success: boolean;
    message: string;
    data?: Data
}

type Data = {
    username: string;
    readonlyUrl: string;
    name: string;
    baseHitPoints: number;
    temporaryHitPoints: number;
    stats: Stat[];
    inventory: Equipment[];
    classes: Class[];
    modifiers: {
        race: Modifier[];
        class: Modifier[];
        background: Modifier[];
        item: Modifier[];
        feat: Modifier[];
        condition: Modifier[];
    }
}

type Stat = {
    id: number;
    value: number;
}

type Equipment = {
    definition: EquipmentDefinition;
}   

type EquipmentDefinition = {
    name: string;
    damage: Damage | null;
}

type Class = {
    level: number;
    definition: ClassDefinition;
}

type ClassDefinition = {
    hitDice: number;
    spellRules: SpellRules | null;
}

type SpellRules = {
    levelSpellSlots: number[][]; // classLevel[slotLevel], level = index
}

type Damage = {
    diceCount: number;
    diceValue: number;
    fixedValue: number | null;
    diceString: string;
}

type Modifier = ScoreBonusModifier | ProficiencyModifier;

type ScoreBonusModifier = {
    type: "bonus";
    subType: "intelligence-score";
    value: number;
}

type ProficiencyModifier = {
    type: "proficiency";
    subType: ProficiencySubType;
}

type ProficiencySubType = SkillProficiencySubType | SavingThrowProficiencySubType;
type SkillProficiencySubType = "arcana" | "history" | "nature" | "religion" | "investigation";
type SavingThrowProficiencySubType = "intelligence-saving-throws" | "wisdom-saving-throws";