import { expect, test as base } from "@playwright/test";
import manifest from "../../database/seeds/e2e/manifest.json";

export const e2e = { manifest, password: process.env.E2E_DEFAULT_PASSWORD };
export async function loginAs(page: import("@playwright/test").Page, email: string) {
  if (!e2e.password) throw new Error("E2E_DEFAULT_PASSWORD must be configured for authenticated Playwright tests.");
  await page.goto("/login");
  await page.getByLabel("E-mail").fill(email);
  await page.getByLabel("Senha").fill(e2e.password);
  const loginResponse = page.waitForResponse(response => response.url().includes("/api/auth/login") && response.request().method() === "POST");
  await page.getByRole("button", { name: "Entrar" }).click();
  const response = await loginResponse;
  if (!response.ok()) {
    const body = await response.json().catch(() => ({})) as { title?: string; code?: string; traceId?: string };
    throw new Error(`E2E login failed for ${email}: HTTP ${response.status()} ${body.code ?? body.title ?? "unknown_error"}${body.traceId ? ` (traceId=${body.traceId})` : ""}`);
  }
  await expect(page).toHaveURL(/\/dashboard$/);
}
export const test = base.extend<{ authenticatedPage: import("@playwright/test").Page }>({
  authenticatedPage: async ({ page }, use) => {
    await loginAs(page, e2e.manifest.users.adminEmail);
    // eslint-disable-next-line react-hooks/rules-of-hooks -- `use` is Playwright's fixture callback.
    await use(page);
  },
});
export { expect };
