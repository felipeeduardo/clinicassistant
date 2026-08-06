import { expect, test } from "./fixtures";
test.skip(process.env.E2E_SKIP === "true", "E2E temporarily disabled by E2E_SKIP.");

test.skip(!process.env.E2E_DEFAULT_PASSWORD, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("administra disponibilidade, bloqueio e férias de um profissional", async ({ authenticatedPage: page }) => {
  await page.goto("/professionals");
  const professionalRow = page.getByRole("row").filter({ hasText: "Dra. Ana Souza" });
  await expect(professionalRow).toBeVisible();
  await professionalRow.getByRole("button", { name: "Agenda" }).click();
  await expect(page.getByRole("dialog", { name: /Agenda de Dra\. Ana Souza/ })).toBeVisible();

  await page.getByLabel("Dia da semana").selectOption("2");
  await page.locator("#availability-start").fill("07:00");
  await page.locator("#availability-end").fill("08:00");
  await page.getByRole("button", { name: "Adicionar disponibilidade" }).click();
  await expect(page.getByText(/terça: 07:00–08:00/)).toBeVisible();

  await page.getByLabel("Início", { exact: true }).nth(1).fill("2026-08-06T10:00");
  await page.getByLabel("Fim", { exact: true }).nth(1).fill("2026-08-06T11:00");
  await page.getByLabel("Motivo").nth(0).fill("Bloqueio E2E");
  await page.getByRole("button", { name: "Adicionar bloqueio" }).click();
  await expect(page.getByText("Bloqueio E2E")).toBeVisible();

  await page.getByLabel("Início", { exact: true }).nth(2).fill("2026-08-07T10:00");
  await page.getByLabel("Fim", { exact: true }).nth(2).fill("2026-08-07T12:00");
  await page.getByLabel("Motivo").nth(1).fill("Férias E2E");
  await page.getByRole("button", { name: "Adicionar férias" }).click();
  await expect(page.getByText("Férias E2E")).toBeVisible();
});
