import { expect, test } from "@playwright/test";

test.describe("landing comercial", () => {
  test("apresenta hero, showcase e CTA", async ({ page }) => {
    await page.goto("/");
    await expect(page.getByRole("heading", { name: /Transforme o WhatsApp/ })).toBeVisible();
    await expect(page.getByRole("link", { name: "Solicitar demonstração" }).first()).toBeVisible();
    await expect(page.getByRole("heading", { name: "Uma visão completa da operação." })).toBeVisible();
    await page.getByRole("tab", { name: "Agenda" }).click();
    await expect(page.getByRole("tabpanel")).toContainText("app.clinicassistant.local / agenda");
    await expect(page.getByRole("heading", { name: "O que você precisa saber" })).toBeVisible();
  });

  test("mantém o menu e o conteúdo sem overflow no mobile", async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto("/");
    await expect(page.getByRole("summary", { name: "Abrir menu" })).toBeVisible();
    await expect(page.locator("body")).toHaveCSS("overflow-x", "visible");
    await expect(page.getByRole("heading", { name: "Veja o Clinic Assistant funcionando na rotina da sua clínica." })).toBeVisible();
  });
});
