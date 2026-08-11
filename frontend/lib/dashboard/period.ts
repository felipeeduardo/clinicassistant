export type DashboardPeriod = "today" | "7d" | "30d" | "month" | "custom";

export const dashboardPeriodLabels: Record<DashboardPeriod, string> = { today: "Hoje", "7d": "7 dias", "30d": "30 dias", month: "Este mês", custom: "Personalizado" };

export function dashboardPeriodRange(period: DashboardPeriod, now = new Date()) {
  const end = new Date(now); end.setDate(end.getDate() + 1); end.setHours(0, 0, 0, 0);
  const start = new Date(end);
  if (period === "today") start.setDate(start.getDate() - 1);
  if (period === "7d") start.setDate(start.getDate() - 7);
  if (period === "30d") start.setDate(start.getDate() - 30);
  if (period === "month") start.setDate(1);
  return { from: start.toISOString(), to: end.toISOString() };
}

export function dashboardCustomRange(from: string, to: string) {
  const start = new Date(`${from}T00:00:00`);
  const end = new Date(`${to}T00:00:00`);
  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime()) || end <= start) return null;
  end.setDate(end.getDate() + 1);
  return { from: start.toISOString(), to: end.toISOString() };
}
