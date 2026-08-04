import { e2e, expect, loginAs, test } from "./fixtures";

test.skip(!e2e.password, "E2E_DEFAULT_PASSWORD is required for authenticated scenarios.");

test("usuário do tenant isolado não visualiza pacientes do tenant principal", async ({ page }) => {
  await loginAs(page, e2e.manifest.users.viewerEmail);
  await page.goto("/patients");
  await expect(page.getByRole("heading", { name: "Pacientes" })).toBeVisible();
  await expect(page.getByText("Paciente Outro Tenant")).toBeVisible();
  await expect(page.getByText("Paciente E2E Principal")).not.toBeVisible();
  await expect(page.getByRole("button", { name: "Novo paciente" })).not.toBeVisible();
});
