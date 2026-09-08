"use client";

import { useMemo, useState } from "react";
import { useQueries, useQuery } from "@tanstack/react-query";
import { CalendarShell, type CalendarAppointment } from "@/components/calendar/calendar";
import { ProtectedShell } from "@/components/protected-shell";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { ErrorState, Skeleton } from "@/components/ui/states";
import { Icon } from "@/components/ui/icon";
import { Button } from "@/components/ui/button";
import { appointmentsApi } from "@/lib/api/appointments";
import { catalogApi } from "@/lib/api/catalog";
import { patientsApi } from "@/lib/api/patients";
import { periodForView, shiftCalendarDate, type CalendarView } from "@/lib/scheduling/calendar";
import { useApi, useAuth } from "@/providers/providers";
import type { AvailabilityRule } from "@/lib/api/types";

type AvailabilityView = "day" | "week" | "month";
type AvailabilityOccurrence = { id: string; date: string; startsAt: string; endsAt: string; slotDurationMinutes: number };

export default function ProfessionalScheduleClient({ professionalId }: { professionalId: string }) {
  const api = useApi();
  const { realtimeStatus } = useAuth();
  const today = new Date().toISOString().slice(0, 10);
  const [date, setDate] = useState(today);
  const [view, setView] = useState<CalendarView>("week");
  const [availabilityDate, setAvailabilityDate] = useState(today);
  const [availabilitySearchDate, setAvailabilitySearchDate] = useState(today);
  const [availabilityView, setAvailabilityView] = useState<AvailabilityView>("week");
  const professional = useQuery({ queryKey: ["professional", professionalId], queryFn: () => catalogApi.getProfessional(api, professionalId) });
  const clinic = useQuery({ queryKey: ["clinic"], queryFn: () => catalogApi.getClinic(api) });
  const [units, specialties, patients] = useQueries({ queries: [
    { queryKey: ["units"], queryFn: () => catalogApi.listUnits(api) },
    { queryKey: ["specialties"], queryFn: () => catalogApi.listSpecialties(api) },
    { queryKey: ["patients"], queryFn: () => patientsApi.list(api) },
  ] });
  const period = useMemo(() => periodForView(date, view), [date, view]);
  const schedule = useQuery({ enabled: Boolean(professional.data), queryKey: ["professional-schedule", professionalId, period], queryFn: () => catalogApi.getProfessionalSchedule(api, professionalId, period.startsAt, period.endsAt) });
  const availability = useQuery({ enabled: Boolean(professional.data), queryKey: ["professional-availability", professionalId], queryFn: () => catalogApi.listAvailabilityRules(api, professionalId) });
  const appointments = useQuery({ enabled: Boolean(professional.data), queryKey: ["professional-appointments", professionalId, period], queryFn: () => appointmentsApi.search(api, { page: 1, pageSize: 100, professionalId, from: period.startsAt, to: period.endsAt }) });
  const names = useMemo(() => ({ patients: new Map((patients.data ?? []).map(item => [item.id, item.name])), units: new Map((units.data ?? []).map(item => [item.id, item.name])), specialties: new Map((specialties.data ?? []).map(item => [item.id, item.name])) }), [patients.data, units.data, specialties.data]);
  const items = useMemo<CalendarAppointment[]>(() => {
    const result: CalendarAppointment[] = (appointments.data?.items ?? []).map(item => ({ ...item, patientName: names.patients.get(item.patientId) ?? "Paciente", professionalName: professional.data?.name ?? "Profissional", unitName: names.units.get(item.clinicUnitId) ?? "Unidade", specialtyName: names.specialties.get(item.specialtyId) ?? "Especialidade" }));
    return result.concat((schedule.data?.blocks ?? []).map(item => ({ id: `block-${item.id}`, clinicUnitId: "", professionalId, specialtyId: "", patientId: "", startsAt: item.startsAt, endsAt: item.endsAt, status: "Blocked", source: "Schedule", notes: item.reason, patientName: item.reason ?? "Bloqueado", professionalName: professional.data?.name ?? "Profissional", unitName: "", specialtyName: "", kind: "block" as const })), (schedule.data?.vacations ?? []).map(item => ({ id: `vacation-${item.id}`, clinicUnitId: "", professionalId, specialtyId: "", patientId: "", startsAt: item.startsAt, endsAt: item.endsAt, status: "Unavailable", source: "Schedule", notes: item.reason, patientName: item.reason ?? "Férias", professionalName: professional.data?.name ?? "Profissional", unitName: "", specialtyName: "", kind: "vacation" as const })));
  }, [appointments.data?.items, names, professional.data?.name, professionalId, schedule.data?.blocks, schedule.data?.vacations]);
  if (professional.isLoading) return <ProtectedShell><PageContainer><Skeleton className="h-64" /></PageContainer></ProtectedShell>;
  if (professional.isError || !professional.data) return <ProtectedShell><PageContainer><ErrorState title="Não foi possível carregar o profissional" onRetry={() => void professional.refetch()} /></PageContainer></ProtectedShell>;
  const loading = availability.isLoading || appointments.isLoading || schedule.isLoading;
  const hasScheduleError = availability.isError || appointments.isError || schedule.isError || clinic.isError || units.isError || specialties.isError || patients.isError;
  return <ProtectedShell><PageContainer><PageHeader actions={<Button href="/professionals" variant="outline"><Icon name="arrowRight" className="rotate-180" />Voltar</Button>} description={`${professional.data.name} · ${professional.data.registrationNumber}. Disponibilidade e consultas no fuso da clínica.`} pathname={`/professionals/${professionalId}/schedule`} title={`Agenda de ${professional.data.name}`} /><section className="mt-6 grid gap-4">{hasScheduleError && <ErrorState title="Não foi possível carregar todos os dados da agenda" description="Atualize a página. Se continuar, verifique a sessão e o retorno da API de agenda/disponibilidade." onRetry={() => { void availability.refetch(); void appointments.refetch(); void schedule.refetch(); void clinic.refetch(); void units.refetch(); void specialties.refetch(); void patients.refetch(); }} />}<WeeklyAvailability date={availabilityDate} loading={availability.isLoading} onDateChange={setAvailabilitySearchDate} onSearch={() => setAvailabilityDate(availabilitySearchDate)} onViewChange={setAvailabilityView} rules={availability.data ?? []} searchDate={availabilitySearchDate} timeZone={clinic.data?.timeZone ?? "America/Recife"} view={availabilityView} /><CalendarShell appointments={items} date={date} filters={{ professionalId }} loading={loading} onDateChange={setDate} onFiltersChange={() => undefined} onNext={() => setDate(shiftCalendarDate(date, view === "month" ? 30 : view === "week" ? 7 : 1))} onPrevious={() => setDate(shiftCalendarDate(date, view === "month" ? -30 : view === "week" ? -7 : -1))} onToday={() => setDate(today)} onViewChange={setView} professionals={[{ id: professionalId, name: professional.data.name }]} specialties={specialties.data ?? []} units={units.data ?? []} realtimeStatus={realtimeStatus} timeZone={clinic.data?.timeZone} view={view} onOpen={() => undefined} /></section></PageContainer></ProtectedShell>;
}

function WeeklyAvailability({ date, loading, onDateChange, onSearch, onViewChange, rules, searchDate, timeZone, view }: { date: string; loading: boolean; onDateChange: (date: string) => void; onSearch: () => void; onViewChange: (view: AvailabilityView) => void; rules: AvailabilityRule[]; searchDate: string; timeZone: string; view: AvailabilityView }) {
  const period = useMemo(() => periodForView(date, view), [date, view]);
  const occurrences = useMemo(() => materializeAvailability(rules, period.startsAt.slice(0, 10), period.endsAt.slice(0, 10), timeZone), [period.endsAt, period.startsAt, rules, timeZone]);
  const grouped = useMemo(() => occurrences.reduce<Record<string, AvailabilityOccurrence[]>>((result, item) => { (result[item.date] ??= []).push(item); return result; }, {}), [occurrences]);
  const days = Object.entries(grouped).sort(([left], [right]) => left.localeCompare(right));
  return <section aria-label="Disponibilidade do profissional" className="rounded-panel border border-slate-200 bg-white p-4 shadow-sm"><div className="flex flex-wrap items-start justify-between gap-4"><div><h2 className="font-semibold text-slate-950">Disponibilidade</h2><p className="mt-1 text-sm text-slate-600">Horários recorrentes importados, exibidos por data.</p></div>{!loading && <span className="text-xs text-slate-500">{rules.length} {rules.length === 1 ? "regra" : "regras"} · {occurrences.length} {occurrences.length === 1 ? "ocorrência" : "ocorrências"}</span>}</div><div className="mt-4 flex flex-wrap items-end justify-between gap-3 border-t border-slate-100 pt-4"><div className="flex flex-wrap items-end gap-2"><label className="grid gap-1 text-sm font-medium text-slate-700" htmlFor="availability-search-date">Data<input id="availability-search-date" aria-label="Data da disponibilidade" className="h-9 rounded-control border border-slate-200 px-3 text-sm font-normal text-slate-950" onChange={event => onDateChange(event.target.value)} type="date" value={searchDate} /></label><Button onClick={onSearch} size="sm">Buscar</Button></div><div className="flex rounded-control border border-slate-200 p-0.5" role="group" aria-label="Visualização da disponibilidade">{(["day", "week", "month"] as AvailabilityView[]).map(item => <Button aria-pressed={view === item} key={item} onClick={() => onViewChange(item)} size="sm" variant={view === item ? "primary" : "ghost"}>{item === "day" ? "Dia" : item === "week" ? "Semana" : "Mês"}</Button>)}</div></div>{loading ? <div className="mt-4 h-24 animate-pulse rounded-control bg-slate-100" /> : rules.length === 0 ? <p className="mt-4 text-sm text-slate-500">Nenhuma disponibilidade recorrente cadastrada.</p> : days.length === 0 ? <p className="mt-4 text-sm text-slate-500">Nenhuma disponibilidade neste período.</p> : <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-3">{days.map(([day, items]) => <article className="rounded-control border border-emerald-100 bg-emerald-50 p-3 text-sm text-emerald-950" key={day}><div className="flex items-baseline justify-between gap-3"><h3 className="font-semibold capitalize">{formatAvailabilityDate(day, timeZone)}</h3><span className="text-xs text-emerald-800">{items.length} {items.length === 1 ? "período" : "períodos"}</span></div><div className="mt-3 flex flex-wrap gap-2">{items.sort((left, right) => left.startsAt.localeCompare(right.startsAt)).map(item => <span className="rounded-full border border-emerald-200 bg-white px-2.5 py-1 text-xs tabular-nums text-emerald-900" key={item.id}>{formatTime(item.startsAt, timeZone)}–{formatTime(item.endsAt, timeZone)} · {item.slotDurationMinutes} min</span>)}</div></article>)}</div>}</section>;
}

function dayOfWeekLabel(value: number | string) {
  const labels = ["domingo", "segunda-feira", "terça-feira", "quarta-feira", "quinta-feira", "sexta-feira", "sábado"];
  return labels[dayOfWeekIndex(value)] ?? String(value).toLowerCase();
}

function dayOfWeekIndex(value: number | string) {
  if (typeof value === "number") return value;
  if (/^\d+$/.test(value)) return Number(value);
  const index: Record<string, number> = { Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 };
  return index[value] ?? -1;
}

function materializeAvailability(rules: AvailabilityRule[], startsAt: string, endsAt: string, timeZone: string): AvailabilityOccurrence[] {
  const result: AvailabilityOccurrence[] = [];
  const cursor = new Date(`${startsAt}T00:00:00Z`);
  const end = new Date(`${endsAt}T00:00:00Z`);
  while (cursor < end) {
    const date = cursor.toISOString().slice(0, 10);
    const dayOfWeek = cursor.getUTCDay();
    for (const rule of rules.filter(item => dayOfWeekIndex(item.dayOfWeek) === dayOfWeek && item.active)) {
      result.push({ id: `availability-${rule.id}-${date}`, date, startsAt: zonedTimeToIso(date, rule.startTime, timeZone), endsAt: zonedTimeToIso(date, rule.endTime, timeZone), slotDurationMinutes: rule.slotDurationMinutes });
    }
    cursor.setUTCDate(cursor.getUTCDate() + 1);
  }
  return result;
}

function formatAvailabilityDate(value: string, timeZone: string) {
  return new Date(`${value}T12:00:00Z`).toLocaleDateString("pt-BR", { day: "2-digit", month: "2-digit", weekday: "long", timeZone });
}

function formatTime(value: string, timeZone: string) {
  return new Date(value).toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit", timeZone });
}

function zonedTimeToIso(date: string, time: string, timeZone: string) {
  const [year, month, day] = date.split("-").map(Number);
  const [hour, minute] = time.slice(0, 5).split(":").map(Number);
  const guess = Date.UTC(year, month - 1, day, hour, minute);
  const parts = new Intl.DateTimeFormat("en-US", { timeZone, year: "numeric", month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit", hourCycle: "h23" }).formatToParts(new Date(guess));
  const values = Object.fromEntries(parts.filter(part => part.type !== "literal").map(part => [part.type, Number(part.value)]));
  const renderedAsUtc = Date.UTC(values.year, values.month - 1, values.day, values.hour, values.minute);
  return new Date(guess - (renderedAsUtc - guess)).toISOString();
}
