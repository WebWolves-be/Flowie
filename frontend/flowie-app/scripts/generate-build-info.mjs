import { execSync } from "node:child_process";
import { writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const target = join(dirname(fileURLToPath(import.meta.url)), "..", "src", "build-info.ts");

function git(command, fallback) {
  try {
    return execSync(`git ${command}`, { encoding: "utf8", stdio: ["ignore", "pipe", "ignore"] }).trim();
  } catch {
    return fallback;
  }
}

// actions/checkout leaves a detached HEAD, so `rev-parse --abbrev-ref HEAD`
// returns the literal "HEAD" in CI. GitHub's own variables are the truth there.
const commit = (process.env.GITHUB_SHA ?? "").slice(0, 7) || git("rev-parse --short HEAD", "onbekend");
const branch = process.env.GITHUB_REF_NAME ?? git("rev-parse --abbrev-ref HEAD", "onbekend");
const builtAt = new Date().toISOString();

writeFileSync(
  target,
  `export const buildInfo = {
  commit: "${commit}",
  branch: "${branch}",
  builtAt: "${builtAt}"
};
`,
  "utf8"
);

console.log(`build-info.ts: ${commit} (${branch}) @ ${builtAt}`);
