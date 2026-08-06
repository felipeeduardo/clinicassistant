import { e2e, expect, loginAs, test } from "./fixtures";
test.skip(process.env.E2E_SKIP === "true", "E2E temporarily disabled by E2E_SKIP.");

test.skip(!e2e.password, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("administrador de plataforma cria um tenant", async ({ page }) => {
  const suffix = Date.now().toString(36);
  const tenantName = `Clínica E2E ${suffix}`;

  await loginAs(page, e2e.manifest.users.platformAdminEmail);
  await page.goto("/platform/onboarding");
  await expect(page.getByRole("heading", { name: "Novo tenant" })).toBeVisible();

  await page.getByLabel("Tenant").fill(tenantName);
  await page.getByLabel("Slug").fill(`clinica-e2e-${suffix}`);
  await page.getByLabel("Razão social").fill(`${tenantName} LTDA`);
  await page.getByLabel("Nome fantasia").fill(tenantName);
  await page.getByLabel("Documento").fill(`TEST-${suffix}`);
  await page.getByLabel("E-mail da clínica").fill(`clinic-${suffix}@example.test`);
  await page.getByLabel("Telefone da clínica").fill("+558100000001");
  await page.getByLabel("Unidade", { exact: true }).fill("Unidade E2E");
  await page.getByLabel("Endereço da unidade").fill("Rua de Teste, 100");
  await page.getByLabel("Telefone da unidade").fill("+558100000002");
  await page.getByLabel("Administrador").fill("Administrador E2E");
  await page.getByLabel("E-mail do administrador").fill(`admin-${suffix}@example.test`);
  await page.getByLabel("Senha temporária").fill("SenhaTemporariaE2E2026");
  await page.getByRole("button", { name: "Criar tenant" }).click();

  await expect(page.getByRole("status")).toHaveText("Onboarding concluído.");
});
