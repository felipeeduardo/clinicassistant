import { e2e, expect, test } from "./fixtures";

test.skip(!e2e.password, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("acessa pacientes e agenda com os dados determinísticos", async ({ authenticatedPage: page }) => {
  await page.goto("/patients");
  await expect(page.getByRole("heading", { name: "Pacientes" })).toBeVisible();
  await page.goto("/appointments");
  await expect(page.getByRole("heading", { name: "Agenda" })).toBeVisible();
});

test("abre a conversa de fila humana definida no manifesto", async ({ authenticatedPage: page }) => {
  await page.goto("/conversations");
  await expect(page.getByRole("heading", { name: "Inbox de conversas" })).toBeVisible();
  await expect(page.locator("body")).not.toContainText(e2e.manifest.tenantIds.isolated);
});
