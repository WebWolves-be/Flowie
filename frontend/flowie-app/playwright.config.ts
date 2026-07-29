import { defineConfig, devices } from "@playwright/test";

const baseURL = process.env["E2E_BASE_URL"] ?? "https://localhost:4200";

export default defineConfig({
  testDir: "./e2e",
  timeout: 30_000,
  expect: { timeout: 10_000 },
  retries: 1,
  workers: 1,
  reporter: [["list"], ["html", { open: "never" }]],
  use: {
    baseURL,
    ignoreHTTPSErrors: true,
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },
  projects: [
    {
      name: "setup",
      testMatch: /auth\.setup\.ts/,
      teardown: "cleanup",
      use: { ...devices["Desktop Chrome"], viewport: { width: 1280, height: 720 } },
    },
    {
      name: "desktop",
      dependencies: ["setup"],
      // mobile-layout.spec.ts asserts phone-only rules (bottom nav, 44px touch
      // targets, 16px fields) that deliberately do not hold at 1280px.
      testIgnore: [
        /auth\.setup\.ts/,
        /cleanup\.teardown\.ts/,
        /mobile-layout\.spec\.ts/,
      ],
      use: {
        ...devices["Desktop Chrome"],
        viewport: { width: 1280, height: 720 },
        storageState: "e2e/.auth/user.json",
      },
    },
    {
      name: "mobile",
      dependencies: ["setup"],
      testIgnore: [/auth\.setup\.ts/, /cleanup\.teardown\.ts/],
      use: {
        ...devices["Pixel 7"],
        storageState: "e2e/.auth/user.json",
      },
    },
    // Pixel 7 is 412px wide, which hides defects that only appear on a narrow
    // phone. 375x812 is the iPhone 13 Mini / SE class of device and is the
    // narrowest viewport the app supports, so the layout rules are enforced there.
    {
      name: "mobile-small",
      dependencies: ["setup"],
      testMatch: /mobile-layout\.spec\.ts/,
      use: {
        ...devices["Pixel 7"],
        viewport: { width: 375, height: 812 },
        storageState: "e2e/.auth/user.json",
      },
    },
    // A phone in landscape is wider than the `md` breakpoint but far too short
    // for a desktop layout — the case that used to scroll sideways by 175px.
    {
      name: "mobile-landscape",
      dependencies: ["setup"],
      testMatch: /mobile-layout\.spec\.ts/,
      use: {
        ...devices["Pixel 7"],
        viewport: { width: 812, height: 375 },
        storageState: "e2e/.auth/user.json",
      },
    },
    {
      name: "cleanup",
      testMatch: /cleanup\.teardown\.ts/,
      use: {
        ...devices["Desktop Chrome"],
        viewport: { width: 1280, height: 720 },
        storageState: "e2e/.auth/user.json",
      },
    },
  ],
});
