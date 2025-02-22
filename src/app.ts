import express from "express";
import dotenv from "dotenv";
import morgan from "morgan";
import * as sakura from "./adapters/sakura"

dotenv.config();

const app = express();
const port = process.env.PORT || 3000;

app.use(morgan('combined'));

app.get("/", (_, res) => {
    res.send("Hi!");
});

app.get("/convert/sakura/:id", async (req, res) => {
    const characterSheetUrl = `https://dndjp.sakura.ne.jp/OUTPUT.php?ID=${req.params.id}`;
    const characterSheet = await sakura.downloader(characterSheetUrl);
    const character = sakura.adapter(characterSheet);
    res.contentType("application/json");
    res.send(character);
});


app.listen(port, () => {
    console.log(`Server started at http://localhost:${port}`);
});