import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const target = join(root, "src", "build-info.ts");

// package.json is the single source of truth, so `npm version patch` is all a
// release takes. The generated file is byte-identical between builds — it only
// changes when the version does, which keeps it committed without every build
// leaving the working tree dirty.
const { version } = JSON.parse(readFileSync(join(root, "package.json"), "utf8"));

writeFileSync(
  target,
  `export const buildInfo = {
  version: "${version}"
};
`,
  "utf8"
);

console.log(`build-info.ts: ${version}`);
