import fs from "node:fs/promises";
import * as lawtext from "lawtext";

const file = process.argv[2];
if (!file) {
  console.error("usage: node verify-lawtext.mjs <lawtext-file>");
  process.exit(2);
}

const input = await fs.readFile(file, "utf8");
const runner = lawtext.run ?? lawtext.default?.run;
if (typeof runner !== "function") {
  console.error("Lawtext oracle API not found (run)");
  process.exit(2);
}

const result = await runner({
  input: { lawtext: input },
  outtypes: ["json"],
  controlel: true
});

if (!result || !result.json) {
  console.error("Lawtext oracle parse failed: no json output");
  process.exit(2);
}

console.log(JSON.stringify({ ok: true }));
