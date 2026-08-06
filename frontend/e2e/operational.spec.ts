import { e2e, expect, test } from "./fixtures";
test.skip(process.env.E2E_SKIP === "true", "E2E temporarily disabled by E2E_SKIP.");

test.skip(!e2e.password, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("busca e abre o detalhe de um paciente do tenant E2E", async ({ authenticatedPage: page }) => {
  await page.goto("/patients");
  await expect(page.getByRole("heading", { name: "Pacientes" })).toBeVisible();
  await page.getByRole("textbox", { name: "Buscar pacientes" }).fill("Paciente E2E Principal");
  await expect(page.getByRole("button", { name: "Paciente E2E Principal" })).toBeVisible();
  await page.getByRole("button", { name: "Paciente E2E Principal" }).click();
  await expect(page.getByRole("heading", { name: "Paciente E2E Principal" })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Contato" })).toBeVisible();
});

test("acessa a agenda com os dados determinísticos", async ({ authenticatedPage: page }) => {
  await page.goto("/appointments");
  await expect(page.getByRole("heading", { name: "Agenda" })).toBeVisible();
});

test("opera a conversa da fila humana definida no manifesto", async ({ authenticatedPage: page }) => {
  await page.goto("/conversations");
  await expect(page.getByRole("heading", { name: "Inbox de conversas" })).toBeVisible();
  await expect(page.locator("body")).not.toContainText(e2e.manifest.tenantIds.isolated);
  await page.getByLabel("Buscar conversa").fill("Paciente E2E Principal");
  await page.getByRole("button", { name: "Buscar" }).click();
  await page.getByRole("button", { name: "Paciente E2E Principal" }).first().click();

  await expect(page.getByRole("button", { name: "Assumir" })).toBeVisible();
  await page.getByRole("button", { name: "Assumir" }).click();
  await expect(page.getByText("Human · automação Human")).toBeVisible();

  await page.getByLabel("Prioridade").selectOption("Urgent");
  await expect(page.getByText("Urgent").last()).toBeVisible();

  await page.getByRole("button", { name: "Pausar" }).click();
  await expect(page.getByRole("button", { name: "Retomar" })).toBeVisible();
  await page.getByRole("button", { name: "Retomar" }).click();
  await expect(page.getByRole("button", { name: "Pausar" })).toBeVisible();

  await page.getByLabel("Transferir conversa").selectOption({ label: "Operador E2E" });
  await page.getByRole("button", { name: "Transferir" }).click();
  await expect(page.getByText("Human · automação Human")).toBeVisible();

  await page.getByRole("button", { name: "Liberar" }).click();
  await expect(page.getByText("WaitingHuman · automação Human")).toBeVisible();

  await page.getByRole("button", { name: "Encerrar" }).click();
  await expect(page.getByRole("button", { name: "Reabrir" })).toBeVisible();
  await page.getByRole("button", { name: "Reabrir" }).click();
  await expect(page.getByRole("button", { name: "Encerrar" })).toBeVisible();

  const manualMessage = `Mensagem manual E2E ${Date.now()}`;
  await page.getByLabel("Mensagem manual").fill(manualMessage);
  await page.getByRole("button", { name: "Enviar" }).click();
  await expect(page.getByText(manualMessage)).toBeVisible();
});
