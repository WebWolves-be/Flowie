# iOS App via Capacitor — Flowie

## Context
Angular 20 web app with existing mobile-responsive layout. Goal: wrap it with Capacitor, publish to TestFlight for internal distribution. Backend already hosted (prod URL in `environment.prod.ts`). Apple Developer account needs to be created first.

---

## Phase 1: Capacitor Setup (Windows dev machine)

**Files:** `frontend/flowie-app/package.json`, new `capacitor.config.ts`

- [x] 1. Install packages:
   ```bash
   cd frontend/flowie-app
   npm install @capacitor/core @capacitor/ios
   npm install -D @capacitor/cli @capacitor/assets
   ```

- [x] 2. Init Capacitor (run once):
   ```bash
   npx cap init Flowie be.novara.flowie --web-dir dist/flowie-mobile-app
   ```
   Creates `capacitor.config.ts`.

- [x] 3. Set `capacitor.config.ts`:
   ```ts
   import type { CapacitorConfig } from '@capacitor/cli';
   const config: CapacitorConfig = {
     appId: 'be.novara.flowie',
     appName: 'Flowie',
     webDir: 'dist/flowie-app',
   };
   export default config;
   ```

- [x] 4. Add `build:ios` script to `package.json`:
   ```json
   "build:ios": "ng build --configuration production && npx cap sync ios"
   ```

- [x] 5. Fix safe-area for iOS bottom nav — `src/index.html` viewport meta:
   ```html
   <meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">
   ```

- [x] 6. Fix bottom nav safe area — `bottom-nav.component.html` nav element:
   ```html
   style="padding-bottom: env(safe-area-inset-bottom);"
   ```

- [x] 7. Verify backend CORS allows `capacitor://localhost` origin.

- [x] 8. Verify production build works:
   ```bash
   ng build --configuration production
   # check dist/flowie-app/index.html exists, no budget errors
   ```

- [x] 9. Commit all changes.

---

## Phase 2: iOS Project (Mac)

**Prerequisites on Mac:** Xcode 15+, CocoaPods (`sudo gem install cocoapods`), Node.js

- [ ] 1. Pull repo on Mac, then:
   ```bash
   cd frontend/flowie-app
   npm install
   ng build --configuration production
   npx cap add ios        # creates ios/ directory
   npx cap sync ios
   ```

- [ ] 2. Commit `ios/` directory to git (do this once).

- [ ] 3. Generate app icons — create `resources/icon.png` (1024x1024) and `resources/splash.png` (2732x2732), then:
   ```bash
   npx capacitor-assets generate --ios
   ```

- [ ] 4. Open Xcode:
   ```bash
   npx cap open ios
   ```

- [ ] 5. In Xcode > App target > General:
   - Bundle ID: `be.novara.flowie`
   - Version: `1.0.0`, Build: `1`
   - Deployment Target: iOS 16.0

- [ ] 6. Test on iOS Simulator (Cmd+R) — verify login works and hits production API.

---

## Phase 3: Apple Developer Account + App Store Connect

- [ ] 1. Enroll at [developer.apple.com](https://developer.apple.com) ($99/yr) — wait for approval.

- [ ] 2. Create App ID:
   - Identifiers > App IDs > (+)
   - Bundle ID: `be.novara.flowie` (explicit, cannot change later)

- [ ] 3. Create app in App Store Connect:
   - New App > iOS > Name: `Flowie` > Bundle ID: `be.novara.flowie`

- [ ] 4. In Xcode > Signing & Capabilities:
   - Enable "Automatically manage signing"
   - Select your Developer Team

---

## Phase 4: TestFlight Build & Upload

**Repeatable workflow for every new build:**

On Windows:
```bash
git commit && git push
```

On Mac:
```bash
git pull
npm install
ng build --configuration production
npx cap sync ios
```

In Xcode:
1. Increment Build number (General tab)
2. Product > Archive
3. Organizer > Distribute App > App Store Connect > Upload

In App Store Connect:
- TestFlight tab > wait ~15min for processing > add 2 internal testers (no App Review required)

---

## Optional: Fastlane automation (if manual workflow becomes repetitive)
```bash
gem install fastlane && cd ios/App && fastlane init
```
Configure `Fastfile` with `gym` (build) + `pilot` (TestFlight upload).

---

## Verification Checklist
- [ ] `ng build --configuration production` succeeds with no budget errors
- [ ] App loads in iOS Simulator, login hits production API
- [ ] Bottom nav safe area correct on iPhone X+ (no overlap with home indicator)
- [ ] Build uploads to TestFlight without rejection
- [ ] Testers can install via TestFlight link

---

