import { e2e, expect, test } from "./fixtures";
test.skip(process.env.E2E_SKIP === "true", "E2E temporarily disabled by E2E_SKIP.");

test.skip(!e2e.password, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("cria, protege contra conflito, reagenda e cancela uma consulta", async ({ authenticatedPage: page }) => {
  await page.goto("/appointments");
  await page.getByLabel("Data", { exact: true }).fill(e2e.manifest.appointments.date);
  await page.getByLabel("Unidade").selectOption({ label: "Boa Viagem" });
  await page.getByLabel("Especialidade").selectOption({ label: "Clínico Geral" });
  await page.getByLabel("Profissional").selectOption({ label: "Dra. Ana Souza" });
  await page.getByLabel("Paciente").selectOption({ label: "Paciente E2E Principal" });

  const firstAvailableSlot = page.getByRole("button", { name: /^\d{2}:\d{2}$/ }).first();
  const selectedTime = await firstAvailableSlot.textContent();
  await firstAvailableSlot.click();
  await page.getByRole("button", { name: "Criar consulta" }).click();

  const createdRow = page.getByRole("row").filter({ hasText: selectedTime ?? "" }).filter({ hasText: "Pending" }).first();
  await expect(createdRow).toBeVisible();
  await createdRow.getByRole("button", { name: "Detalhes" }).click();
  await expect(page.getByRole("dialog", { name: "Detalhe do agendamento" })).toBeVisible();

  await page.getByLabel("Novo horário").fill("2026-08-03T12:00");
  await page.getByRole("button", { name: "Reagendar" }).click();
  await expect(page.getByRole("heading", { name: "Não foi possível concluir a operação" })).toBeVisible();

  await page.getByRole("button", { name: "Tentar novamente" }).click();
  await page.getByLabel("Novo horário").fill("2026-08-03T13:30");
  await page.getByRole("button", { name: "Reagendar" }).click();
  await expect(page.getByRole("dialog", { name: "Detalhe do agendamento" })).toBeHidden();

  const rescheduledRow = page.getByRole("row").filter({ hasText: "13:30" }).filter({ hasText: "Pending" }).first();
  await expect(rescheduledRow).toBeVisible();
  page.once("dialog", dialog => dialog.accept());
  await rescheduledRow.getByRole("button", { name: "Cancelar" }).click();
  const cancelledRow = page.getByRole("row").filter({ hasText: "13:30" }).filter({ hasText: "Cancelled" }).first();
  await expect(cancelledRow).toBeVisible();
});
