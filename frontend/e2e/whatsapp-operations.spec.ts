import { e2e, expect, test } from "./fixtures";
test.skip(process.env.E2E_SKIP === "true", "E2E temporarily disabled by E2E_SKIP.");

test.skip(!e2e.password, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("cria e encontra um template administrativo", async ({ authenticatedPage: page }) => {
  const name = `e2e-template-${Date.now()}`;
  const contentSid = `HX${Date.now().toString(16).padStart(32, "0")}`;
  await page.goto("/integrations/whatsapp/templates");
  await expect(page.getByRole("heading", { name: "Templates WhatsApp" })).toBeVisible();
  await page.getByLabel("Nome").fill(name);
  await page.getByLabel("Content SID").fill(contentSid);
  await page.getByLabel("Idioma").fill("pt_BR");
  await page.getByRole("button", { name: "Criar template" }).click();
  await expect(page.getByText(name)).toBeVisible();

  const createdTemplate = page.getByRole("row").filter({ hasText: name });
  await createdTemplate.getByRole("link", { name: "Editar" }).click();
  await expect(page.getByRole("heading", { name: "Editar template" })).toBeVisible();
  await expect(page.getByLabel("Content SID")).toBeDisabled();

  const updatedName = `${name}-atualizado`;
  await page.getByLabel("Nome").fill(updatedName);
  await page.getByRole("button", { name: "Salvar alterações" }).click();
  await expect(page.getByRole("status")).toHaveText("Alterações salvas.");

  await page.getByRole("button", { name: "Voltar" }).click();
  const updatedTemplate = page.getByRole("row").filter({ hasText: updatedName });
  await updatedTemplate.getByRole("button", { name: "Ativar" }).click();
  await expect(updatedTemplate.getByRole("button", { name: "Desativar" })).toBeVisible();
  await updatedTemplate.getByRole("button", { name: "Desativar" }).click();
  await expect(updatedTemplate.getByRole("button", { name: "Ativar" })).toBeVisible();
});
