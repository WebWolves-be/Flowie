# PWA Conversion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convert the Flowie Angular 20 app into an installable PWA (add-to-home-screen, service worker, manifest, branded placeholder icon) without App Store distribution.

**Architecture:** Scaffold with `ng add @angular/pwa`, then configure the manifest, generate a teal "F" placeholder icon set, add iOS-specific `<head>` tags, and keep the backend API uncached so task data stays live. Push notifications are explicitly out of scope (future phase); the service worker added here is the foundation for them.

**Tech Stack:** Angular 20 (standalone bootstrap, `browser` builder), `@angular/service-worker`, `sharp` (dev-only, for icon generation).

**Spec:** `docs/superpowers/specs/2026-07-21-pwa-conversion-design.md`

**Working directory for all commands:** `frontend/flowie-app`

**Note on testing:** This is configuration/scaffolding work — there are no meaningful unit tests for a manifest or service-worker config. Each task therefore uses a concrete **verification command with expected output** in place of a failing unit test. The discipline (verify before commit) is preserved.

**Key environment facts (verified against the repo):**
- Builder: `@angular-devkit/build-angular:browser` (the older builder, not the application builder).
- Build output: `dist/flowie-app` (index.html sits directly in this folder — no `/browser` subfolder).
- Static assets: `src/assets/` and `src/favicon.ico` (there is **no** `public/` directory). Icons therefore live in `src/assets/icons/` and are referenced as `assets/icons/...`.
- Providers are declared **inline in `src/main.ts`** via `bootstrapApplication(...)`. There is **no** `app.config.ts`.
- App language is Dutch (`<html lang="nl">`). Brand color is teal `#0d9488`.

---

### Task 1: Scaffold PWA with `ng add @angular/pwa`

**Files:**
- Create: `frontend/flowie-app/ngsw-config.json`
- Create: `frontend/flowie-app/src/manifest.webmanifest`
- Create: `frontend/flowie-app/src/assets/icons/icon-*.png` (schematic-generated placeholders)
- Modify: `frontend/flowie-app/package.json` (adds `@angular/service-worker`)
- Modify: `frontend/flowie-app/angular.json` (`serviceWorker`, `ngswConfigPath`, manifest asset)
- Modify: `frontend/flowie-app/src/index.html` (manifest link + `theme-color` meta)
- Possibly modify: `frontend/flowie-app/src/main.ts` and/or create `src/app/app.config.ts` (SW registration — reconciled in Task 2)

- [ ] **Step 1: Run the schematic**

Run (from `frontend/flowie-app`):

```bash
npx ng add @angular/pwa --project flowie-app
```

If prompted to proceed with installing the package, accept.

- [ ] **Step 2: Verify the expected files now exist**

Run (from `frontend/flowie-app`):

```bash
ls ngsw-config.json src/manifest.webmanifest && ls src/assets/icons/
```

Expected: `ngsw-config.json` and `src/manifest.webmanifest` both listed, and `src/assets/icons/` contains `icon-72x72.png` through `icon-512x512.png`.

- [ ] **Step 3: Verify angular.json wiring**

Run (from `frontend/flowie-app`):

```bash
node -e "const b=require('./angular.json').projects['flowie-app'].architect.build.options; console.log('serviceWorker:', b.serviceWorker); console.log('ngswConfigPath:', b.ngswConfigPath); console.log('assets:', JSON.stringify(b.assets));"
```

Expected: `serviceWorker: true`, `ngswConfigPath: ngsw-config.json`, and `assets` now includes `src/manifest.webmanifest`.

- [ ] **Step 4: Verify the build still succeeds**

Run (from `frontend/flowie-app`):

```bash
npx ng build
```

Expected: build completes successfully and `dist/flowie-app/ngsw-worker.js` and `dist/flowie-app/manifest.webmanifest` are produced.

- [ ] **Step 5: Commit**

```bash
git add frontend/flowie-app
git commit -m "feat(pwa): scaffold PWA support via ng add @angular/pwa"
```

---

### Task 2: Reconcile service-worker registration into `main.ts`

The `@angular/pwa` schematic registers the service worker via `provideServiceWorker(...)`. In a standalone app it targets `app.config.ts`; since this project has none, it may have created one or edited `main.ts`. This task guarantees the provider ends up in the existing inline providers array in `main.ts` and that no stray `app.config.ts` is left behind.

**Files:**
- Modify: `frontend/flowie-app/src/main.ts`
- Delete (if the schematic created it): `frontend/flowie-app/src/app/app.config.ts`

- [ ] **Step 1: Inspect what the schematic changed**

Run (from `frontend/flowie-app`):

```bash
git show HEAD -- src/main.ts src/app/app.config.ts | head -100
```

Note whether `provideServiceWorker` landed in `main.ts`, in a new `app.config.ts`, or nowhere.

- [ ] **Step 2: Ensure `main.ts` imports the needed symbols**

Confirm the top of `src/main.ts` includes these imports (add any that are missing):

```ts
import { isDevMode } from "@angular/core";
import { provideServiceWorker } from "@angular/service-worker";
```

- [ ] **Step 3: Ensure the provider is in the inline providers array**

The `bootstrapApplication` call in `src/main.ts` must include `provideServiceWorker` in its `providers` array, alongside the existing providers:

```ts
bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    provideServiceWorker("ngsw-worker.js", {
      enabled: !isDevMode(),
      registrationStrategy: "registerWhenStable:30000",
    }),
  ],
}).catch((err) => console.error(err));
```

- [ ] **Step 4: Remove any duplicate `app.config.ts` the schematic created**

If Step 1 showed the schematic created `src/app/app.config.ts` solely for the SW provider (and it is not otherwise referenced by `main.ts`), delete it:

```bash
git ls-files src/app/app.config.ts
```

If that prints a path, confirm `main.ts` does **not** import from `./app/app.config` (`grep -n "app.config" src/main.ts`). If there is no such import, delete the file:

```bash
git rm src/app/app.config.ts
```

- [ ] **Step 5: Verify the provider is wired exactly once and the app builds**

Run (from `frontend/flowie-app`):

```bash
grep -rn "provideServiceWorker" src/ && npx ng build
```

Expected: exactly **one** `provideServiceWorker` occurrence, located in `src/main.ts`, and the build succeeds.

- [ ] **Step 6: Commit**

```bash
git add frontend/flowie-app/src
git commit -m "refactor(pwa): register service worker in main.ts providers"
```

---

### Task 3: Configure the web manifest for Flowie

**Files:**
- Modify: `frontend/flowie-app/src/manifest.webmanifest`

- [ ] **Step 1: Replace the manifest contents**

Overwrite `src/manifest.webmanifest` with:

```json
{
  "name": "Flowie",
  "short_name": "Flowie",
  "lang": "nl",
  "description": "Flowie - taak- en projectbeheer",
  "theme_color": "#0d9488",
  "background_color": "#0d9488",
  "display": "standalone",
  "orientation": "portrait",
  "scope": "/",
  "start_url": "/",
  "icons": [
    { "src": "assets/icons/icon-72x72.png", "sizes": "72x72", "type": "image/png", "purpose": "maskable any" },
    { "src": "assets/icons/icon-96x96.png", "sizes": "96x96", "type": "image/png", "purpose": "maskable any" },
    { "src": "assets/icons/icon-128x128.png", "sizes": "128x128", "type": "image/png", "purpose": "maskable any" },
    { "src": "assets/icons/icon-144x144.png", "sizes": "144x144", "type": "image/png", "purpose": "maskable any" },
    { "src": "assets/icons/icon-152x152.png", "sizes": "152x152", "type": "image/png", "purpose": "maskable any" },
    { "src": "assets/icons/icon-192x192.png", "sizes": "192x192", "type": "image/png", "purpose": "maskable any" },
    { "src": "assets/icons/icon-384x384.png", "sizes": "384x384", "type": "image/png", "purpose": "maskable any" },
    { "src": "assets/icons/icon-512x512.png", "sizes": "512x512", "type": "image/png", "purpose": "maskable any" }
  ]
}
```

- [ ] **Step 2: Verify it is valid JSON with the expected values**

Run (from `frontend/flowie-app`):

```bash
node -e "const m=JSON.parse(require('node:fs').readFileSync('src/manifest.webmanifest','utf8')); console.log('name:',m.name,'| display:',m.display,'| theme:',m.theme_color,'| lang:',m.lang,'| icons:',m.icons.length);"
```

Expected: `name: Flowie | display: standalone | theme: #0d9488 | lang: nl | icons: 8`.

(Note: `require()` only parses `.json`, not `.webmanifest`, so read + `JSON.parse` explicitly.)

- [ ] **Step 3: Commit**

```bash
git add frontend/flowie-app/src/manifest.webmanifest
git commit -m "feat(pwa): configure manifest with Flowie branding"
```

---

### Task 4: Generate the teal "F" placeholder icons

Replace the schematic's Angular-logo placeholders with a simple font-independent teal square + white "F" (built from rectangles, so no font rendering is required). Also produce a 180×180 `apple-touch-icon.png` for iOS.

**Files:**
- Create: `frontend/flowie-app/scripts/generate-icons.mjs`
- Modify: `frontend/flowie-app/package.json` (adds `sharp` dev dependency)
- Overwrite: `frontend/flowie-app/src/assets/icons/icon-*.png`
- Create: `frontend/flowie-app/src/assets/icons/apple-touch-icon.png`

- [ ] **Step 1: Add the `sharp` dev dependency**

Run (from `frontend/flowie-app`):

```bash
npm install --save-dev sharp
```

- [ ] **Step 2: Create the icon generation script**

Create `frontend/flowie-app/scripts/generate-icons.mjs`:

```js
import sharp from "sharp";
import { mkdir } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const outDir = join(dirname(fileURLToPath(import.meta.url)), "..", "src", "assets", "icons");

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
```

- [ ] **Step 3: Run the script**

Run (from `frontend/flowie-app`):

```bash
node scripts/generate-icons.mjs
```

Expected: it logs `wrote icon-72x72.png` ... `wrote icon-512x512.png` and `wrote apple-touch-icon.png`.

- [ ] **Step 4: Verify the icons are real PNGs of the right size**

Run (from `frontend/flowie-app`):

```bash
node -e "const s=require('sharp'); s('src/assets/icons/icon-512x512.png').metadata().then(m=>console.log('512 icon:',m.format,m.width+'x'+m.height)); s('src/assets/icons/apple-touch-icon.png').metadata().then(m=>console.log('apple:',m.format,m.width+'x'+m.height));"
```

Expected: `512 icon: png 512x512` and `apple: png 180x180`.

- [ ] **Step 5: Commit**

```bash
git add frontend/flowie-app/scripts/generate-icons.mjs frontend/flowie-app/src/assets/icons frontend/flowie-app/package.json frontend/flowie-app/package-lock.json
git commit -m "feat(pwa): generate teal placeholder app icons"
```

---

### Task 5: Add iOS-specific tags to `index.html`

iOS Safari ignores most of the manifest, so it needs explicit `<head>` tags to render the app icon and launch full-screen.

**Files:**
- Modify: `frontend/flowie-app/src/index.html`

- [ ] **Step 1: Add the Apple meta tags and touch icon**

In `src/index.html`, inside `<head>` (after the existing `theme-color` meta added by the schematic, and after the manifest `<link>`), add:

```html
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="apple-mobile-web-app-title" content="Flowie">
<meta name="apple-mobile-web-app-status-bar-style" content="black-translucent">
<link rel="apple-touch-icon" href="assets/icons/apple-touch-icon.png">
```

If the schematic did **not** already add a `theme-color` meta, also add:

```html
<meta name="theme-color" content="#0d9488">
```

- [ ] **Step 2: Verify the tags are present exactly once each**

Run (from `frontend/flowie-app`):

```bash
grep -c "apple-mobile-web-app-capable" src/index.html && grep -c "apple-touch-icon" src/index.html && grep -c "theme-color" src/index.html && grep -c "manifest.webmanifest" src/index.html
```

Expected: each command prints `1`.

- [ ] **Step 3: Commit**

```bash
git add frontend/flowie-app/src/index.html
git commit -m "feat(pwa): add iOS home-screen meta tags"
```

---

### Task 6: Verify the caching strategy keeps the API live

The service worker must cache the app shell but never the backend API, so task/project data always reflects the server.

**Files:**
- Modify (only if needed): `frontend/flowie-app/ngsw-config.json`

- [ ] **Step 1: Confirm there are no `dataGroups`**

Run (from `frontend/flowie-app`):

```bash
node -e "const c=require('./ngsw-config.json'); console.log('dataGroups:', c.dataGroups ? c.dataGroups.length : 'none'); console.log('assetGroups:', c.assetGroups.map(g=>g.name).join(', '));"
```

Expected: `dataGroups: none` (or the key is absent) and `assetGroups` lists the default `app` and `assets` groups. If a `dataGroups` array exists, remove it so no API responses are cached.

- [ ] **Step 2: Confirm the manifest and icons fall under an asset group**

Run (from `frontend/flowie-app`):

```bash
node -e "const c=require('./ngsw-config.json'); console.log(JSON.stringify(c.assetGroups.find(g=>g.name==='assets').resources, null, 1));"
```

Expected: the `assets` group's `resources.files` includes a glob such as `/assets/**` and/or `/*.(png|webmanifest|...)` that covers the icons and manifest. If icons/manifest are not covered, add `"/assets/**"` and `"/*.webmanifest"` to that group's `files` array.

- [ ] **Step 3: Commit (only if you changed the file)**

```bash
git add frontend/flowie-app/ngsw-config.json
git commit -m "chore(pwa): ensure service worker never caches the backend API"
```

If nothing changed, skip this commit.

---

### Task 7: End-to-end verification of the installable PWA

The service worker only registers in a **production build served over a secure context** (`localhost` counts). `ng serve` does **not** register it.

**Files:** none (verification only).

- [ ] **Step 1: Produce a production build**

Run (from `frontend/flowie-app`):

```bash
npx ng build
```

Expected: success; `dist/flowie-app/` contains `index.html`, `manifest.webmanifest`, `ngsw-worker.js`, and `assets/icons/icon-512x512.png`.

- [ ] **Step 2: Serve the production build on localhost**

Run (from `frontend/flowie-app`):

```bash
npx http-server dist/flowie-app -p 8080 -c-1
```

Leave it running. `localhost` is a secure context, so the service worker will register.

- [ ] **Step 3: Verify install criteria in Chrome**

Open `http://localhost:8080` in Chrome, then DevTools → **Application**:
- **Manifest** panel: name "Flowie", theme color teal, all 8 icons load (no red errors), "Installable" with no listed issues.
- **Service Workers** panel: `ngsw-worker.js` is **activated and running**.
- The address bar shows an **install** affordance (desktop) / the browser menu offers "Install Flowie".

Expected: manifest valid, service worker active, app is installable.

- [ ] **Step 4: Verify the API is not cached**

With the app open, log in (test creds in root `CLAUDE.md`), then in DevTools → **Network**, reload and confirm calls to the .NET backend are served from the network (Size column is not "(ServiceWorker)"). Optionally, in the **Application → Cache Storage** panel, confirm no backend API responses are stored.

Expected: backend/API responses come from the network, not the service worker cache.

- [ ] **Step 5: Verify home-screen install (device or emulation)**

Preferred — on a real iPhone (iOS 16.4+) reachable on your network: open the served URL in Safari, tap **Share → Add to Home Screen**, confirm the teal "F" icon appears and launches Flowie full-screen (no Safari chrome).

If no device is available, use Chrome desktop "Install Flowie" from Step 3 and confirm it opens in its own standalone window with the teal "F" icon.

Expected: Flowie installs, shows the teal "F" icon, and launches in standalone (no browser chrome).

- [ ] **Step 6: Stop the server**

Stop the `http-server` process (Ctrl+C).

---

## Definition of Done

- `ng build` succeeds and emits `ngsw-worker.js` + `manifest.webmanifest`.
- Chrome reports the app as installable; the service worker activates.
- The teal "F" icon shows on install; the app launches standalone.
- Backend API calls are never served from the service worker cache.
- Push notifications remain unimplemented (documented future phase in the spec).
