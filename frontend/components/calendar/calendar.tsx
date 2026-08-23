"use client";

import type { Appointment } from "@/lib/api/types";
import { DEFAULT_CLINIC_TIME_ZONE, type CalendarView, type CalendarFilterState } from "@/lib/scheduling/calendar";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/surfaces";
import { FormField, Input, Select } from "@/components/ui/form";
import { EmptyState, Skeleton } from "@/components/ui/states";
import { Drawer } from "@/components/ui/drawer";
import { useState } from "react";

export type CalendarAppointment = Appointment & { patientName: string; professionalName: string; unitName: string; specialtyName?: string; kind?: "appointment" | "block" | "vacation" };

type CalendarProps = {
  appointments: CalendarAppointment[];
  date: string;
  view: CalendarView;
  loading?: boolean;
  filters: CalendarFilterState;
  professionals: { id: string; name: string }[];
  units: { id: string; name: string }[];
  specialties: { id: string; name: string }[];
  onDateChange: (date: string) => void;
  onViewChange: (view: CalendarView) => void;
  onPrevious: () => void;
  onNext: () => void;
  onToday: () => void;
  onRefresh?: () => void;
  onCreate?: () => void;
  onQuickCreate?: (date: string) => void;
  onFiltersChange: (filters: CalendarFilterState) => void;
  onOpen: (appointment: CalendarAppointment) => void;
  realtimeStatus?: "offline" | "connecting" | "online";
  timeZone?: string;
};

const labels: Record<CalendarView, string> = { day: "Dia", week: "Semana", month: "Mês", list: "Lista" };
const statusLabels: Record<string, string> = { Pending: "Agendada", Confirmed: "Confirmada", Rescheduled: "Reagendada", Cancelled: "Cancelada", Completed: "Concluída", NoShow: "Não compareceu", Blocked: "Bloqueado", Unavailable: "Indisponível", Conflict: "Conflito" };
const formatTime = (value: string, timeZone: string) => new Date(value).toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit", timeZone });
const formatDate = (value: string, timeZone: string) => new Date(value).toLocaleDateString("pt-BR", { dateStyle: "full", timeZone });
const dateKey = (value: string, timeZone: string) => new Intl.DateTimeFormat("en-CA", { timeZone, year: "numeric", month: "2-digit", day: "2-digit" }).format(new Date(value));

export function CalendarShell(props: CalendarProps) {
  return <div className="grid gap-4"><Card className="grid gap-4 p-3"><CalendarToolbar {...props} /><CalendarFilterBar {...props} /><CalendarStatusLegend /></Card>{props.loading ? <CalendarSkeleton view={props.view} /> : props.appointments.length === 0 ? <CalendarEmptyState onCreate={props.onCreate} /> : <CalendarViewContent {...props} />}</div>;
}

export function CalendarStatusLegend() {
  return <div aria-label="Legenda de status das consultas" className="flex flex-wrap items-center gap-x-4 gap-y-2 border-t border-slate-100 pt-3 text-xs text-slate-600">{["Pending", "Confirmed", "Completed", "Cancelled", "Rescheduled", "NoShow"].map(status => <span className="inline-flex items-center gap-1.5" key={status}><span aria-hidden="true" className={`size-2 rounded-full ${status === "Pending" ? "bg-brand-500" : status === "Confirmed" ? "bg-emerald-500" : status === "Completed" ? "bg-slate-400" : status === "Cancelled" ? "bg-red-500" : status === "Rescheduled" ? "bg-amber-500" : "bg-orange-500"}`} />{statusLabels[status]}</span>)}</div>;
}

export function CalendarToolbar({ date, view, onDateChange, onViewChange, onPrevious, onNext, onToday, onRefresh, realtimeStatus = "offline", timeZone = DEFAULT_CLINIC_TIME_ZONE }: CalendarProps) {
  return <div className="flex flex-wrap items-center justify-between gap-3 max-sm:flex-col max-sm:items-stretch"><div className="flex items-center gap-2 max-sm:flex-wrap"><Button onClick={onToday} size="sm" variant="outline">Hoje</Button><Button aria-label="Período anterior" onClick={onPrevious} size="sm" variant="outline">←</Button><Button aria-label="Próximo período" onClick={onNext} size="sm" variant="outline">→</Button><Input aria-label="Escolher data" className="h-8 w-32" id="calendar-date" onChange={event => onDateChange(event.target.value)} type="date" value={date} /><Button aria-label="Atualizar agenda" onClick={onRefresh} size="sm" variant="ghost">Atualizar</Button></div><div className="flex items-center justify-end gap-3 max-sm:flex-wrap max-sm:justify-between"><span className="text-xs text-slate-500">Horários em {timeZone}</span><RealtimeIndicator status={realtimeStatus} /><CalendarViewSwitcher onChange={onViewChange} value={view} /></div></div>;
}

export function CalendarViewSwitcher({ value, onChange }: { value: CalendarView; onChange: (view: CalendarView) => void }) {
  return <div aria-label="Visualização do calendário" className="flex rounded-control border border-slate-200 p-0.5" role="group">{(Object.keys(labels) as CalendarView[]).map(item => <Button aria-pressed={value === item} key={item} onClick={() => onChange(item)} size="sm" variant={value === item ? "primary" : "ghost"}>{labels[item]}</Button>)}</div>;
}

export function RealtimeIndicator({ status = "offline" }: { status?: "offline" | "connecting" | "online" }) {
  const copy = status === "online" ? "Connected" : status === "connecting" ? "Reconnecting" : "Offline";
  const tone = status === "online" ? "text-emerald-700" : status === "connecting" ? "text-amber-700" : "text-slate-500";
  const dot = status === "online" ? "bg-emerald-500" : status === "connecting" ? "bg-amber-500" : "bg-slate-400";
  return <span aria-label={`Realtime ${copy}`} className={`inline-flex items-center gap-1 text-xs ${tone}`}><span aria-hidden="true" className={`size-2 rounded-full ${dot}`} />{copy}</span>;
}

export function CalendarFilterBar({ filters, professionals, units, specialties, onFiltersChange }: CalendarProps) {
  const count = Object.values(filters).filter(Boolean).length;
  const [open, setOpen] = useState(false);
  const fields = () => <div className="grid gap-3 sm:flex sm:flex-wrap sm:items-end"><FormField htmlFor="calendar-search" label="Paciente"><Input id="calendar-search" onChange={event => onFiltersChange({ ...filters, search: event.target.value })} placeholder="Buscar paciente" value={filters.search ?? ""} /></FormField><FormField htmlFor="calendar-professional" label="Profissional"><Select id="calendar-professional" onChange={event => onFiltersChange({ ...filters, professionalId: event.target.value })} value={filters.professionalId ?? ""}><option value="">Todos</option>{professionals.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</Select></FormField><FormField htmlFor="calendar-unit" label="Unidade"><Select id="calendar-unit" onChange={event => onFiltersChange({ ...filters, unitId: event.target.value })} value={filters.unitId ?? ""}><option value="">Todas</option>{units.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</Select></FormField><FormField htmlFor="calendar-specialty" label="Especialidade"><Select id="calendar-specialty" onChange={event => onFiltersChange({ ...filters, specialtyId: event.target.value })} value={filters.specialtyId ?? ""}><option value="">Todas</option>{specialties.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</Select></FormField><FormField htmlFor="calendar-status" label="Status"><Select id="calendar-status" onChange={event => onFiltersChange({ ...filters, status: event.target.value })} value={filters.status ?? ""}><option value="">Todos</option>{Object.entries(statusLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}</Select></FormField><FormField htmlFor="calendar-source" label="Origem"><Select id="calendar-source" onChange={event => onFiltersChange({ ...filters, source: event.target.value })} value={filters.source ?? ""}><option value="">Todas</option><option value="Dashboard">Dashboard</option><option value="WhatsApp">WhatsApp</option><option value="System">Sistema</option></Select></FormField>{count > 0 && <Button onClick={() => onFiltersChange({})} size="sm" variant="ghost">Limpar filtros ({count})</Button>}</div>;
  return <div className="border-t border-slate-100 pt-3"><div className="hidden sm:block">{fields()}</div><div className="sm:hidden"><Button aria-label="Abrir filtros" onClick={() => setOpen(true)} variant="outline">Filtros{count > 0 ? ` (${count})` : ""}</Button></div><Drawer description="Refine as consultas exibidas no calendário." onClose={() => setOpen(false)} open={open} title="Filtros da agenda"><div className="grid gap-4">{fields()}<Button onClick={() => setOpen(false)}>Aplicar filtros</Button></div></Drawer></div>;
}

function CalendarViewContent(props: CalendarProps) {
  if (props.view === "day") return <CalendarDayView {...props} />;
  if (props.view === "week") return <CalendarWeekView {...props} />;
  if (props.view === "month") return <CalendarMonthView {...props} />;
  return <CalendarListView {...props} />;
}

export function AppointmentCalendarEvent({ appointment, onOpen, timeZone = DEFAULT_CLINIC_TIME_ZONE }: { appointment: CalendarAppointment; onOpen: () => void; timeZone?: string }) {
  const label = appointment.kind === "vacation" ? "Férias" : appointment.kind === "block" ? "Bloqueado" : statusLabels[appointment.status] ?? appointment.status;
  const statusClass = appointment.kind ? "border-slate-300 bg-slate-100" : appointment.status === "Confirmed" ? "border-l-emerald-500 bg-emerald-50/70" : appointment.status === "Cancelled" ? "border-l-red-500 bg-red-50/70 opacity-75" : appointment.status === "Completed" ? "border-l-slate-400 bg-slate-50" : appointment.status === "Rescheduled" ? "border-l-amber-500 bg-amber-50/70" : appointment.status === "NoShow" ? "border-l-orange-500 bg-orange-50/70" : "border-l-brand-500 bg-brand-50";
  return <button aria-label={`Consulta ${formatTime(appointment.startsAt, timeZone)} com ${appointment.patientName}, status ${label}`} className={`w-full rounded-control border border-slate-200 border-l-4 px-2 py-1.5 text-left transition hover:border-brand-300 focus:outline-none focus:ring-2 focus:ring-brand-500 ${statusClass}`} onClick={onOpen} title={`${appointment.patientName} · ${appointment.professionalName} · ${label}`} type="button"><span className="block text-[11px] font-semibold tabular-nums text-slate-600">{formatTime(appointment.startsAt, timeZone)}</span><p className="truncate text-xs font-semibold text-slate-950">{appointment.patientName}</p></button>;
}

export function CalendarDayView({ appointments, onOpen, timeZone = DEFAULT_CLINIC_TIME_ZONE }: CalendarProps) {
  return <Card className="overflow-hidden p-0"><div className="border-b border-slate-200 bg-slate-50 px-4 py-3 text-sm font-semibold">Agenda do dia</div><div className="grid max-h-[min(70vh,680px)] gap-0 overflow-y-auto">{[...appointments].sort((a, b) => a.startsAt.localeCompare(b.startsAt)).map(item => <div className="grid grid-cols-[4.5rem_1fr] gap-3 border-b border-slate-100 p-3" key={item.id}><time className="pt-1 text-xs text-slate-500">{formatTime(item.startsAt, timeZone)}</time><AppointmentCalendarEvent appointment={item} onOpen={() => onOpen(item)} timeZone={timeZone} /></div>)}</div></Card>;
}

export function CalendarWeekView({ appointments, date, onOpen, onQuickCreate, timeZone = DEFAULT_CLINIC_TIME_ZONE }: CalendarProps) {
  const anchor = new Date(`${date}T12:00:00`); const start = new Date(anchor); start.setDate(anchor.getDate() - anchor.getDay()); const days = Array.from({ length: 7 }, (_, index) => { const value = new Date(start); value.setDate(start.getDate() + index); return value; });
  return <Card className="overflow-x-auto p-0"><div className="grid min-w-[760px] grid-cols-7 divide-x divide-slate-200"><div className="col-span-7 grid grid-cols-7 divide-x divide-slate-200 bg-slate-50">{days.map(day => <div className="p-3 text-center text-xs font-semibold uppercase text-slate-600" key={day.toISOString()}>{day.toLocaleDateString("pt-BR", { weekday: "short", day: "2-digit", timeZone })}</div>)}</div>{days.map(day => <div className="min-h-[28rem] space-y-2 bg-white p-2" key={day.toISOString()}>{appointments.filter(item => dateKey(item.startsAt, timeZone) === day.toISOString().slice(0, 10)).map(item => <AppointmentCalendarEvent appointment={item} key={item.id} onOpen={() => onOpen(item)} timeZone={timeZone} />)}{onQuickCreate && <button className="mt-2 w-full rounded-control border border-dashed border-slate-300 p-2 text-xs text-slate-500 hover:border-brand-400 hover:text-brand-700" onClick={() => onQuickCreate(day.toISOString().slice(0, 10))} type="button">+ horário</button>}</div>)}</div></Card>;
}

export function CalendarMonthView({ appointments, date, onOpen, onQuickCreate, timeZone = DEFAULT_CLINIC_TIME_ZONE }: CalendarProps) {
  const anchor = new Date(`${date}T12:00:00`); const start = new Date(anchor.getFullYear(), anchor.getMonth(), 1); const offset = start.getDay(); const days = Array.from({ length: 42 }, (_, index) => { const value = new Date(start); value.setDate(index - offset + 1); return value; });
  return <Card className="overflow-x-auto p-2"><div className="grid min-w-[860px] grid-cols-7 gap-px overflow-hidden rounded-control bg-slate-200">{days.map(day => { const items = appointments.filter(item => dateKey(item.startsAt, timeZone) === day.toISOString().slice(0, 10)).sort((a, b) => a.startsAt.localeCompare(b.startsAt)); const isToday = dateKey(new Date().toISOString(), timeZone) === day.toISOString().slice(0, 10); return <div className={`min-h-48 bg-white p-2 ${day.getMonth() !== anchor.getMonth() ? "bg-slate-50 text-slate-400" : ""}`} key={day.toISOString()}><div className={`flex items-center justify-between text-xs font-semibold ${isToday ? "text-brand-700" : ""}`}><span className={isToday ? "rounded-full bg-brand-100 px-2 py-0.5" : ""}>{day.getDate()}</span>{items.length > 0 && <span className="text-[10px] font-normal text-slate-400">{items.length} {items.length === 1 ? "consulta" : "consultas"}</span>}</div><div className="calendar-day-events mt-2 grid max-h-40 gap-1 overflow-y-auto pr-1">{items.map(item => <AppointmentCalendarEvent appointment={item} key={item.id} onOpen={() => onOpen(item)} timeZone={timeZone} />)}{onQuickCreate && <button aria-label={`Nova consulta em ${day.toLocaleDateString("pt-BR", { timeZone })}`} className="text-left text-xs text-slate-400 hover:text-brand-700" onClick={() => onQuickCreate(day.toISOString().slice(0, 10))} type="button">+ consulta</button>}</div></div>; })}</div></Card>;
}

export function CalendarListView({ appointments, onOpen, timeZone = DEFAULT_CLINIC_TIME_ZONE }: CalendarProps) {
  const groups = appointments.reduce<Record<string, CalendarAppointment[]>>((result, item) => { const key = formatDate(item.startsAt, timeZone); (result[key] ??= []).push(item); return result; }, {});
  return <div className="grid gap-4">{Object.entries(groups).map(([day, items]) => <Card key={day}><h2 className="mb-3 text-sm font-semibold capitalize text-slate-700">{day}</h2><div className="grid gap-2">{items.map(item => <AppointmentCalendarEvent appointment={item} key={item.id} onOpen={() => onOpen(item)} timeZone={timeZone} />)}</div></Card>)}</div>;
}

export function CalendarSkeleton({ view }: { view: CalendarView }) { return <Card aria-label={`Carregando visualização ${labels[view]}`} className="grid gap-3 p-4"><Skeleton className="h-8 w-48" /><div className={`grid gap-3 ${view === "week" || view === "month" ? "grid-cols-7" : "grid-cols-1"}`}>{Array.from({ length: view === "month" ? 14 : 6 }, (_, index) => <Skeleton className="h-20" key={index} />)}</div></Card>; }

export function CalendarEmptyState({ onCreate }: { onCreate?: () => void }) { return <EmptyState action={onCreate ? <Button onClick={onCreate}>Nova consulta</Button> : undefined} description="Você pode criar uma nova consulta ou ajustar os filtros." title="Nenhuma consulta neste período." />; }
