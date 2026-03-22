import { expect, test } from "@playwright/test";

test.describe("Authentication and guards", () => {
  test("login page renders core fields", async ({ page }) => {
    await page.goto("/Identity/Account/Login");

    await expect(page.locator("input[name='Input.Email']")).toBeVisible();
    await expect(page.locator("input[name='Input.Password']")).toBeVisible();
    await expect(page.locator("#Input_RememberMe")).toBeVisible();

    const submitButton = page.locator("form button[type='submit']");
    await expect(submitButton).toBeVisible();

    await expect(page).toHaveURL(/\/Identity\/Account\/Login/i);
  });

  test("register page renders core fields", async ({ page }) => {
    await page.goto("/Identity/Account/Register");

    await expect(page.locator("input[name='Input.Email']")).toBeVisible();
    await expect(page.locator("input[name='Input.Password']")).toBeVisible();
    await expect(page.locator("input[name='Input.ConfirmPassword']")).toBeVisible();

    const submitButton = page.locator("form button[type='submit']");
    await expect(submitButton).toBeVisible();

    await expect(page).toHaveURL(/\/Identity\/Account\/Register/i);
  });

  test("anonymous user cannot access admin users route", async ({ page }) => {
    await page.context().clearCookies();
    await page.goto("/admin/users");

    await expect(page.getByText(/Voce nao tem permissao para acessar esta pagina\.|You are not authorized to access this page\.|No tienes permiso para acceder a esta p[aá]gina\.?/i)).toBeVisible();
  });
});
