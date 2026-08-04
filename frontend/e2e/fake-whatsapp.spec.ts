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
});
