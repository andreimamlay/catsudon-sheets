export type Adapter = {
    canConvert: CanConvert;
    convert: Convert;
}

export type CanConvert = (characterSheetUrl: string) => boolean;
export type Convert = (characterSheetUrl: string) => Promise<CCFoliaCharacter>;

export type CCFoliaCharacter = {
    kind: "character";
    data: CCFoliaCharacterData
}

export type CCFoliaCharacterData = {
    name: string;
    initiative: number;
    externalUrl: string;
    iconUrl: string;
    commands: string;
    status: CCFoliaStatus[];
    params: CCFoliaParam[];
}

export type CCFoliaStatus = {
    label: string;
    value: number;
    max: number;
}

export type CCFoliaParam = {
    label: string;
    value: string;
}

export type ChatPalette = {
    initiative?: string;
    hitDice?: string;
    attacks: string[];
    abilityScores: string[];
    savingThrows: string[];
    skills: string[];
}