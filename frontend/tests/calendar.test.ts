import { describe, expect, it } from "vitest";
import { activeFilterCount, periodForView, shiftCalendarDate, toUtcPeriod } from "@/lib/scheduling/calendar";

describe("calendar utilities", () => {
  it("navigates dates without changing the local calendar day", () => {
    expect(shiftCalendarDate("2026-08-03", 1)).toBe("2026-08-04");
    expect(shiftCalendarDate("2026-08-03", -1)).toBe("2026-08-02");
  });
  it("builds an inclusive UTC day period", () => {
    expect(toUtcPeriod("2026-08-03")).toEqual({ startsAt: "2026-08-03T00:00:00.000Z", endsAt: "2026-08-03T23:59:59.999Z" });
  });
  it("counts only active filters", () => {
    expect(activeFilterCount({ professional: "p1", unit: "", status: undefined, patient: " Ana " })).toBe(2);
  });
  it("builds server periods for week and month views", () => {
    expect(periodForView("2026-08-05", "week")).toEqual({ startsAt: "2026-08-02T00:00:00.000Z", endsAt: "2026-08-09T00:00:00.000Z" });
    expect(periodForView("2026-08-05", "month")).toEqual({ startsAt: "2026-08-01T00:00:00.000Z", endsAt: "2026-09-01T00:00:00.000Z" });
  });
});
