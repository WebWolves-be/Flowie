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

// actions/checkout leaves a detached HEAD, so `rev-parse --short HEAD` still
// works but the branch name does not. GitHub's own variable is the truth there.
const commit = (process.env.GITHUB_SHA ?? "").slice(0, 7) || git("rev-parse --short HEAD", "onbekend");

// A hash is unreadable over the phone. Build the version from the build time in
// UTC so it sorts chronologically and can be read out digit by digit — the
// displayed date is rendered in UTC too, so the two always agree.
const now = new Date();
const pad = (value) => String(value).padStart(2, "0");
const version = [
  now.getUTCFullYear(),
  pad(now.getUTCMonth() + 1),
  pad(now.getUTCDate())
].join(".") + `-${pad(now.getUTCHours())}${pad(now.getUTCMinutes())}`;

const builtAt = now.toISOString();

writeFileSync(
  target,
  `export const buildInfo = {
  version: "${version}",
  commit: "${commit}",
  builtAt: "${builtAt}"
};
`,
  "utf8"
);

console.log(`build-info.ts: ${version} (${commit})`);
