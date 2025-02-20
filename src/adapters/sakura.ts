import axios from "axios";
import Encoding from "encoding-japanese";
import { Adapter, CanConvert, CCFoliaCharacter, ChatPalette, Convert } from "./types";
import * as cheerio from "cheerio"

const decoder = new TextDecoder();

const canConvert: CanConvert = (url: string): boolean => {
    return /^https\:\/\/dndjp.sakura.ne.jp\/OUTPUT.php\?ID=(\d+)$/.test(url);
}

const convert: Convert = async (characterSheetUrl: string) => {
    const characterSheetData = await download(characterSheetUrl);
    const character: CCFoliaCharacter = {
        kind: "character",
        data: {
            name: "",
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

    const $ = cheerio.load(characterSheetData);

    readStatus($, character, chatPalette);
    readParameters($, character, chatPalette);
    readAttacks($, character, chatPalette);
    readSkills($, character, chatPalette);
    readSpellSlots($, character, chatPalette);

    character.data.commands = toCommands(chatPalette);
    
    return character;
};

async function download(characterSheetUrl: string) {
    if (!canConvert(characterSheetUrl)) {
        throw new Error("Invalid character sheet URL");
    }

    const response = await axios.request({
        method: "GET",
        url: characterSheetUrl,
        responseType: "arraybuffer",
        responseEncoding: "binary",
    });

    const encoding = Encoding.detect(response.data);
    if (!encoding) {
        throw new Error("Failed to detect encoding");
    }

    const unicodeString = Encoding.convert(response.data, "UTF8", encoding);
    const pageHtml = decoder.decode(new Uint8Array(unicodeString));

    return pageHtml;
};

const CssSelectors = {
    characterName: "table.DD5ePage:nth-child(5) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(1) > div:nth-child(2)",
    initiative: "table.DD5ePage:nth-child(6) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(1)",
    maxHp: "td.LBC:nth-child(1)",
    currentHp: "td.LBC:nth-child(2)",
    tempHp: "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(1)",
    hitDice: "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(2)",
    ac: "table.DD5ePage:nth-child(6) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(1)",
    inspiration: "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2)",
    abilityScores: [
        { label: "筋力", selector: ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(3)"},
        { label: "敏捷力", selector: ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(3) > td:nth-child(3)"},
        { label: "耐久力", selector: ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(4) > td:nth-child(3)"},
        { label: "知力", selector: ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(5) > td:nth-child(3)"},
        { label: "判断力", selector: ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(6) > td:nth-child(3)"},
        { label: "魅力", selector: ".MBC > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(3)"},
    ],
    savingThrows: [
        { label: "筋力", selector: "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(2)"},
        { label: "敏捷力", selector: "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(3) > td:nth-child(2)"},
        { label: "耐久力", selector: "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(4) > td:nth-child(2)"},
        { label: "知力", selector: "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(5) > td:nth-child(2)"},
        { label: "判断力", selector: "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(6) > td:nth-child(2)"},
        { label: "魅力", selector: "table.DD5ePage:nth-child(7) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(2) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(2)"}
    ],
    attacks: [
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(3)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(5)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(9)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(11)",
        "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(13)",
    ],
    attacksExtra: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(15) > td:nth-child(1) > div:nth-child(1)",
    skills: [
        { label: "威圧", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(3) > td:nth-child(1)" },
        { label: "医術", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(4) > td:nth-child(1)" },
        { label: "運動", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(5) > td:nth-child(1)" },
        { label: "隠密", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(6) > td:nth-child(1)" },
        { label: "軽業", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(7) > td:nth-child(1)" },
        { label: "看破", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(8) > td:nth-child(1)" },
        { label: "芸能", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(9) > td:nth-child(1)" },
        { label: "自然", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(10) > td:nth-child(1)" },
        { label: "宗教", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(11) > td:nth-child(1)" },
        { label: "生存", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(12) > td:nth-child(1)" },
        { label: "説得", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(13) > td:nth-child(1)" },
        { label: "捜査", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(14) > td:nth-child(1)" },
        { label: "知覚", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(15) > td:nth-child(1)" },
        { label: "手先の早業", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(1)" },
        { label: "動物使い", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(17) > td:nth-child(1)" },
        { label: "ペテン", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(18) > td:nth-child(1)" },
        { label: "魔法学", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(19) > td:nth-child(1)" },
        { label: "歴史", selector: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(20) > td:nth-child(1)" },
    ],
    passivePerception: "table.DD5ePage:nth-child(8) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(22) > td:nth-child(2)",
    slots: [
        { 
            total: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(3)",
            used: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(5)" 
        },
        { 
            total: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(33) > td:nth-child(3)",
            used: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(1) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(33) > td:nth-child(5)" 
        },
        { 
            total: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(3)",
            used: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(5)" 
        },
        { 
            total: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(19) > td:nth-child(3)",
            used: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(19) > td:nth-child(5)" 
        },
        { 
            total: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(35) > td:nth-child(3)",
            used: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(35) > td:nth-child(5)" 
        },
        { 
            total: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(3)",
            used: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(5)" 
        },
        { 
            total: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(3)",
            used: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(16) > td:nth-child(5)" 
        },
        { 
            total: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(28) > td:nth-child(3)",
            used: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(28) > td:nth-child(5)" 
        },
        { 
            total: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(39) > td:nth-child(3)",
            used: "table.DD5ePage:nth-child(12) > tbody:nth-child(1) > tr:nth-child(1) > td:nth-child(3) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(39) > td:nth-child(5)" 
        }
    ]
}


function readStatus($: cheerio.CheerioAPI, character: CCFoliaCharacter, chatPalette: ChatPalette) {
    character.data.name = $(CssSelectors.characterName).text();

    const initiativeBonus = parseInt($(CssSelectors.initiative).text());
    character.data.initiative = 0;
    chatPalette.initiative = `1d20+${initiativeBonus} イニシアチブ`;

    const maxHp = parseInt($(CssSelectors.maxHp).text());
    const currentHp = parseInt($(CssSelectors.currentHp).text());
    const tempHp = parseInt($(CssSelectors.tempHp).text());
    const hitDice = $(CssSelectors.hitDice).text().trim();
    const hitDiceMatch = hitDice.match(/^(\d+)d(\d+)$/);
    if (hitDiceMatch) {
        chatPalette.hitDice = `1d${hitDiceMatch[2]} ヒットダイスでHP回復`;
    }

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

    character.data.status.push({
        label: "AC",
        value: parseInt($(CssSelectors.ac).text()),
        max: 0
    });

    const inspiration = parseInt($(CssSelectors.inspiration).text());
    character.data.status.push({
        label: "インスピ",
        value: isNaN(inspiration) ? 0 : inspiration,
        max: 2
    });
}
function readSpellSlots($: cheerio.CheerioAPI, character: CCFoliaCharacter, chatPalette: ChatPalette) {
    for (let slotIndex = 0; slotIndex < CssSelectors.slots.length; slotIndex++) {
        const slotSelectors = CssSelectors.slots[slotIndex];
        const total = parseInt($(slotSelectors.total).text());
        if (isNaN(total)) continue;

        const used = parseInt($(slotSelectors.used).text());

        character.data.status.push({
            label: `スロット${slotIndex + 1}`,
            value: total - (isNaN(used) ? 0 : used),
            max: total
        });
    }
}

function readParameters($: cheerio.CheerioAPI, character: CCFoliaCharacter, chatPalette: ChatPalette) {
    for (let abilityScoreIndex = 0; abilityScoreIndex < CssSelectors.abilityScores.length; abilityScoreIndex++) {
        const attribute = CssSelectors.abilityScores[abilityScoreIndex];
        let value = $(attribute.selector).text().trim();
        
        const chatPaletteValue = value || "+0";
        chatPalette.abilityScores.push(`1d20${chatPaletteValue} 【${attribute.label}】能力値判定`);

        character.data.params.push({
            label: attribute.label,
            value: value || "0",
        });
    }

    const passivePerception = $(CssSelectors.passivePerception).text().trim();
    character.data.params.push({
        label: "自動知覚",
        value: passivePerception || "0",
    });

    for (let savingThrowIndex = 0; savingThrowIndex < CssSelectors.abilityScores.length; savingThrowIndex++) {
        const savingThrow = CssSelectors.savingThrows[savingThrowIndex];
        let value = $(savingThrow.selector).text().trim();
        
        const chatPaletteValue = value || "0";
        const additionalSign = !value.startsWith("-") ? "+" : "";

        chatPalette.savingThrows.push(`1d20${additionalSign}${chatPaletteValue} 【${savingThrow.label}】セーヴィングスロー`);
    }
}

function toCommands(chatPalette: ChatPalette): string {
    const lines: (string | undefined)[] = [];

    lines.push(chatPalette.initiative);
    lines.push(chatPalette.hitDice);

    if (chatPalette.attacks.length > 0) {
        lines.push("=================  攻撃  ================");
        lines.push(...chatPalette.attacks);
    }

    if (chatPalette.savingThrows.length > 0) {
        lines.push("===========  セーヴィングスロー  ==========");
        lines.push(...chatPalette.savingThrows);
    }

    if (chatPalette.abilityScores.length > 0) {
        lines.push("=============  能力値判定  ===============");
        lines.push(...chatPalette.abilityScores);
    }

    if (chatPalette.skills.length > 0) {
        lines.push("=============  技能判定  ================");
        lines.push(...chatPalette.skills);
    }

    return lines
        .filter((line): line is string => !!line)
        .join("\n");
}

function readAttacks($: cheerio.CheerioAPI, character: CCFoliaCharacter, chatPalette: ChatPalette) {
    for (let attackIndex = 0; attackIndex < CssSelectors.attacks.length; attackIndex++) {
        const attackRow = CssSelectors.attacks[attackIndex];
        const row = $(attackRow);
        
        const attackName = row.find("td:nth-child(1)").text().trim();
        if (!attackName) continue;

        const attackBonus = parseInt(row.find("td:nth-child(2)").text());
        const damage = row.find("td:nth-child(3)").text().trim();

        addAttack(chatPalette, attackName, attackBonus, damage);
    }

    const extraAttaks = $(CssSelectors.attacksExtra).text().trim().split("\n");
    for (const extraAttack of extraAttaks) {
        const nameBonusDamageMatch = extraAttack.match(/^(.*)\s*([\+\-]?(\d+))\s*(\d+d\d+[\+\-]?\d*)$/);
        if (nameBonusDamageMatch) {
            const attackName = nameBonusDamageMatch[1].trim();
            const attackBonus = parseInt(nameBonusDamageMatch[2]);
            const damage = nameBonusDamageMatch[4];
 
            addAttack(chatPalette, attackName, attackBonus, damage);
            continue;
        }

        const nameDamageMatch = extraAttack.match(/^(.*)\s*(\d+d\d+[\+\-]?\d*)$/);
        if (nameDamageMatch) {
            const attackName = nameDamageMatch[1].trim();
            const damage = nameDamageMatch[2];
 
            addAttack(chatPalette, attackName, NaN, damage);
            continue;
        }
    }

    function addAttack(chatPalette: ChatPalette, attackName: string, attackBonus: number, damage: string) {
        if (isNaN(attackBonus)) {
            // No attack roll, no critical
            chatPalette.attacks.push(`${damage} 【${attackName}】 ダメージ`);
        } else {
            const sign = attackBonus >= 0 ? "+" : "-";
            chatPalette.attacks.push(`1d20${sign}${attackBonus} 【${attackName}】 攻撃ロール`);
            chatPalette.attacks.push(`${damage} 【${attackName}】 ダメージ`);

            const damageDiceMatch = damage.match(/(\d+)d(\d+)([\+\-]\d+)?/);
            if (damageDiceMatch) {
                const diceCount = parseInt(damageDiceMatch[1]) * 2;
                const criticalDamage = `${diceCount}d${damageDiceMatch[2]}${damageDiceMatch[3] || ""}`;
                chatPalette.attacks.push(`${criticalDamage} 【${attackName}】 クリティカル`);
            }
        }
    }
}

function readSkills($: cheerio.CheerioAPI, character: CCFoliaCharacter, chatPalette: ChatPalette) {
    for (let skillIndex = 0; skillIndex < CssSelectors.skills.length; skillIndex++) {
        const skill = CssSelectors.skills[skillIndex];
        
        const value = $(skill.selector).text().trim();
        const chatPaletteValue = value || "0";
        const additionalSign = !value.startsWith("-") ? "+" : "";

        chatPalette.skills.push(`1d20${additionalSign}${chatPaletteValue} 【${skill.label}】技能判定`);
    }
}

export default {
    canConvert,
    convert
} satisfies Adapter
