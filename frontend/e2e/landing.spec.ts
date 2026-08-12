import { expect, test } from "@playwright/test";

test.describe("landing comercial", () => {
  test("apresenta hero, showcase e CTA", async ({ page }) => {
    await page.goto("/");
    await expect(page.getByRole("heading", { name: /Transforme o WhatsApp/ })).toBeVisible();
    await expect(page.getByRole("link", { name: "Solicitar demonstração" }).first()).toBeVisible();
    await expect(page.getByRole("heading", { name: "Uma visão completa da operação." })).toBeVisible();
    await page.getByRole("tab", { name: "Agenda" }).click();
    await expect(page.getByRole("tabpanel")).toContainText("app.iarecepcao.com.br / agenda");
    await expect(page.getByRole("heading", { name: "O que você precisa saber" })).toBeVisible();
    const routine = page.locator("#problema");
    await expect(routine.getByText("Qual médico atende cardiologia?")).toBeVisible();
    await expect(routine.getByRole("heading", { name: "Um fluxo organizado, com sua equipe no controle." })).toBeVisible();
    const results = page.locator("#beneficios");
    await expect(results.getByRole("heading", { name: "IA + Equipe" })).toBeVisible();
    await expect(results.getByText("Sua equipe continua no controle.")).toBeVisible();
    const security = page.locator("#seguranca");
    await expect(security.getByRole("heading", { name: "Sua equipe continua no controle." })).toBeVisible();
    await expect(security.getByRole("heading", { name: "Controle de acesso" })).toBeVisible();
    const faq = page.locator("#faq");
    const firstQuestion = faq.getByRole("button", { name: "O paciente precisa instalar aplicativo?" });
    await firstQuestion.click();
    await expect(firstQuestion).toHaveAttribute("aria-expanded", "true");
    await expect(faq).toContainText("A experiência pode acontecer pelo WhatsApp");
    await expect(page.getByRole("heading", { name: "Próximo passo" })).toBeVisible();
    await expect(page.getByRole("contentinfo")).toContainText("IA Recepção");
  });

  test("mantém o menu e o conteúdo sem overflow no mobile", async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto("/");
    await expect(page.getByRole("summary", { name: "Abrir menu" })).toBeVisible();
    await expect(page.locator("body")).toHaveCSS("overflow-x", "visible");
    await expect(page.getByRole("heading", { name: "Veja a IA Recepção funcionando na rotina da sua clínica." })).toBeVisible();
  });

  test("Como funciona apresenta o fluxo operacional completo", async ({ page }) => {
    await page.goto("/#como-funciona");
    const flow = page.locator("#como-funciona");
    for (const label of ["Paciente", "WhatsApp", "IA Recepção", "Especialidades", "Profissionais", "Disponibilidade", "Agenda", "Recepção humana", "Painel da clínica"]) {
      await expect(flow.getByRole("heading", { name: label })).toBeVisible();
    }
    await expect(page.locator("body")).toHaveCSS("overflow-x", "visible");
  });
});
