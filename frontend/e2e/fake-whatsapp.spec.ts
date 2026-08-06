import { e2e, expect, test } from "./fixtures";

test.skip(!e2e.password, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("valida e enfileira uma mensagem pelo Fake WhatsApp", async ({ authenticatedPage: page }) => {
  await page.goto("/integrations/whatsapp");
  await expect(page.getByRole("heading", { name: "Integração WhatsApp" })).toBeVisible();
  await expect(page.getByText("Fake")).toBeVisible();

  await page.getByRole("button", { name: "Validar configuração" }).click();
  await expect(page.getByRole("status")).toHaveText("Operação solicitada.");

  await page.getByRole("button", { name: "Enviar teste" }).click();
  await expect(page.getByRole("status")).toHaveText("Operação solicitada.");
  await expect(page.getByRole("alert")).not.toBeVisible();

  await page.goto("/conversations");
  await page.getByLabel("Buscar conversa").fill("Paciente E2E Principal");
  await page.getByRole("button", { name: "Buscar" }).click();
  await page.getByRole("button", { name: "Paciente E2E Principal" }).first().click();
  await expect(page.getByText("Test window opened.")).toBeVisible();
  const inboundTestMessage = page.locator("article").filter({ hasText: "Test window opened." });
  await inboundTestMessage.getByRole("button", { name: "Marcar como lida" }).click();
  await expect(inboundTestMessage.getByRole("button", { name: "Marcar como lida" })).toBeHidden();
});
