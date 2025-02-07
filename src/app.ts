import express from "express";
import axios from "axios";
import Encoding from "encoding-japanese";

const app = express();
const port = 3000;
const encoder = new TextEncoder();
const decoder = new TextDecoder();

// https://character-service.dndbeyond.com/character/v5/character/138855347

app.get("/convert/sakura/:id", async (req, res) => {
    const id = parseInt(req.params.id, 10);
    if (!id || isNaN(id)) {
        res.status(400);
        res.send("Invalid ID");
        return;
    }

    const sheetUrl = `https://dndjp.sakura.ne.jp/OUTPUT.php?ID=${id}`;
    const paletteUrl = `https://dndjp.sakura.ne.jp/CREATECP.php?ID=${id}`;
    const response = await axios.request({
        method: "GET",
        url: paletteUrl,
        responseType: "arraybuffer",
        responseEncoding: "binary",
    });

    if (response.status !== 200) {
        res.status(400);
        res.send("Failed to load character sheet");
        return;
    }

    const encoding = Encoding.detect(response.data);
    if (!encoding) {
        res.status(400);
        res.send("Failed to detect encoding");
        return;
    }

    const unicodeString = Encoding.convert(response.data, "UTF8", encoding);
    const pageHtml = decoder.decode(new Uint8Array(unicodeString));

    const character = readCharacter(pageHtml);
    character.data.externalUrl = sheetUrl;

    res.contentType("application/json");
    res.send(character);
});

function readCharacter(pageHtml: string): Character {
    const character: Character = {
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
    }

    const lines = pageHtml.split("\n");
    let lineIndex = 0;
    let iterations = 0;
    let state: States = "name";
    const commands: string[] = [];
    const slots: Status[] = [];
    let hp = 0;
    let maxHp = 0;
    let ac = 0;

    while (lineIndex++ <= lines.length && iterations++ < 1000) {
        let line = lines[lineIndex];
        switch (state) {
            case "name": {
                const match = line.match(/^■チャットパレット:(.+)\(PL/);
                if (match) {
                    character.data.name = match[1];
                    state = "initiative";
                }
                break;
            }
            case "initiative": {
                const match = line.match(/(.+) ▼イニシアチブ$/);
                if (match) {
                    commands.push(match[0]);
                    state = "ac";
                }
                break;
            }
            case "ac": {
                const match = line.match(/^▼AC:(\d+)/);
                if (match) {
                    ac = parseInt(match[1], 10);
                    state = "hp";
                }
                break;
            }
            case "hp": {
                const match = line.match(/^▼HP:(\d+)\/(\d+)/);
                if (match) {
                    hp = parseInt(match[1], 10);
                    maxHp = parseInt(match[2], 10);
                    state = "commands";
                }
                break;
            }
            case "commands": {
                if (line.startsWith("■攻撃==")) {
                    commands.push(line);
                    while (lineIndex++ < lines.length) {
                        line = lines[lineIndex];
                        if (line.startsWith("■特徴・特性")) {
                            lineIndex--;
                            state = "slots";
                            break;
                        }
                        commands.push(line);
                    }
                }
                break;
            }
            case "slots": {
                if (line.startsWith("▼初級呪文--")) {
                    while (lineIndex++ < lines.length) {
                        line = lines[lineIndex];
                        if (!line) break;

                        const match = line.match(/^▼(\d+)レベル呪文\(スロット数=(\d+)\)/);
                        if (match) {
                            const level = parseInt(match[1], 10);
                            const amount = parseInt(match[2], 10);
                            if (amount == 0) continue;

                            slots.push({ 
                                label: `Lv${level}呪文スロット`, 
                                value: amount, 
                                max: amount 
                            });
                        }
                    }

                    state = "end";
                }
                break;
            }
            case "end": {
                character.data.commands = commands.join("\n");
                character.data.status.push({ label: "HP", value: hp, max: maxHp });
                character.data.status.push({ label: "AC", value: ac, max: 0 });
                if (slots.length > 0) {
                    character.data.status.push(...slots);
                }
                return character;
            };
        }
    }

    return character
}

app.listen(port, () => {
    console.log(`Server started at http://localhost:${port}`);
});

type States = "name" | "initiative" | "ac" | "hp" | "commands" | "slots" | "end";

type Character = {
    kind: "character";
    data: CharacterData
}

type CharacterData = {
    name: string;
    initiative: number;
    externalUrl: string;
    iconUrl: string;
    commands: string;
    status: Status[];
    params: Param[];
}

type Status = {
    label: string;
    value: number;
    max: number;
}

type Param = {
    label: string;
    value: string;
}