import { expect, test } from "@playwright/test";
test("exibe formulário de login", async ({ page }) => { await page.goto("/login"); await expect(page.getByRole("heading", { name: "Clinic Assistant" })).toBeVisible(); await expect(page.getByRole("button", { name: "Entrar" })).toBeVisible(); });
