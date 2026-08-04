import { e2e, expect, test } from "./fixtures";

test.skip(!e2e.password, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("cria e edita uma especialidade administrativa", async ({ authenticatedPage: page }) => {
  const name = `Especialidade E2E ${Date.now()}`;
  const updatedName = `${name} Atualizada`;

  await page.goto("/specialties");
  await expect(page.getByRole("heading", { name: "Especialidades" })).toBeVisible();
  await page.getByRole("button", { name: "Nova especialidade" }).click();
  await page.getByLabel("Nome").fill(name);
  await page.getByLabel("Descrição").fill("Especialidade criada pelo teste E2E.");
  await page.getByRole("button", { name: "Salvar alterações" }).click();
  await expect(page.getByText(name)).toBeVisible();

  const specialtyRow = page.getByRole("row").filter({ hasText: name });
  await specialtyRow.getByRole("button", { name: "Editar" }).click();
  await page.getByLabel("Nome").fill(updatedName);
  await page.getByRole("button", { name: "Salvar alterações" }).click();
  await expect(page.getByText(updatedName)).toBeVisible();
});
