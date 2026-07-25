import { test as setup, expect, request } from "@playwright/test";

const EMAIL = process.env["E2E_EMAIL"] ?? "e2e@flowie.test";
const PASSWORD = process.env["E2E_PASSWORD"] ?? "TestPass123!";
const API_URL = process.env["E2E_API_URL"] ?? "http://localhost:5229";
const REGISTRATION_CODE = process.env["E2E_REGISTRATION_CODE"] ?? "1311";

setup("authenticate", async ({ page }) => {
  const api = await request.newContext({ baseURL: API_URL, ignoreHTTPSErrors: true });
  const login = await api.post("/auth/login", {
    data: { email: EMAIL, password: PASSWORD },
  });
  if (!login.ok()) {
    const register = await api.post("/auth/register", {
      data: {
        firstName: "E2E",
        lastName: "Tester",
        email: EMAIL,
        password: PASSWORD,
        registrationCode: REGISTRATION_CODE,
      },
    });
    expect(register.ok(), await register.text()).toBeTruthy();
  }
  await api.dispose();

  await page.goto("/login");
  await page.fill("#email", EMAIL);
  await page.fill("#password", PASSWORD);
  await page.click('button[type="submit"]');
  await page.waitForURL((url) => !url.toString().includes("/login"), {
    timeout: 15_000,
  });
  await page.context().storageState({ path: "e2e/.auth/user.json" });
});
