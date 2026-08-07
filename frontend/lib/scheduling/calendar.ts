export type CalendarView = "day" | "week" | "month" | "list";

export function shiftCalendarDate(value: string, days: number) {
  const date = new Date(`${value}T12:00:00`);
  date.setDate(date.getDate() + days);
  return date.toISOString().slice(0, 10);
}

export function toUtcPeriod(date: string) {
  return { startsAt: `${date}T00:00:00.000Z`, endsAt: `${date}T23:59:59.999Z` };
}

export function periodForView(date: string, view: CalendarView) {
  if (view === "day" || view === "list") return toUtcPeriod(date);
  const anchor = new Date(`${date}T12:00:00`); const start = new Date(anchor);
  if (view === "week") start.setDate(anchor.getDate() - anchor.getDay()); else start.setDate(1);
  const end = new Date(start); if (view === "week") end.setDate(start.getDate() + 7); else end.setMonth(start.getMonth() + 1, 1);
  return { startsAt: `${start.toISOString().slice(0, 10)}T00:00:00.000Z`, endsAt: `${end.toISOString().slice(0, 10)}T00:00:00.000Z` };
}

export function activeFilterCount(filters: Record<string, string | undefined>) {
  return Object.values(filters).filter(value => Boolean(value?.trim())).length;
}
