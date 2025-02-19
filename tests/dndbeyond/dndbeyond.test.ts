import { adapter, downloader } from "../../src/adapters/dndbeyond";
import { readTestFixture } from "../utils";


describe("dnd beyond", () => {
    it("downloads character sheet", async () => {
        const characterSheetUrl = "https://www.dndbeyond.com/characters/135389047";
        const characterSheetData = await downloader(characterSheetUrl);

        expect(characterSheetData).toBeDefined();
        expect(characterSheetData).toBeTruthy();
    });

    it("throws error on invalid character sheet URL", async () => {
        const characterSheetUrl = "https://www.dndbeyond.com/characters/invalid";
        const downloadTask = downloader(characterSheetUrl);
        await expect(downloadTask).rejects.toThrow("Invalid character sheet URL");
    });

    it("converts character sheet html to CCFolia format", () => {
        const data = readTestFixture(__dirname, "test-sheet.json");
        const character = adapter(data);
        console.log(JSON.stringify(character, undefined, 2));
    });
});