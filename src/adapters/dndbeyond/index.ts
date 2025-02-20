import axios from "axios";
import { Adapter, CanConvert, Character, ChatPalette, Convert } from "../types";

const characterSheetRegex = /^https\:\/\/www\.dndbeyond\.com\/characters\/(\d+)$/;

const canConvert: CanConvert = (characterSheetUrl: string) => {
    return characterSheetRegex.test(characterSheetUrl);
}

export const convert: Convert = async (characterSheetUrl: string) => {
    const characterSheetData = await download(characterSheetUrl);
    const characterSheetResponse = JSON.parse(characterSheetData) as ApiResponse;
    const characterSheet = characterSheetResponse.data;

    const character: Character = {
        kind: "character",
        data: {
            name: characterSheet.name,
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

    readStatus(characterSheet, character, chatPalette);
    
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

function readStatus(characterSheet: Data, character: Character, chatPalette: ChatPalette) {
    // maxhp = base + (total level) * con modifier
    
    character.data.status.push({
        label: "HP",
        value: isNaN(currentHp) ? maxHp : currentHp,
        max: maxHp
    });

    character.data.status.push({
        label: "ー時HP",
        value: isNaN(tempHp) ? 0 : tempHp,
        max: 0
    });
    
    for (const stat of characterSheet.stats) {
        switch (stat.id) {
            case 1:
        }
    }
}

export default {
    canConvert,
    convert
} satisfies Adapter;
