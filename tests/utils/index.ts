import { readFileSync } from "fs";
import { join } from "path";

export function readTestFixture(dirname: string, name: string): string {
    return readFileSync(join(dirname, `fixtures/${name}`), "utf-8");
}