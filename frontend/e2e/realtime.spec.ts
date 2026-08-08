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
    await Promise.allSettled([observerContext.close(), actorContext.close()]);
  }
});
