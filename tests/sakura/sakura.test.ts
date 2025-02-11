import { adapter, downloader } from "../../src/adapters/sakura";
import { readTestFixture } from "../utils";


describe("sakura", () => {
    it("downloads character sheet", async () => {
        const characterSheetUrl = "https://dndjp.sakura.ne.jp/OUTPUT.php?ID=46214";
        const characterSheetData = await downloader(characterSheetUrl);

        expect(characterSheetData).toBeDefined();
        expect(characterSheetData).toBeTruthy();
    });

    it("throws error on invalid character sheet URL", async () => {
        const characterSheetUrl = "https://dndjp.sakura.ne.jp/OUTPUT.php?ID=invalid";
        const downloadTask = downloader(characterSheetUrl);
        await expect(downloadTask).rejects.toThrow("Invalid character sheet URL");
    });

    it("converts character sheet html to CCFolia format", () => {
        const data = readTestFixture(__dirname, "test-sheet.html");
        const character = adapter(data);
        console.log(JSON.stringify(character, undefined, 2));
    });
});