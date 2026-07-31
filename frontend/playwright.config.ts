import { defineConfig } from "@playwright/test";
export default defineConfig({ testDir: "./e2e", timeout: 30_000, reporter: "list", use: { baseURL: "http://127.0.0.1:3000", trace: "retain-on-failure", screenshot: "only-on-failure" }, projects: [{ name: "chromium", use: { browserName: "chromium" } }], webServer: { command: "npm run dev", port: 3000, reuseExistingServer: true } });
