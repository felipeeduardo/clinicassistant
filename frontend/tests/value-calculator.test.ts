import { describe, expect, it } from "vitest";
import { calculateOperationalImpact } from "@/lib/commercial/value-calculator";

describe("operational value calculator", () => {
  it("calculates repetitive time and potential value", () => {
    const result = calculateOperationalImpact({ attendants: 2, monthlyTeamCost: 6000, requestsPerDay: 30, minutesPerRequest: 4, workDays: 22, automatablePercent: 50, monthlyInvestment: 500 });
    expect(result.repetitiveMinutes).toBe(2640);
    expect(result.releasedHours).toBe(22);
    expect(result.timeValue).toBe(412.5);
    expect(result.estimatedImpact).toBe(-87.5);
  });

  it("clamps invalid and excessive percentages safely", () => {
    const result = calculateOperationalImpact({ attendants: 0, monthlyTeamCost: 0, requestsPerDay: 10, minutesPerRequest: 5, workDays: 20, automatablePercent: 200, monthlyInvestment: 0 });
    expect(result.releasedHours).toBeCloseTo(16.6667);
    expect(calculateOperationalImpact({ attendants: 1, monthlyTeamCost: 100, requestsPerDay: 0, minutesPerRequest: 5, workDays: 20, automatablePercent: -10, monthlyInvestment: 0 }).releasedHours).toBe(0);
  });
});
