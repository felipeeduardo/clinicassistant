import { e2e, expect, test } from "./fixtures";
test.skip(process.env.E2E_SKIP === "true", "E2E temporarily disabled by E2E_SKIP.");

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

  const updatedRow = page.getByRole("row").filter({ hasText: updatedName });
  await updatedRow.getByRole("button", { name: "Dependências" }).click();
  await expect(page.getByText("Pode ser desativada com segurança")).toBeVisible();
  await page.getByLabel("Fechar painel").click();
  await updatedRow.getByRole("button", { name: "Desativar" }).click();
  await expect(updatedRow.getByText("Inativa")).toBeVisible();
});

test("exibe dependências da especialidade vinculada no seed E2E", async ({ authenticatedPage: page }) => {
  await page.goto("/specialties");
  const specialtyRow = page.getByRole("row").filter({ hasText: "Clínico Geral" });
  await specialtyRow.getByRole("button", { name: "Dependências" }).click();
  await expect(page.getByText("Profissionais vinculados")).toBeVisible();
  await expect(page.getByText("Existem dependências ativas")).toBeVisible();
});

test("cria uma unidade administrativa", async ({ authenticatedPage: page }) => {
  const name = `Unidade E2E ${Date.now()}`;
  await page.goto("/units");
  await page.getByRole("button", { name: "Nova unidade" }).click();
  await page.getByLabel("Nome").fill(name);
  await page.getByLabel("Endereço").fill("Rua da Unidade E2E, 100");
  await page.getByLabel("Telefone").fill("+558100000099");
  await page.getByRole("button", { name: "Salvar alterações" }).click();
  await expect(page.getByRole("row").filter({ hasText: name })).toBeVisible();
});
