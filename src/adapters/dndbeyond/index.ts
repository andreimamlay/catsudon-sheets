import axios from "axios";
import { Adapter, CanConvert, CCFoliaCharacter, ChatPalette, Convert } from "../types";
import { DndBeyondApiResponse, DndBeyondCharacterData, FeatureDefinition, Modifier, ModifierSubType, ModifierType } from "./types";

const characterSheetRegex = /^https\:\/\/www\.dndbeyond\.com\/characters\/(\d+)$/;

const canConvert: CanConvert = (characterSheetUrl: string) => {
    return characterSheetRegex.test(characterSheetUrl);
}

export const convert: Convert = async (characterSheetUrl: string) => {
    const characterSheetData = await download(characterSheetUrl);
    const characterSheetResponse = JSON.parse(characterSheetData) as DndBeyondApiResponse;
    if (!characterSheetResponse.data) {
        throw new Error("Could not read character sheet data");
    }

    const dndBeyondCharacterData = characterSheetResponse.data;

    const character: CCFoliaCharacter = {
        kind: "character",
        data: {
            name: dndBeyondCharacterData.name,
            initiative: 0,
            externalUrl: "",
            iconUrl: "",
            commands: "",
            status: [],
            params: []
        }
    };

    const chatPalette: ChatPalette = {
        attacks: [],
        abilityScores: [],
        savingThrows: [],
        skills: []
    };

    const features: [number, string, number][] = []
    for (const trait of dndBeyondCharacterData.race.racialTraits) {
        features.push([trait.definition.id, trait.definition.name, 1]);
    }

    const characterLevel = dndBeyondCharacterData.classes.reduce((acc, characterClass) => acc + characterClass.level, 0);

    for (const characterClass of dndBeyondCharacterData.classes) {
        if (!characterClass.isStartingClass) continue;

        for (const feature of characterClass.classFeatures) {
            if (feature.definition.requiredLevel > characterClass.level) continue;
            features.push([feature.definition.id, feature.definition.name, feature.definition.requiredLevel]);
        }
    }

    console.log(features);

    // readStatus(dndBeyondCharacterData, character, chatPalette);
    
    return character;
}

async function download(characterSheetUrl: string) {
    const urlMatch = characterSheetUrl.match(characterSheetRegex);
    if (!urlMatch) {
        throw new Error("Invalid character sheet URL");
    }
    
    const characterSheetJsonUrl = `https://character-service.dndbeyond.com/character/v5/character/${urlMatch[1]}`;
    const response = await axios.request({
        method: "GET",
        url: characterSheetJsonUrl,
        responseType: "text"
    });

    if (response.status !== 200) {
        throw new Error("Failed to load character sheet");
    }
    
    return response.data;
}

function readStatus(dndCharacter: DndBeyondCharacterData, character: CCFoliaCharacter, chatPalette: ChatPalette) {
    const abilityScores = {
        strength: dndCharacter.stats[0].value,
        dexterity: dndCharacter.stats[1].value,
        constitution: dndCharacter.stats[2].value,
        intelligence: dndCharacter.stats[3].value,
        wisdom: dndCharacter.stats[4].value,
        charisma: dndCharacter.stats[5].value,
    };

    const modifiers = [
        ...dndCharacter.modifiers.race, 
        ...dndCharacter.modifiers.class,
        ...dndCharacter.modifiers.background,
        ...dndCharacter.modifiers.item,
        ...dndCharacter.modifiers.feat
    ];

    abilityScores.strength += readModifiers(modifiers, "bonus", "strength-score");
    abilityScores.dexterity += readModifiers(modifiers, "bonus", "dexterity-score");
    abilityScores.constitution += readModifiers(modifiers, "bonus", "constitution-score");
    abilityScores.intelligence += readModifiers(modifiers, "bonus", "intelligence-score");
    abilityScores.wisdom += readModifiers(modifiers, "bonus", "wisdom-score");
    abilityScores.charisma += readModifiers(modifiers, "bonus", "charisma-score");

    const abilityModifiers = {
        strength: abilityScoreToModifier(abilityScores.strength),
        dexterity: abilityScoreToModifier(abilityScores.dexterity),
        constitution: abilityScoreToModifier(abilityScores.constitution),
        intelligence: abilityScoreToModifier(abilityScores.intelligence),
        wisdom: abilityScoreToModifier(abilityScores.wisdom),
        charisma: abilityScoreToModifier(abilityScores.charisma)
    };

    const savingThrows = { ...abilityModifiers };
    savingThrows.strength += readModifiers(modifiers, "proficiency", "strength-saving-throws");
    savingThrows.dexterity += readModifiers(modifiers, "proficiency", "dexterity-saving-throws");
    savingThrows.constitution += readModifiers(modifiers, "proficiency", "constitution-saving-throws");
    savingThrows.intelligence += readModifiers(modifiers, "proficiency", "intelligence-saving-throws");
    savingThrows.wisdom += readModifiers(modifiers, "proficiency", "wisdom-saving-throws");
    savingThrows.charisma += readModifiers(modifiers, "proficiency", "charisma-saving-throws");
}

function readModifiers<T extends ModifierType>(modifiers: Modifier[], type: T, subType: ModifierSubType<T>) {
    let value = 0;
    for (const modifier of modifiers) {
        if (modifier.type === type && modifier.subType === subType) {
            if (modifier.type === "proficiency") {
                return 2;
            }
            
            value += modifier.value;
        }
    }

    return value;
}


function abilityScoreToModifier(score: number) {
    return Math.floor((score - 10) / 2);
}

export default {
    canConvert,
    convert
} satisfies Adapter;
