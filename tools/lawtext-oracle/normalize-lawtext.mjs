import fs from "node:fs/promises";

const file = process.argv[2];
if (!file) {
  console.error("usage: node normalize-lawtext.mjs <lawtext-file>");
  process.exit(2);
}

const text = await fs.readFile(file, "utf8");
const normalized = text.replace(/\r\n?/g, "\n").split("\n").map(l => l.replace(/[ \t]+$/g, "")).join("\n").replace(/\n*$/g, "\n");
process.stdout.write(normalized);
