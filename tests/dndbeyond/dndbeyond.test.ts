import * as dndbeyond from "../../src/adapters/dndbeyond";
import { readTestFixture } from "../utils";
import axios from "axios";
import MockAdapter from "axios-mock-adapter";

const mock = new MockAdapter(axios);

describe("dnd beyond", () => {
    beforeEach(() => {
        mock.reset();
    });

    it("downloads character sheet", async () => {
        const characterSheetUrl = "https://www.dndbeyond.com/characters/135389047";
        const characterSheetData = await dndbeyond.convert(characterSheetUrl);

        expect(characterSheetData).toBeDefined();
        expect(characterSheetData).toBeTruthy();
    });

    it("throws error on invalid character sheet URL", async () => {
        const characterSheetUrl = "https://www.dndbeyond.com/characters/invalid";
        const downloadTask = await dndbeyond.convert(characterSheetUrl);
        await expect(downloadTask).rejects.toThrow("Invalid character sheet URL");
    });

    it("converts character sheet html to CCFolia format", async () => {
        const data = readTestFixture(__dirname, "test-sheet.json");
        mock.onGet("https://character-service.dndbeyond.com/character/v5/character/1").reply(200, data);

        const character = await dndbeyond.convert("https://www.dndbeyond.com/characters/1");
        console.log(JSON.stringify(character, undefined, 2));
    });
});