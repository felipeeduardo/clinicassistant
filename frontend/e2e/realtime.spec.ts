import { e2e, expect, loginAs, test } from "./fixtures";
test.skip(process.env.E2E_SKIP === "true", "E2E temporarily disabled by E2E_SKIP.");

test.skip(!e2e.password, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("uma alteração de catálogo atualiza outra sessão pelo SignalR", async ({ browser }) => {
  const suffix = Date.now();
  const name = `Realtime E2E ${suffix}`;
  const observerContext = await browser.newContext();
  const actorContext = await browser.newContext();
  const observer = await observerContext.newPage();
  const actor = await actorContext.newPage();

  try {
    await loginAs(observer, e2e.manifest.users.adminEmail);
    await observer.goto("/specialties");
    await expect(observer.getByRole("heading", { name: "Especialidades" })).toBeVisible();
    await expect(observer.getByText("Tempo real conectado")).toBeVisible();

    await loginAs(actor, e2e.manifest.users.adminEmail);
    await actor.goto("/specialties");
    await actor.getByRole("button", { name: "Nova especialidade" }).click();
    await actor.getByLabel("Nome").fill(name);
    await actor.getByLabel("Descrição").fill("Evento SignalR E2E.");
    await actor.getByRole("button", { name: "Salvar alterações" }).click();
    await expect(actor.getByText(name)).toBeVisible();

    await expect(observer.getByText(name)).toBeVisible({ timeout: 10_000 });
  } finally {
    await observerContext.close();
    await actorContext.close();
  }
});

test("fila humana, dashboard e auditoria são atualizados em outra sessão", async ({ browser }) => {
  const suffix = Date.now();
  const specialtyName = `Auditoria realtime ${suffix}`;
  const observerContext = await browser.newContext();
  const actorContext = await browser.newContext();
  const queueObserver = await observerContext.newPage();
  const dashboardObserver = await observerContext.newPage();
  const auditObserver = await observerContext.newPage();
  const actor = await actorContext.newPage();

  try {
    await loginAs(queueObserver, e2e.manifest.users.adminEmail);
    await queueObserver.goto("/conversations");
    await expect(queueObserver.getByText("Tempo real conectado")).toBeVisible();
    const queueSummary = queueObserver.getByText(/conversa\(s\) aguardando atendimento\./);
    await expect(queueSummary).not.toHaveText("0 conversa(s) aguardando atendimento.");
    const queueBefore = Number((await queueSummary.textContent())?.match(/^\d+/)?.[0]);

    await loginAs(dashboardObserver, e2e.manifest.users.adminEmail);
    await dashboardObserver.goto("/dashboard");
    await expect(dashboardObserver.getByText("Tempo real conectado")).toBeVisible();
    const dashboardQueue = dashboardObserver.getByText("Fila humana", { exact: true }).locator("xpath=ancestor::section[1]").locator("p").last();
    await expect(dashboardQueue).not.toHaveText("—");
    const dashboardBefore = Number(await dashboardQueue.textContent());

    await loginAs(auditObserver, e2e.manifest.users.adminEmail);
    await auditObserver.goto("/audit");
    await expect(auditObserver.getByText("Tempo real conectado")).toBeVisible();
    await expect(auditObserver.locator("tbody tr").first()).toBeVisible();
    const auditRowsBefore = await auditObserver.locator("tbody tr").count();

    await loginAs(actor, e2e.manifest.users.adminEmail);
    await actor.goto("/conversations");
    await actor.getByLabel("Buscar conversa").fill("Paciente E2E Secundário");
    await actor.getByRole("button", { name: "Buscar" }).click();
    await actor.getByRole("button", { name: "Paciente E2E Secundário" }).first().click();
    await actor.getByRole("button", { name: "Liberar" }).click();

    await expect.poll(async () => Number((await queueSummary.textContent())?.match(/^\d+/)?.[0])).toBe(queueBefore + 1);
    await expect.poll(async () => Number(await dashboardQueue.textContent())).toBe(dashboardBefore + 1);

    await actor.goto("/specialties");
    await actor.getByRole("button", { name: "Nova especialidade" }).click();
    await actor.getByLabel("Nome").fill(specialtyName);
    await actor.getByLabel("Descrição").fill("Auditoria atualizada por SignalR.");
    await actor.getByRole("button", { name: "Salvar alterações" }).click();
    await expect.poll(async () => auditObserver.locator("tbody tr").count()).toBe(auditRowsBefore + 1);
  } finally {
    await observerContext.close();
    await actorContext.close();
  }
});
