export type ValueCalculatorInputs = { attendants: number; monthlyTeamCost: number; requestsPerDay: number; minutesPerRequest: number; workDays: number; automatablePercent: number; monthlyInvestment: number };
export type ValueCalculatorResult = { repetitiveMinutes: number; automatableMinutes: number; releasedHours: number; hourlyCost: number; timeValue: number; estimatedImpact: number };

const nonNegative = (value: number) => Number.isFinite(value) && value > 0 ? value : 0;

export function calculateOperationalImpact(inputs: ValueCalculatorInputs): ValueCalculatorResult {
  const requests = nonNegative(inputs.requestsPerDay) * nonNegative(inputs.workDays);
  const repetitiveMinutes = requests * nonNegative(inputs.minutesPerRequest);
  const automatableMinutes = repetitiveMinutes * Math.min(100, Math.max(0, nonNegative(inputs.automatablePercent))) / 100;
  const releasedHours = automatableMinutes / 60;
  const hourlyCost = nonNegative(inputs.monthlyTeamCost) / Math.max(1, nonNegative(inputs.attendants) * 160);
  const timeValue = releasedHours * hourlyCost;
  return { repetitiveMinutes, automatableMinutes, releasedHours, hourlyCost, timeValue, estimatedImpact: timeValue - nonNegative(inputs.monthlyInvestment) };
}
