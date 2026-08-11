import { describe, expect, it } from "vitest";
import { readCalendarUrl, writeCalendarUrl } from "@/lib/scheduling/calendar-url";

describe("calendar URL state", () => {
  it("reads only supported views and non-sensitive filters", () => {
    const state = readCalendarUrl("?view=week&date=2026-08-11&professionalId=pro-1&search=Ana%20Silva");
    expect(state.view).toBe("week");
    expect(state.date).toBe("2026-08-11");
    expect(state.filters).toMatchObject({ professionalId: "pro-1", search: "Ana Silva" });
  });

  it("ignores invalid date/view values", () => {
    const state = readCalendarUrl("?view=invalid&date=11-08-2026");
    expect(state.view).toBeUndefined();
    expect(state.date).toBeUndefined();
  });

  it("serializes view, date and active filters", () => {
    const url = writeCalendarUrl("/appointments", "month", "2026-08-11", { unitId: "unit-1", status: "Confirmed" });
    expect(url).toContain("view=month");
    expect(url).toContain("date=2026-08-11");
    expect(url).toContain("unitId=unit-1");
    expect(url).toContain("status=Confirmed");
  });
});
