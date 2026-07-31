import { expect, test as base } from "@playwright/test";
import manifest from "../../database/seeds/e2e/manifest.json";

export const e2e = { manifest, password: process.env.E2E_DEFAULT_PASSWORD };
export const test = base.extend<{ authenticatedPage: import("@playwright/test").Page }>({
  authenticatedPage: async ({ page }, use) => {
    if (!e2e.password) throw new Error("E2E_DEFAULT_PASSWORD must be configured for authenticated Playwright tests.");
    await page.goto("/login");
    await page.getByLabel("E-mail").fill(e2e.manifest.users.adminEmail);
    await page.getByLabel("Senha").fill(e2e.password);
    await page.getByRole("button", { name: "Entrar" }).click();
    await expect(page).toHaveURL(/\/dashboard$/);
    // eslint-disable-next-line react-hooks/rules-of-hooks -- `use` is Playwright's fixture callback.
    await use(page);
  },
});
export { expect };
