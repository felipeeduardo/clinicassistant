import { describe, expect, it } from "vitest";
import { dashboardCustomRange, dashboardPeriodRange } from "@/lib/dashboard/period";

describe("dashboard period range", () => {
  it("creates a one-day UTC range for today", () => {
    const range = dashboardPeriodRange("today", new Date("2026-08-11T15:00:00Z"));
    expect(new Date(range.to).getTime() - new Date(range.from).getTime()).toBe(86_400_000);
  });

  it("creates a thirty-day range", () => {
    const range = dashboardPeriodRange("30d", new Date("2026-08-11T15:00:00Z"));
    expect(new Date(range.to).getTime() - new Date(range.from).getTime()).toBe(30 * 86_400_000);
  });

  it("creates an inclusive custom date range", () => {
    const range = dashboardCustomRange("2026-08-01", "2026-08-03");
    expect(range && new Date(range.to).getTime() - new Date(range.from).getTime()).toBe(3 * 86_400_000);
    expect(dashboardCustomRange("2026-08-03", "2026-08-01")).toBeNull();
  });
});
