export type DndBeyondApiResponse = {
    id: number;
    success: boolean;
    message: string;
    data?: DndBeyondCharacterData
}

export type DndBeyondCharacterData = {
    username: string;
    readonlyUrl: string;
    name: string;
    baseHitPoints: number;
    temporaryHitPoints: number;
    stats: [Stat<1>, Stat<2>, Stat<3>, Stat<4>, Stat<5>, Stat<6>];
    race: Race;
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

export type Stat<id> = {
    id: id;
    value: number;
}

export type Race = {
    racialTraits: RacialTrait[];
}

export type RacialTrait = {
    definition: TraitDefinition;
}

export type TraitDefinition = {
    id: number;
    name: string;
}

export type Equipment = {
    definition: EquipmentDefinition;
}   

export type EquipmentDefinition = {
    name: string;
    damage: Damage | null;
}

export type Class = {
    level: number;
    isStartingClass: boolean;
    definition: ClassDefinition;
    classFeatures: ClassFeature[];
}

export type ClassDefinition = {
    hitDice: number;
    spellRules: SpellRules | null;
}

export type SpellRules = {
    levelSpellSlots: number[][]; // classLevel[slotLevel], level = index
}

export type ClassFeature = {
    definition: FeatureDefinition;
}

export type FeatureDefinition = {
    id: number;
    name: string;
    requiredLevel: number;
}

export type Damage = {
    diceCount: number;
    diceValue: number;
    fixedValue: number | null;
    diceString: string;
}

export type Modifier = ScoreBonusModifier | ProficiencyModifier;
export type ModifierType = Modifier["type"];
export type ModifierSubType<T extends ModifierType> = T extends "bonus" ? ScoreBonusSubType : ProficiencySubType;

export type ScoreBonusModifier = {
    type: "bonus";
    subType: ScoreBonusSubType;
    value: number;
}

export type ScoreBonusSubType = "strength-score" | "dexterity-score" | "constitution-score" | "intelligence-score" | "wisdom-score" | "charisma-score";

export type ProficiencyModifier = {
    type: "proficiency";
    subType: ProficiencySubType;
}

export type ProficiencySubType = SkillProficiencySubType | SavingThrowProficiencySubType;
export type SkillProficiencySubType = "arcana" | "history" | "nature" | "religion" | "investigation";
export type SavingThrowProficiencySubType = "strength-saving-throws" | "dexterity-saving-throws" | "constitution-saving-throws" | "intelligence-saving-throws" | "wisdom-saving-throws" | "charisma-saving-throws";