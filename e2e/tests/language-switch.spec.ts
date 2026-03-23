import { expect, test } from "@playwright/test";

test.describe("Language switch", () => {
  test("switch to English via flag and persist on protected route", async ({ page }) => {
    await page.context().clearCookies();
    await page.addInitScript(() => {
      localStorage.removeItem("cryptomarket.uiLanguage");
    });
    await page.goto("/");

    await Promise.all([
      page.waitForURL(/uiLang=en-US/i),
      page.locator(".language-flag-card.lang-us").click()
    ]);

    await page.goto("/orders");

    await expect(page.getByText("You are not authorized to access this page.")).toBeVisible();
  });

  test("switch to Spanish via flag and persist on protected route", async ({ page }) => {
    await page.context().clearCookies();
    await page.addInitScript(() => {
      localStorage.removeItem("cryptomarket.uiLanguage");
    });
    await page.goto("/");

    await Promise.all([
      page.waitForURL(/uiLang=es-ES/i),
      page.locator(".language-flag-card.lang-es").click()
    ]);

    await page.goto("/orders");

    await expect(page.getByText(/No tienes permiso para acceder a esta p[aá]gina\.?/i)).toBeVisible();
  });

  test("switch to English persists after full refresh", async ({ page }) => {
    await page.context().clearCookies();
    await page.addInitScript(() => {
      localStorage.removeItem("cryptomarket.uiLanguage");
    });
    await page.goto("/");

    await Promise.all([
      page.waitForURL(/uiLang=en-US/i),
      page.locator(".language-flag-card.lang-us").click()
    ]);

    await page.reload();
    await page.goto("/orders");

    await expect(page.getByText("You are not authorized to access this page.")).toBeVisible();
  });
});
