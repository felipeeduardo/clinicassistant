import { describe, expect, it } from "vitest";
import nextConfig from "../next.config";

describe("security headers", () => {
  it("restricts content sources without wildcards or unsafe-eval", async () => {
    const rules = await nextConfig.headers?.();
    const headers = rules?.[0]?.headers ?? [];
    const csp = headers.find((header) => header.key === "Content-Security-Policy")?.value;

    expect(csp).toContain("default-src 'self'");
    expect(csp).toContain("frame-ancestors 'none'");
    expect(csp).toContain("connect-src 'self' http://localhost:8080 ws://localhost:8080");
    expect(csp).not.toContain("unsafe-eval");
    expect(csp).not.toContain("*");
    expect(headers).toContainEqual({ key: "X-Frame-Options", value: "DENY" });
    expect(headers).toContainEqual({ key: "X-Content-Type-Options", value: "nosniff" });
  });
});
