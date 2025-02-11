export type CharacterSheetAdapter = (characterSheetData: string) => Character;
export type CharacterSheetDownloader = (characterSheetUrl: string) => Promise<string>;

export type Character = {
    kind: "character";
    data: CharacterData
}

export type CharacterData = {
    name: string;
    initiative: number;
    externalUrl: string;
    iconUrl: string;
    commands: string;
    status: Status[];
    params: Param[];
}

export type Status = {
    label: string;
    value: number;
    max: number;
}

export type Param = {
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