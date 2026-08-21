"use client";

import { useMutation, useQueries, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useState } from "react";
import { CalendarShell, type CalendarAppointment } from "@/components/calendar/calendar";
import { ProtectedShell } from "@/components/protected-shell";
import { Button } from "@/components/ui/button";
import { Drawer } from "@/components/ui/drawer";
import { FormField, FormSection, Input, Select } from "@/components/ui/form";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card } from "@/components/ui/surfaces";
import { ErrorState } from "@/components/ui/states";
import { appointmentsApi } from "@/lib/api/appointments";
import { catalogApi } from "@/lib/api/catalog";
import { patientsApi } from "@/lib/api/patients";
import type { AppointmentDetail, AppointmentRequest, SchedulePeriod } from "@/lib/api/types";
import { periodForView, shiftCalendarDate, type CalendarFilterState, type CalendarView } from "@/lib/scheduling/calendar";
import { readCalendarUrl, writeCalendarUrl } from "@/lib/scheduling/calendar-url";
import { useApi, useAuth } from "@/providers/providers";

function periodToCalendarItem(period: SchedulePeriod, kind: "block" | "vacation", professionalName: string): CalendarAppointment {
  return { id: `${kind}-${period.id}`, clinicUnitId: "", professionalId: "", specialtyId: "", patientId: "", startsAt: period.startsAt, endsAt: period.endsAt, status: kind === "vacation" ? "Unavailable" : "Blocked", source: "Schedule", notes: period.reason, patientName: period.reason ?? (kind === "vacation" ? "Férias" : "Indisponível"), professionalName, unitName: "", specialtyName: "", kind };
}

export default function AppointmentsPage() {
  const api = useApi();
  const { realtimeStatus } = useAuth();
  const client = useQueryClient();
  const today = new Date().toISOString().slice(0, 10);
  const [date, setDate] = useState(today);
  const [view, setView] = useState<CalendarView>("month");
  const [filters, setFilters] = useState<CalendarFilterState>({});
  const [detail, setDetail] = useState<AppointmentDetail | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [rescheduleAt, setRescheduleAt] = useState("");
  const [form, setForm] = useState({ clinicUnitId: "", professionalId: "", specialtyId: "", patientId: "", startsAt: "", endsAt: "", notes: "" });
  const clinic = useQuery({ queryKey: ["clinic"], queryFn: () => catalogApi.getClinic(api) });

  useEffect(() => {
    const state = readCalendarUrl(window.location.search);
    if (state.view) setView(state.view); else if (window.matchMedia("(max-width: 640px)").matches) setView("list");
    if (state.date) setDate(state.date);
    setFilters(state.filters);
  }, []);
  useEffect(() => { window.history.replaceState(null, "", writeCalendarUrl(window.location.pathname, view, date, filters)); }, [date, filters, view]);

  const period = useMemo(() => periodForView(date, view), [date, view]);
  const appointments = useQuery({ queryKey: ["appointments", period, filters.professionalId, filters.specialtyId, filters.unitId, filters.status, filters.source], queryFn: () => appointmentsApi.search(api, { page: 1, pageSize: 100, from: period.startsAt, to: period.endsAt, professionalId: filters.professionalId, specialtyId: filters.specialtyId, unitId: filters.unitId, status: filters.status, source: filters.source }) });
  const [units, specialties, professionals, patients] = useQueries({ queries: [
    { queryKey: ["units"], queryFn: () => catalogApi.listUnits(api) },
    { queryKey: ["specialties"], queryFn: () => catalogApi.listSpecialties(api) },
    { queryKey: ["professionals"], queryFn: () => catalogApi.listProfessionals(api) },
    { queryKey: ["patients"], queryFn: () => patientsApi.list(api) },
  ] });
  const professionalSchedule = useQuery({ enabled: Boolean(filters.professionalId), queryKey: ["professional-schedule", filters.professionalId, period], queryFn: () => catalogApi.getProfessionalSchedule(api, filters.professionalId!, period.startsAt, period.endsAt) });
  const displayNames = useMemo(() => ({ units: new Map((units.data ?? []).map(item => [item.id, item.name])), professionals: new Map((professionals.data ?? []).map(item => [item.id, item.name])), patients: new Map((patients.data ?? []).map(item => [item.id, item.name])), specialties: new Map((specialties.data ?? []).map(item => [item.id, item.name])) }), [patients.data, professionals.data, specialties.data, units.data]);
  const displayName = (map: Map<string, string>, id: string) => map.get(id) ?? "Não informado";
  const calendarAppointments = useMemo<CalendarAppointment[]>(() => {
    const items: CalendarAppointment[] = (appointments.data?.items ?? []).map(item => ({ ...item, patientName: displayName(displayNames.patients, item.patientId), professionalName: displayName(displayNames.professionals, item.professionalId), unitName: displayName(displayNames.units, item.clinicUnitId), specialtyName: displayName(displayNames.specialties, item.specialtyId) })).filter(item => !filters.search || item.patientName.toLocaleLowerCase().includes(filters.search.toLocaleLowerCase()));
    return items.concat((professionalSchedule.data?.blocks ?? []).map(item => periodToCalendarItem(item, "block", displayName(displayNames.professionals, filters.professionalId ?? ""))), (professionalSchedule.data?.vacations ?? []).map(item => periodToCalendarItem(item, "vacation", displayName(displayNames.professionals, filters.professionalId ?? ""))));
  }, [appointments.data?.items, displayNames, filters.professionalId, filters.search, professionalSchedule.data?.blocks, professionalSchedule.data?.vacations]);
  const slots = useQuery({ enabled: Boolean(form.professionalId), queryKey: ["availability", form.professionalId, date], queryFn: () => appointmentsApi.availability(api, form.professionalId, date) });
  const refresh = () => client.invalidateQueries({ queryKey: ["appointments"] });
  const create = useMutation({ mutationFn: (request: AppointmentRequest) => appointmentsApi.create(api, request, crypto.randomUUID()), onSuccess: async () => { await refresh(); setForm(value => ({ ...value, startsAt: "", endsAt: "", notes: "" })); setCreateOpen(false); } });
  const confirm = useMutation({ mutationFn: (item: AppointmentDetail) => appointmentsApi.confirm(api, item.appointment.id, item.version, crypto.randomUUID()), onSuccess: refresh });
  const cancel = useMutation({ mutationFn: (item: AppointmentDetail) => appointmentsApi.cancel(api, item.appointment.id, "Cancelamento administrativo", item.version, crypto.randomUUID()), onSuccess: refresh });
  const reschedule = useMutation({ mutationFn: (item: AppointmentDetail) => { const start = new Date(rescheduleAt); const duration = new Date(item.appointment.endsAt).getTime() - new Date(item.appointment.startsAt).getTime(); return appointmentsApi.reschedule(api, item.appointment.id, { startsAt: start.toISOString(), endsAt: new Date(start.getTime() + duration).toISOString(), expectedVersion: item.version, notes: item.appointment.notes }, crypto.randomUUID()); }, onSuccess: async () => { await refresh(); setDetail(null); setRescheduleAt(""); } });
  const mutationError = confirm.error ?? cancel.error ?? reschedule.error;
  const open = async (item: CalendarAppointment) => { if (!item.kind) setDetail(await appointmentsApi.detail(api, item.id)); };
  const submit = (event: React.FormEvent) => { event.preventDefault(); if (form.startsAt && form.endsAt) create.mutate({ ...form, startsAt: form.startsAt, endsAt: form.endsAt, source: "Dashboard", notes: form.notes || null }); };
  const focusCreate = (requestedDate?: string) => { if (requestedDate) setDate(requestedDate); setCreateOpen(true); };

  return <ProtectedShell><PageContainer><PageHeader actions={<Button onClick={() => focusCreate()}><Icon name="calendar" />Nova consulta</Button>} description="Crie, acompanhe e atualize consultas da clínica com disponibilidade em tempo real." pathname="/appointments" title="Agenda" />
    <Drawer description="Preencha os dados e escolha um horário disponível." onClose={() => setCreateOpen(false)} open={createOpen} title="Nova consulta"><form className="grid gap-6" onSubmit={submit} aria-label="Agendamento manual"><FormSection description="Selecione os vínculos necessários para criar uma consulta." title="Dados do atendimento"><FormField htmlFor="appointment-patient" label="Paciente"><Select id="appointment-patient" onChange={event => setForm(value => ({ ...value, patientId: event.target.value }))} value={form.patientId}><option value="">Selecione</option>{patients.data?.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</Select></FormField><FormField htmlFor="appointment-specialty" label="Especialidade"><Select id="appointment-specialty" onChange={event => setForm(value => ({ ...value, specialtyId: event.target.value }))} value={form.specialtyId}><option value="">Selecione</option>{specialties.data?.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</Select></FormField><FormField htmlFor="appointment-professional" label="Profissional"><Select id="appointment-professional" onChange={event => setForm(value => ({ ...value, professionalId: event.target.value, startsAt: "", endsAt: "" }))} value={form.professionalId}><option value="">Selecione</option>{professionals.data?.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</Select></FormField><FormField htmlFor="appointment-unit" label="Unidade"><Select id="appointment-unit" onChange={event => setForm(value => ({ ...value, clinicUnitId: event.target.value }))} value={form.clinicUnitId}><option value="">Selecione</option>{units.data?.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</Select></FormField><FormField htmlFor="appointment-date" label="Data"><Input id="appointment-date" onChange={event => setDate(event.target.value)} type="date" value={date} /></FormField><FormField htmlFor="appointment-notes" label="Observação administrativa"><Input id="appointment-notes" onChange={event => setForm(value => ({ ...value, notes: event.target.value }))} value={form.notes} /></FormField></FormSection>{slots.data && <section><h2 className="font-semibold text-slate-950">Horários disponíveis</h2><div className="mt-3 flex flex-wrap gap-2">{slots.data.map(slot => <Button key={slot.startsAt} onClick={() => setForm(value => ({ ...value, startsAt: slot.startsAt, endsAt: slot.endsAt }))} size="sm" type="button" variant={form.startsAt === slot.startsAt ? "primary" : "outline"}>{new Date(slot.startsAt).toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" })}</Button>)}</div></section>}<div className="flex justify-end"><Button disabled={create.isPending || !form.clinicUnitId || !form.specialtyId || !form.professionalId || !form.patientId || !form.startsAt} loading={create.isPending} type="submit"><Icon name="calendar" />Criar consulta</Button></div></form></Drawer>
    <section className="mt-6" aria-live="polite">{appointments.isError ? <ErrorState onRetry={() => void appointments.refetch()} title="Não foi possível carregar a agenda" /> : <CalendarShell appointments={calendarAppointments} date={date} filters={filters} loading={appointments.isLoading} onCreate={focusCreate} onDateChange={setDate} onFiltersChange={setFilters} onNext={() => setDate(shiftCalendarDate(date, view === "month" ? 30 : view === "week" ? 7 : 1))} onOpen={open} onPrevious={() => setDate(shiftCalendarDate(date, view === "month" ? -30 : view === "week" ? -7 : -1))} onQuickCreate={focusCreate} onRefresh={() => void appointments.refetch()} onToday={() => setDate(today)} onViewChange={setView} professionals={professionals.data ?? []} realtimeStatus={realtimeStatus} specialties={specialties.data ?? []} timeZone={clinic.data?.timeZone} units={units.data ?? []} view={view} />}</section>
    {mutationError && !detail && <ErrorState description={mutationError instanceof Error ? mutationError.message : "Atualize a agenda e tente novamente."} onRetry={() => { confirm.reset(); cancel.reset(); reschedule.reset(); }} title="Não foi possível concluir a operação" />}
    <Drawer description="Consulte os dados vinculados e reagende quando necessário." onClose={() => setDetail(null)} open={Boolean(detail)} title="Detalhe do agendamento">{detail && <div className="grid gap-6">{reschedule.isError && <ErrorState description="Verifique a disponibilidade do novo horário ou atualize a agenda." onRetry={() => reschedule.reset()} title="Não foi possível concluir a operação" />}<dl className="grid gap-4 text-sm sm:grid-cols-2">{[["Paciente", detail.patientName], ["Profissional", detail.professionalName], ["Unidade", detail.unitName], ["Especialidade", detail.specialtyName], ["Data", new Date(detail.appointment.startsAt).toLocaleDateString("pt-BR")], ["Horário", `${new Date(detail.appointment.startsAt).toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" })} – ${new Date(detail.appointment.endsAt).toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" })}`], ["Status", detail.appointment.status], ["Origem", detail.appointment.source], ["Versão", String(detail.version)]].map(([label, value]) => <div key={label}><dt className="text-slate-500">{label}</dt><dd className="mt-1 font-medium">{value}</dd></div>)}</dl>{detail.appointment.notes && <div><p className="text-sm text-slate-500">Observação administrativa</p><p className="mt-1 text-sm">{detail.appointment.notes}</p></div>}{detail.appointment.status === "Pending" && <div className="flex flex-wrap gap-2"><Button loading={confirm.isPending} onClick={() => confirm.mutate(detail)}>Confirmar</Button><Button loading={cancel.isPending} onClick={() => cancel.mutate(detail)} variant="outline">Cancelar</Button></div>}{detail.appointment.status !== "Cancelled" && detail.appointment.status !== "Rescheduled" && <FormField htmlFor="reschedule-at" label="Novo horário"><Input id="reschedule-at" onChange={event => setRescheduleAt(event.target.value)} type="datetime-local" value={rescheduleAt} /><Button className="mt-3" disabled={!rescheduleAt} loading={reschedule.isPending} onClick={() => reschedule.mutate(detail)}>Reagendar</Button></FormField>}</div>}</Drawer>
  </PageContainer></ProtectedShell>;
}
