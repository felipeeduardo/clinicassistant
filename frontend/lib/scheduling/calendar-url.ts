import type { CalendarFilterState, CalendarView } from "./calendar";

const views: CalendarView[] = ["day", "week", "month", "list"];

export function readCalendarUrl(search: string) {
  const params = new URLSearchParams(search);
  const requestedView = params.get("view");
  const view = views.includes(requestedView as CalendarView) ? requestedView as CalendarView : undefined;
  const date = params.get("date");
  return {
    view,
    date: date && /^\d{4}-\d{2}-\d{2}$/.test(date) ? date : undefined,
    filters: {
      professionalId: params.get("professionalId") || undefined,
      specialtyId: params.get("specialtyId") || undefined,
      unitId: params.get("unitId") || undefined,
      status: params.get("status") || undefined,
      source: params.get("source") || undefined,
      search: params.get("search") || undefined,
    } satisfies CalendarFilterState,
  };
}

export function writeCalendarUrl(pathname: string, view: CalendarView, date: string, filters: CalendarFilterState) {
  const params = new URLSearchParams({ view, date });
  Object.entries(filters).forEach(([key, value]) => { if (value?.trim()) params.set(key, value); });
  return `${pathname}?${params.toString()}`;
}
