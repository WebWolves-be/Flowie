# Design: Convert Flowie Angular app to an installable PWA

**Date:** 2026-07-21
**Status:** Approved
**Branch:** `mobile-strategy`

## Problem

Users want Flowie on their phones as a real app icon that launches full-screen and feels
native, without the overhead of the Apple App Store (developer account, review cycles,
provisioning, Xcode/Mac build chain). An earlier experiment wrapped the app in Capacitor
(`mobile-app-capacitator` branch), but App Store distribution is not essential for the
stated goal.

## Decision

Ship Flowie as a Progressive Web App (PWA). Users install it via Safari/Chrome
"Add to Home Screen". A well-built PWA is also the web layer Capacitor uses, so wrapping
it natively later (if the App Store ever becomes necessary) requires minimal rework — the
Capacitor path is not burned.

## Goals

- Installable to the home screen on iOS (16.4+) and Android.
- Launches full-screen (standalone display, no browser chrome).
- App shell cached via a service worker for fast loads and an offline shell.
- Flowie-branded app icon (placeholder for now, real art swappable later).
- Task data stays live — the backend API is never cached.

## Non-Goals (explicitly out of scope, documented for later)

- **Web push notifications** — planned as a later phase. iOS supports web push for
  installed PWAs since iOS 16.4 via the standard Push API + VAPID keys; the backend would
  send via the Web Push protocol (e.g. a .NET `WebPush` library), no APNs/Apple Developer
  account required. The service worker added here is the foundation this will build on.
- Offline data reads/writes, background sync, mutation queueing.
- In-app "new version available" update prompt UI.

## Current State (main branch)

- **Angular 20**, standalone bootstrap. Providers are declared inline in
  `frontend/flowie-app/src/main.ts` — there is **no** separate `app.config.ts`.
- App language is Dutch (`<html lang="nl">`).
- Primary brand color is teal (`teal-600` = `#0d9488`) per the Tailwind guidelines.
- No PWA packages, no service worker, no manifest.
- No icon/logo assets — `src/assets` is empty and there is no favicon file, although
  `index.html` references `favicon.ico`.

## Design

### 1. Scaffold with `ng add @angular/pwa`

Run in `frontend/flowie-app`. This adds:

- `@angular/service-worker` dependency (matching the Angular 20 major version).
- `ngsw-config.json` — service worker caching configuration.
- `src/manifest.webmanifest` — web app manifest.
- `public/icons/` (or equivalent) — generated icon set.
- `"serviceWorker": true` and the manifest asset entry in `angular.json` (production build).
- Service worker registration.

### 2. Wire service worker registration into `main.ts`

Because providers live inline in `main.ts` (no `app.config.ts`), add `provideServiceWorker`
to the `bootstrapApplication` providers array:

```ts
provideServiceWorker('ngsw-worker.js', {
  enabled: !isDevMode(),
  registrationStrategy: 'registerWhenStable:30000',
})
```

If `ng add` creates or assumes an `app.config.ts`, reconcile it so the provider ends up in
the existing inline-providers setup rather than introducing a parallel config file.

### 3. Manifest configuration (`src/manifest.webmanifest`)

- `name`: "Flowie"
- `short_name`: "Flowie"
- `lang`: "nl"
- `display`: "standalone"
- `theme_color`: "#0d9488" (teal-600)
- `background_color`: "#0d9488"
- `start_url`: "/"
- `orientation`: "portrait"
- `icons`: the generated set (see §5), including a `512x512` maskable icon.

### 4. iOS-specific tags in `index.html`

iOS ignores most of the manifest, so add to `<head>`:

- `<link rel="apple-touch-icon" href="...">` (180×180)
- `<meta name="apple-mobile-web-app-capable" content="yes">`
- `<meta name="apple-mobile-web-app-title" content="Flowie">`
- `<meta name="apple-mobile-web-app-status-bar-style" content="black-translucent">`
- `<meta name="theme-color" content="#0d9488">`

### 5. Icons — placeholder set

Generate a simple placeholder icon: solid teal (`#0d9488`) background with a white "F"
centered. Produce the standard sizes the manifest references (72, 96, 128, 144, 152, 192,
384, 512, plus a 512 maskable and a 180 `apple-touch-icon`). These are intended to be
swapped for real brand art later without touching the manifest/HTML wiring.

### 6. Caching strategy (`ngsw-config.json`)

- **Keep** the default `assetGroups`: the app shell (`index.html`, JS/CSS, manifest) with
  `installMode: prefetch`, and other static assets `lazy`/`prefetch` as generated.
- **Deliberately do not add any `dataGroups`.** The .NET backend API must never be served
  from the service worker cache so task/project data always reflects the server. Document
  this intent in-repo (a note in the design/spec and, if helpful, alongside the config) so
  a future change doesn't accidentally cache API responses.

## Verification

The service worker only registers in a **production** build; `ng serve` does not run it.

1. `ng build` the frontend.
2. Serve the production build over HTTPS or `localhost`.
3. Run a Lighthouse audit — confirm "Installable" / PWA criteria pass.
4. Confirm the manifest and `ngsw-worker.js` load without console errors.
5. On a real device (or Chrome desktop "Install"): install to home screen, confirm the
   Flowie icon appears and the app launches full-screen (standalone, no browser chrome).
6. Confirm task data still loads live (API not cached) after install.

## Risks / Notes

- iOS web push (future phase) requires the app to be installed to the home screen — this
  PWA install step is the prerequisite, so the foundation aligns with that later work.
- The "Add to Home Screen" step is manual on iOS; users may need an in-app nudge. Out of
  scope for this change but worth a follow-up.
- `ng add @angular/pwa` must resolve a service-worker version compatible with Angular 20.
