import { describe, expect, it } from "vitest";
import { commercialConfig } from "@/lib/commercial/config";

describe("commercial configuration", () => {
  it("defaults to a safe demo mode without public pricing", () => {
    expect(commercialConfig.mode).toBe("demo");
    expect(commercialConfig.showPricing).toBe(false);
    expect(commercialConfig.ctaLabel).toBe("Solicitar demonstração");
  });
});
