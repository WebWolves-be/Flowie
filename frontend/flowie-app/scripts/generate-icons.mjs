import sharp from "sharp";
import { mkdir, writeFile } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const publicDir = join(dirname(fileURLToPath(import.meta.url)), "..", "public");
const outDir = join(publicDir, "icons");

const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <rect width="512" height="512" rx="112" fill="#0d9488"/>
  <rect x="180" y="140" width="70" height="232" fill="#ffffff"/>
  <rect x="180" y="140" width="180" height="64" fill="#ffffff"/>
  <rect x="180" y="238" width="140" height="60" fill="#ffffff"/>
</svg>`;

const sizes = [72, 96, 128, 144, 152, 192, 384, 512];

await mkdir(outDir, { recursive: true });

for (const size of sizes) {
  await sharp(Buffer.from(svg))
    .resize(size, size)
    .png()
    .toFile(join(outDir, `icon-${size}x${size}.png`));
  console.log(`wrote icon-${size}x${size}.png`);
}

await sharp(Buffer.from(svg))
  .resize(180, 180)
  .png()
  .toFile(join(outDir, "apple-touch-icon.png"));
console.log("wrote apple-touch-icon.png");

await writeFile(join(publicDir, "favicon.svg"), svg);
console.log("wrote favicon.svg");

for (const size of [16, 32, 48]) {
  await sharp(Buffer.from(svg))
    .resize(size, size)
    .png()
    .toFile(join(publicDir, `favicon-${size}x${size}.png`));
  console.log(`wrote favicon-${size}x${size}.png`);
}
