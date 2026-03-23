# Complete iOS Deployment Guide - Step by Step

## 🚨 Current Issue: Simulator 500 Error

Your simulator runs but gets a 500 error because the app is trying to connect to the production API. Let's fix this first, then proceed with Apple Developer setup.

---

## PHASE 2: iOS Project Setup (You Are Here)

### Step 2.1: Verify Backend API is Running

Open a browser on your Mac and visit:
```
https://flowie-api-prd.livelywave-420a42c9.westeurope.azurecontainerapps.io/health
```

**Expected result:** You should see `Healthy` or a similar response.

**If the URL doesn't load:**
- Your production backend might be down
- Contact your Azure admin or check Azure Container Apps status

### Step 2.2: Check Simulator Network Logs

1. **Open Xcode** (if not already open):
   ```bash
   cd ~/Projects/Github/Flowie/frontend/flowie-app
   npx cap open ios
   ```

2. **Run the app in Simulator** (Cmd+R)

3. **Open the Debug Console** in Xcode:
   - Click the bottom panel where logs appear
   - Look for network errors or 500 responses
   - Look for CORS errors like "Origin not allowed"

4. **Share the error message** - Take a screenshot or copy the exact error from the console

### Step 2.3: Test API Connection from Mac Terminal

Run this command to verify your Mac can reach the production API:

```bash
curl -X POST https://flowie-api-prd.livelywave-420a42c9.westeurope.azurecontainerapps.io/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"claude.code@testing.be","password":"iK845)%U$UYdn25"}'
```

**Expected result:** A JSON response with `accessToken` and `refreshToken`.

**If you get an error:**
- Check your internet connection
- Verify the production backend is deployed and running

### Step 2.4: Enable Network Debugging in Simulator

Add this to your `capacitor.config.ts` to see detailed network logs:

```typescript
import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'be.novara.flowie',
  appName: 'Flowie',
  webDir: 'dist/flowie-app',
  server: {
    // Add this for debugging
    cleartext: true,
    allowNavigation: ['*']
  }
};

export default config;
```

Then rebuild:
```bash
ng build --configuration production
npx cap sync ios
```

---

## PHASE 3: Apple Developer Account Setup

### Step 3.1: Enroll in Apple Developer Program

**⏱️ This takes 24-48 hours for approval. Do this FIRST.**

1. **Go to:** https://developer.apple.com/programs/enroll/

2. **Sign in** with your Apple ID
   - If you don't have an Apple ID, create one at https://appleid.apple.com

3. **Choose enrollment type:**
   - **Individual** (recommended for solo developers) - $99/year
   - **Organization** (requires D-U-N-S number) - $99/year

   For this project, choose **Individual** unless you have a registered company.

4. **Complete the enrollment form:**
   - Legal Name (must match your government ID)
   - Contact Information
   - Accept agreements

5. **Pay the $99 annual fee**
   - Credit card or Apple Pay
   - You'll be charged immediately

6. **Wait for approval email**
   - Usually takes 24-48 hours
   - Check your email for "Enrollment Confirmation"

7. **DO NOT proceed to Phase 3.2 until you receive the approval email**

---

### Step 3.2: Access Apple Developer Account (After Approval)

1. **Go to:** https://developer.apple.com/account

2. **Sign in** with the Apple ID you used for enrollment

3. **Verify you see:**
   - "Certificates, Identifiers & Profiles" menu
   - Your name in the top-right corner
   - "Membership" tab showing "Active"

**If you see "Your enrollment is being processed":**
- Wait for the approval email before continuing
- This can take up to 48 hours

---

### Step 3.3: Create App ID (Bundle Identifier)

**⚠️ This is PERMANENT - you cannot change it later**

1. **Go to:** https://developer.apple.com/account/resources/identifiers/list

2. **Click the (+) button** near "Identifiers"

3. **Select "App IDs"** → Click "Continue"

4. **Select "App"** (not App Clip) → Click "Continue"

5. **Fill in the form:**
   - **Description:** `Flowie`
   - **Bundle ID:** Select "Explicit"
   - **Bundle ID value:** `be.novara.flowie`
     - ⚠️ MUST match exactly what's in `capacitor.config.ts`
     - ⚠️ Cannot be changed after creation

6. **Capabilities:** Leave defaults (you can add later if needed)

7. **Click "Continue"** → **Click "Register"**

8. **Verify:** You should see `be.novara.flowie` in the Identifiers list

---

### Step 3.4: Create App in App Store Connect

1. **Go to:** https://appstoreconnect.apple.com

2. **Sign in** with the same Apple ID

3. **Click "My Apps"** (or "Apps" in the sidebar)

4. **Click the (+) button** → **Select "New App"**

5. **Fill in the form:**
   - **Platforms:** iOS ✓ (check the box)
   - **Name:** `Flowie`
     - This is the public name users will see
     - Can be changed later
   - **Primary Language:** English (or Dutch if you prefer)
   - **Bundle ID:** Select `be.novara.flowie` from the dropdown
     - If you don't see it, go back to Step 3.3
   - **SKU:** `be.novara.flowie` (internal tracking, can be anything unique)
   - **User Access:** Full Access

6. **Click "Create"**

7. **You'll see the app page** with tabs:
   - App Information
   - Pricing and Availability
   - **TestFlight** ← This is where you'll upload builds

---

### Step 3.5: Configure Xcode Signing

**This is where most people get stuck. Follow carefully.**

1. **Open Xcode:**
   ```bash
   cd ~/Projects/Github/Flowie/frontend/flowie-app
   npx cap open ios
   ```

2. **In the Project Navigator (left sidebar):**
   - Click on "App" (the blue Xcode project icon at the top)
   - You'll see two targets: "App" and "App (iOS)"

3. **Select the "App" target** (the first one)

4. **Go to the "Signing & Capabilities" tab**

5. **Check the "Automatically manage signing" checkbox**

6. **In the "Team" dropdown:**
   - **If you see your name/team:** Select it
   - **If you see "Add an Account...":** Click it and sign in with your Apple ID
   - **If you see "None":** Follow Step 3.5.1 below

---

### Step 3.5.1: Add Apple Developer Account to Xcode

**Only needed if "Team" dropdown shows "None"**

1. **In Xcode, go to:** Xcode Menu → Settings (Cmd+,)

2. **Click the "Accounts" tab**

3. **Click the (+) button** at the bottom left

4. **Select "Apple ID"** → Click "Continue"

5. **Sign in** with your Apple Developer Apple ID

6. **Wait for the account to load**
   - You should see your email and "Personal Team" or your organization name

7. **Close the Settings window**

8. **Go back to:** Project → App target → Signing & Capabilities

9. **Now the "Team" dropdown should show your team** → Select it

---

### Step 3.5.2: Fix Common Signing Errors

**Error: "Failed to create provisioning profile"**

1. Go to Signing & Capabilities
2. Change the Bundle Identifier temporarily:
   - Add a `.test` suffix: `be.novara.flowie.test`
   - Wait 5 seconds
3. Change it back to: `be.novara.flowie`
4. Xcode will re-fetch the provisioning profile

**Error: "Your account already has an Apple Development certificate"**

1. This is normal - ignore it
2. Xcode will use the existing certificate

**Error: "No signing certificate found"**

1. Go to https://developer.apple.com/account/resources/certificates/list
2. Click (+) → Select "Apple Development" → Continue
3. Follow the instructions to create a Certificate Signing Request (CSR)
4. Upload the CSR → Download the certificate
5. Double-click the downloaded certificate to install it in Keychain
6. Restart Xcode

---

### Step 3.6: Verify Signing is Working

1. **In Xcode, Signing & Capabilities tab, you should see:**
   - ✅ "Automatically manage signing" is checked
   - ✅ Team is selected (your name or team name)
   - ✅ "Provisioning Profile" shows "Xcode Managed Profile"
   - ✅ "Signing Certificate" shows "Apple Development: your-email@example.com"

2. **No errors** in yellow or red boxes

3. **Test it:** Press Cmd+R to build and run in Simulator
   - If it builds without signing errors, you're good ✅

---

## PHASE 4: Building and Uploading to TestFlight

### Step 4.1: Prepare for First Build

1. **Open Xcode:**
   ```bash
   cd ~/Projects/Github/Flowie/frontend/flowie-app
   npx cap open ios
   ```

2. **Select the "App" target** → **General tab**

3. **Set version numbers:**
   - **Version:** `1.0.0`
   - **Build:** `1`
   - ⚠️ You'll increment "Build" for each new upload (2, 3, 4, etc.)
   - You only change "Version" for major releases (1.1.0, 2.0.0, etc.)

4. **Set Deployment Target:**
   - **iOS Deployment Target:** `16.0`
   - This is the minimum iOS version required to run your app

---

### Step 4.2: Create an Archive (Build for Release)

1. **In Xcode, select the build destination:**
   - Top toolbar, next to "App" scheme
   - Click and select **"Any iOS Device (arm64)"**
   - ⚠️ Do NOT select Simulator - it won't work for TestFlight

2. **Clean the build folder:**
   - Menu: **Product → Clean Build Folder** (Cmd+Shift+K)
   - Wait for it to finish

3. **Create the archive:**
   - Menu: **Product → Archive**
   - This will compile the release version
   - ⏱️ Takes 2-5 minutes depending on your Mac

4. **Wait for the "Organizer" window to open**
   - If it doesn't open automatically: **Window → Organizer** (Cmd+Shift+Option+O)
   - You should see your archive in the "Archives" tab

---

### Step 4.3: Distribute the Archive to App Store Connect

1. **In the Organizer window:**
   - Select your archive (should be at the top)
   - Click the **"Distribute App"** button on the right

2. **Select distribution method:**
   - Choose **"App Store Connect"**
   - Click **"Next"**

3. **Select destination:**
   - Choose **"Upload"** (not "Export")
   - Click **"Next"**

4. **Distribution options:**
   - ✅ "Upload your app's symbols to receive symbolicated crash logs from Apple"
   - ✅ "Manage Version and Build Number" (if shown)
   - Click **"Next"**

5. **Automatically manage signing:**
   - Select **"Automatically manage signing"**
   - Click **"Next"**

6. **Review the build information:**
   - Verify Bundle ID: `be.novara.flowie`
   - Verify Version: `1.0.0 (1)`
   - Click **"Upload"**

7. **Wait for upload to complete:**
   - ⏱️ Takes 2-10 minutes depending on file size and internet speed
   - You'll see a progress bar

8. **Success dialog:**
   - Click **"Done"**

---

### Step 4.4: Wait for Processing in App Store Connect

1. **Go to:** https://appstoreconnect.apple.com

2. **Click "My Apps"** → **Select "Flowie"**

3. **Click the "TestFlight" tab** at the top

4. **You'll see "Processing" or "Waiting for Processing"**
   - ⏱️ This takes 10-30 minutes (sometimes up to an hour)
   - You can close the browser and check back later
   - You'll receive an email when it's ready

5. **Wait for email:** "The build has finished processing"

---

### Step 4.5: Add Internal Testers

**⚠️ Only do this AFTER the build finishes processing**

1. **In App Store Connect → Flowie → TestFlight tab:**

2. **You should see your build** under "iOS Builds"
   - Build 1.0.0 (1)
   - Status: "Ready to Submit" or "Missing Compliance"

3. **If you see "Missing Compliance":**
   - Click the build → Click "Manage" next to "Export Compliance"
   - Answer "No" to encryption questions (unless you added custom encryption)
   - Click "Start Internal Testing"

4. **Add internal testers:**
   - Sidebar: Click "Internal Testing"
   - Click the (+) button next to "Testers"
   - Select "App Store Connect Users" tab
   - Click (+) to add a tester
   - Enter their email address (must have an Apple ID)
   - Click "Add"
   - Repeat for up to 100 internal testers

5. **Enable automatic distribution:**
   - Toggle "Enable Automatic Distribution" ON
   - This auto-sends new builds to testers

6. **Testers will receive an email:**
   - "You're invited to test Flowie"
   - They need to install TestFlight app from the App Store
   - Click the link in the email
   - Install your app via TestFlight

---

### Step 4.6: Install TestFlight on Your Device

**To test on your own iPhone/iPad:**

1. **Download TestFlight app:**
   - Open App Store on your iOS device
   - Search "TestFlight"
   - Install the official Apple TestFlight app

2. **Check your email** (the one you used for Apple Developer)
   - Look for "You're invited to test Flowie"
   - Click the link or "View in TestFlight" button

3. **TestFlight will open:**
   - Tap "Accept"
   - Tap "Install"

4. **Your app will install** like a normal App Store app

5. **Open the app and test:**
   - Login with: `claude.code@testing.be` / `iK845)%U$UYdn25`
   - Verify it connects to production API
   - Test all features

---

## REPEATABLE WORKFLOW: Uploading New Builds

**Every time you make code changes and want to update TestFlight:**

### On Windows (or your dev machine):

1. **Make your code changes**

2. **Commit and push:**
   ```bash
   git add .
   git commit -m "Your commit message"
   git push
   ```

### On Mac:

1. **Pull the latest code:**
   ```bash
   cd ~/Projects/Github/Flowie/frontend/flowie-app
   git pull
   ```

2. **Install dependencies (if package.json changed):**
   ```bash
   npm install
   ```

3. **Build the Angular app:**
   ```bash
   ng build --configuration production
   ```

4. **Sync with Capacitor:**
   ```bash
   npx cap sync ios
   ```

5. **Open Xcode:**
   ```bash
   npx cap open ios
   ```

6. **Increment the Build number:**
   - App target → General tab
   - Change Build from `1` → `2` (or `3`, `4`, etc.)
   - ⚠️ Must be a higher number than the previous upload

7. **Repeat Phase 4 Steps 4.2-4.4:**
   - Product → Archive
   - Distribute → Upload
   - Wait for processing

8. **Testers get the new build automatically** (if auto-distribution is enabled)

---

## 🔧 TROUBLESHOOTING

### "No accounts with App Store Connect access"

- Go to https://appstoreconnect.apple.com/access/users
- Verify your Apple ID is listed as "Admin" role
- Wait 1-2 hours after enrollment for permissions to propagate

### "The bundle identifier is already in use"

- Someone else registered `be.novara.flowie`
- You must use a unique Bundle ID: `be.yourcompany.flowie`
- Update it in `capacitor.config.ts` and Xcode

### "Invalid provisioning profile"

- Xcode → Settings → Accounts
- Right-click your team → "Download Manual Profiles"
- Try archiving again

### "Simulator 500 error"

1. **Check if production API is running:**
   ```bash
   curl https://flowie-api-prd.livelywave-420a42c9.westeurope.azurecontainerapps.io/health
   ```

2. **Check Xcode console logs** for the exact error

3. **Verify CORS settings** in backend `Program.cs` include `capacitor://localhost`

4. **Test with a real device instead of Simulator:**
   - Plug in iPhone via USB
   - Select it as the build destination
   - Press Cmd+R to run

### "Build stuck on 'Processing'"

- Normal processing time: 15-30 minutes
- If stuck for 2+ hours, reject the build and upload again

---

## 📋 CHECKLIST: Are You Ready for TestFlight?

- [ ] Enrolled in Apple Developer Program ($99 paid, approval email received)
- [ ] Created App ID `be.novara.flowie` in developer.apple.com
- [ ] Created app in App Store Connect
- [ ] Added Apple ID to Xcode Accounts
- [ ] Automatic signing enabled with no errors
- [ ] Can build and run in Simulator without errors
- [ ] Production API is reachable from Mac
- [ ] Build number incremented for each upload
- [ ] Archive created successfully
- [ ] Upload to App Store Connect completed
- [ ] Build finished processing (email received)
- [ ] Internal testers added
- [ ] TestFlight app installed on test device
- [ ] App works on real device via TestFlight

---

## 🆘 NEXT STEPS FOR YOUR 500 ERROR

**Immediate actions:**

1. Run this on your Mac to test API connectivity:
   ```bash
   curl https://flowie-api-prd.livelywave-420a42c9.westeurope.azurecontainerapps.io/health
   ```

2. Share the Xcode console logs when the 500 error occurs

3. Verify your production backend is deployed and running on Azure

4. Try testing on a **real iOS device** instead of Simulator (sometimes Simulator has network restrictions)

**Once the API connection works, you can proceed with Apple Developer enrollment.**
