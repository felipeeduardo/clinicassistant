import { expect, test } from "@playwright/test";
test.skip(process.env.E2E_SKIP === "true", "E2E temporarily disabled by E2E_SKIP.");

test("exibe formulário de login", async ({ page }) =>
{
    await page.goto("/login");

    await expect(page.getByRole("heading", { name: "Entre na sua conta" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Entrar" })).toBeVisible();
});

test("autentica o administrador do manifesto E2E", async ({ page }) =>
{
    test.skip(!process.env.E2E_DEFAULT_PASSWORD, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");
    await page.goto("/login");
    await page.getByLabel("E-mail").fill("admin.e2e@fake.local");
    await page.getByLabel("Senha").fill(process.env.E2E_DEFAULT_PASSWORD!);
    await page.getByRole("button", { name: "Entrar" }).click();

    await expect(page).toHaveURL(/\/dashboard$/);
});
