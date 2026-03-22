import { expect, test } from "@playwright/test";

declare const process: {
  env: Record<string, string | undefined>;
};

const adminEmail = process.env.E2E_ADMIN_EMAIL;
const adminPassword = process.env.E2E_ADMIN_PASSWORD;

async function loginAsSeededAdmin(page: import("@playwright/test").Page) {
  await page.goto("/Identity/Account/Login");

  await page.locator("input[name='Input.Email']").fill(adminEmail ?? "");
  await page.locator("input[name='Input.Password']").fill(adminPassword ?? "");

  const submitButton = page.locator("form button[type='submit']");
  await expect(submitButton).toBeVisible();

  await Promise.all([
    page.waitForURL(url => !url.pathname.toLowerCase().includes("/identity/account/login"), { timeout: 15_000 }),
    submitButton.click()
  ]);
}

test.describe("Seeded admin authentication flow", () => {
  test.skip(!adminEmail || !adminPassword, "Set E2E_ADMIN_EMAIL and E2E_ADMIN_PASSWORD to run seeded-admin tests.");

  test.beforeEach(async ({ page }) => {
    await page.context().clearCookies();
    await page.addInitScript(() => {
      localStorage.setItem("darkmarket.cookieConsent.v1", JSON.stringify({
        essential: true,
        analytics: true,
        updatedAtUtc: new Date().toISOString()
      }));
    });
  });

  test("seeded admin can login", async ({ page }) => {
    await loginAsSeededAdmin(page);

    await expect(page.locator("a.logout-btn[href='/Identity/Account/Logout']")).toBeVisible();

    const identityCookie = (await page.context().cookies()).find(c => c.name.includes("Identity.Application"));
    expect(identityCookie).toBeDefined();
  });

  test("seeded admin can access critical admin routes", async ({ page }) => {
    await loginAsSeededAdmin(page);

    const criticalRoutes = [
      "/admin",
      "/admin/users",
      "/admin/products",
      "/admin/orders",
      "/admin/payments",
      "/admin/logs"
    ];

    for (const route of criticalRoutes) {
      await page.goto(route);

      const unauthorized = page.getByText(/Voce nao tem permissao para acessar esta pagina\.|You are not authorized to access this page\.|No tienes permiso para acceder a esta p[aá]gina\.?/i);
      await expect(unauthorized).toHaveCount(0);
      await expect(page.locator("h1")).toBeVisible();
    }
  });
});
