"use client";

import { useMutation, useQueries, useQuery, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { ProtectedShell } from "@/components/protected-shell";
import { ProfessionalForm } from "@/features/catalog/catalog-forms";
import { catalogApi } from "@/lib/api/catalog";
import { can } from "@/lib/auth/permissions";
import type { AvailabilityRuleRequest, Professional, ProfessionalSchedule, SchedulePeriodRequest } from "@/lib/api/types";
import { useApi, useAuth } from "@/providers/providers";
import { Button } from "@/components/ui/button";
import { DataTable, TableCell, TableHeader } from "@/components/ui/data-table";
import { Drawer } from "@/components/ui/drawer";
import { FormField, Input, Select } from "@/components/ui/form";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { StatusBadge } from "@/components/ui/surfaces";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/states";
import { ScheduleImportPanel } from "@/components/scheduling/schedule-import-panel";
import { FilterToolbar, RowActionButton, RowActions } from "@/components/ui/admin";

export default function ProfessionalsPage() {
  const api = useApi(); const { user } = useAuth();
  const router = useRouter();
  const professionals = useQuery({ queryKey: ["professionals"], enabled: user?.role !== "PlatformAdmin", queryFn: () => catalogApi.listProfessionals(api) });
  const [units, specialties] = useQueries({ queries: [{ queryKey: ["units"], enabled: user?.role !== "PlatformAdmin", queryFn: () => catalogApi.listUnits(api) }, { queryKey: ["specialties"], enabled: user?.role !== "PlatformAdmin", queryFn: () => catalogApi.listSpecialties(api) }] });
  const [editing, setEditing] = useState<Professional | null | undefined>(); const [schedule, setSchedule] = useState<{ professional: Professional; data: ProfessionalSchedule } | null>(null);
  const [search, setSearch] = useState(""); const [status, setStatus] = useState(""); const [specialtyId, setSpecialtyId] = useState(""); const [unitId, setUnitId] = useState("");
  const allowed = can(user, "manageCatalog"); const formReady = Boolean(units.data && specialties.data);
  const visibleProfessionals = professionals.data?.filter(item => {
    const term = search.trim().toLocaleLowerCase();
    return (!term || [item.name, item.email, item.registrationNumber].some(value => value.toLocaleLowerCase().includes(term))) && (!status || item.status === status) && (!unitId || item.clinicUnitId === unitId) && (!specialtyId || item.specialtyIds.includes(specialtyId));
  });
  const showSchedule = (professional: Professional) => { router.push(`/professionals/${professional.id}/schedule`); };

  return <ProtectedShell><PageContainer><PageHeader actions={<>{allowed && <ScheduleImportPanel mode="button" />}{allowed && <Button disabled={!formReady} onClick={() => setEditing(null)}><Icon name="professionals" />Novo profissional</Button>}</>} description="Cadastre profissionais, gerencie disponibilidade e consulte suas agendas." pathname="/professionals" title="Profissionais" />
    {editing !== undefined && formReady && allowed && <ProfessionalForm professional={editing ?? undefined} specialties={specialties.data!} units={units.data!} onCancel={() => setEditing(undefined)} onComplete={() => setEditing(undefined)} />}
    <section className="grid gap-4"><FilterToolbar><FormField htmlFor="professional-search" label="Buscar profissional"><Input id="professional-search" onChange={event => setSearch(event.target.value)} placeholder="Nome, registro ou e-mail" value={search} /></FormField><FormField htmlFor="professional-specialty" label="Especialidade"><Select id="professional-specialty" onChange={event => setSpecialtyId(event.target.value)} value={specialtyId}><option value="">Todas</option>{specialties.data?.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</Select></FormField><FormField htmlFor="professional-unit" label="Unidade"><Select id="professional-unit" onChange={event => setUnitId(event.target.value)} value={unitId}><option value="">Todas</option>{units.data?.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</Select></FormField><FormField htmlFor="professional-status" label="Status"><Select id="professional-status" onChange={event => setStatus(event.target.value)} value={status}><option value="">Todos</option><option value="Active">Ativo</option><option value="Inactive">Inativo</option></Select></FormField></FilterToolbar>{professionals.isError ? <ErrorState title="Não foi possível carregar os profissionais" onRetry={() => void professionals.refetch()} /> : professionals.isLoading ? <Skeleton className="h-80" /> : visibleProfessionals?.length === 0 ? <EmptyState action={allowed ? <Button onClick={() => setEditing(null)}><Icon name="professionals" />Novo profissional</Button> : undefined} description="Ajuste os filtros ou cadastre um profissional para disponibilizar agenda e atendimento." title="Nenhum profissional encontrado" /> : <DataTable><thead><tr><TableHeader>Profissional</TableHeader><TableHeader className="hidden lg:table-cell">Especialidade</TableHeader><TableHeader className="hidden md:table-cell">Unidade</TableHeader><TableHeader>Registro</TableHeader><TableHeader>Status</TableHeader><TableHeader className="text-right">Ações</TableHeader></tr></thead><tbody>{visibleProfessionals?.map(item => <tr className="transition-colors hover:bg-slate-50" key={item.id}><TableCell><span className="font-medium">{item.name}</span><span className="block text-xs text-slate-500 lg:hidden">{item.email}</span></TableCell><TableCell className="hidden lg:table-cell">{item.specialtyIds.map(id => specialties.data?.find(specialty => specialty.id === id)?.name).filter(Boolean).join(", ") || "—"}</TableCell><TableCell className="hidden md:table-cell">{units.data?.find(unit => unit.id === item.clinicUnitId)?.name ?? "—"}</TableCell><TableCell>{item.registrationNumber}</TableCell><TableCell><StatusBadge tone={item.status === "Active" ? "success" : "neutral"}>{item.status === "Active" ? "Ativo" : "Inativo"}</StatusBadge></TableCell><TableCell><RowActions><RowActionButton displayLabel="Agenda" icon="calendar" tone="agenda" label={`Agenda de ${item.name}`} onClick={() => void showSchedule(item)} />{allowed && <RowActionButton displayLabel="Editar" icon="edit" tone="edit" label={`Editar ${item.name}`} onClick={() => setEditing(item)} />}</RowActions></TableCell></tr>)}</tbody></DataTable>}</section>
    <Drawer description="Resumo e administração da disponibilidade profissional." onClose={() => setSchedule(null)} open={Boolean(schedule)} title={schedule ? `Agenda de ${schedule.professional.name}` : "Agenda"}>{schedule && <div className="grid gap-6"><div className="grid gap-3 sm:grid-cols-3"><ScheduleMetric label="Consultas" value={schedule.data.appointments.length} /><ScheduleMetric label="Bloqueios" value={schedule.data.blocks.length} /><ScheduleMetric label="Férias" value={schedule.data.vacations.length} /></div>{allowed && <ScheduleAdministration professional={schedule.professional} />}</div>}</Drawer>
  </PageContainer></ProtectedShell>;
}
function ScheduleMetric({ label, value }: { label: string; value: number }) { return <div className="rounded-control bg-slate-50 p-4"><p className="text-sm text-slate-600">{label}</p><p className="mt-1 text-2xl font-semibold text-slate-950">{value}</p></div>; }

function ScheduleAdministration({ professional }: { professional: Professional }) {
  const api = useApi(); const client = useQueryClient();
  const [availability, setAvailability] = useState({ dayOfWeek: "1", startTime: "08:00", endTime: "17:00", slotDurationMinutes: "30" });
  const [block, setBlock] = useState({ startsAt: "", endsAt: "", reason: "" }); const [vacation, setVacation] = useState({ startsAt: "", endsAt: "", reason: "" });
  const [rules, blocks, vacations] = useQueries({ queries: [
    { queryKey: ["professional-availability", professional.id], queryFn: () => catalogApi.listAvailabilityRules(api, professional.id) },
    { queryKey: ["professional-blocks", professional.id], queryFn: () => catalogApi.listScheduleBlocks(api, professional.id) },
    { queryKey: ["professional-vacations", professional.id], queryFn: () => catalogApi.listVacations(api, professional.id) },
  ] });
  const refresh = async () => { await Promise.all([client.invalidateQueries({ queryKey: ["professional-availability", professional.id] }), client.invalidateQueries({ queryKey: ["professional-blocks", professional.id] }), client.invalidateQueries({ queryKey: ["professional-vacations", professional.id] })]); };
  const addRule = useMutation({ mutationFn: (request: AvailabilityRuleRequest) => catalogApi.addAvailabilityRule(api, professional.id, request), onSuccess: refresh });
  const addBlock = useMutation({ mutationFn: (request: SchedulePeriodRequest) => catalogApi.addScheduleBlock(api, professional.id, request), onSuccess: async () => { setBlock({ startsAt: "", endsAt: "", reason: "" }); await refresh(); } });
  const addVacation = useMutation({ mutationFn: (request: SchedulePeriodRequest) => catalogApi.addVacation(api, professional.id, request), onSuccess: async () => { setVacation({ startsAt: "", endsAt: "", reason: "" }); await refresh(); } });
  const removeBlock = useMutation({ mutationFn: (id: string) => catalogApi.deleteScheduleBlock(api, professional.id, id), onSuccess: refresh });
  const removeVacation = useMutation({ mutationFn: (id: string) => catalogApi.deleteVacation(api, professional.id, id), onSuccess: refresh });
  return <section aria-label="Administração da agenda" className="grid gap-6 border-t border-slate-200 pt-6"><div><h3 className="font-semibold text-slate-950">Disponibilidade semanal</h3><p className="mt-1 text-sm text-slate-600">Adicione períodos recorrentes para a agenda do profissional.</p><div className="mt-3 grid gap-3 sm:grid-cols-2"><FormField htmlFor="availability-day" label="Dia da semana"><Select id="availability-day" onChange={event => setAvailability(value => ({ ...value, dayOfWeek: event.target.value }))} value={availability.dayOfWeek}>{[[0, "Domingo"], [1, "Segunda-feira"], [2, "Terça-feira"], [3, "Quarta-feira"], [4, "Quinta-feira"], [5, "Sexta-feira"], [6, "Sábado"]].map(([value, label]) => <option key={value} value={value}>{label}</option>)}</Select></FormField><FormField htmlFor="availability-duration" label="Duração do slot (minutos)"><Input id="availability-duration" min="5" onChange={event => setAvailability(value => ({ ...value, slotDurationMinutes: event.target.value }))} type="number" value={availability.slotDurationMinutes} /></FormField><FormField htmlFor="availability-start" label="Início"><Input id="availability-start" onChange={event => setAvailability(value => ({ ...value, startTime: event.target.value }))} type="time" value={availability.startTime} /></FormField><FormField htmlFor="availability-end" label="Fim"><Input id="availability-end" onChange={event => setAvailability(value => ({ ...value, endTime: event.target.value }))} type="time" value={availability.endTime} /></FormField></div><Button className="mt-3" loading={addRule.isPending} onClick={() => addRule.mutate({ dayOfWeek: Number(availability.dayOfWeek), startTime: availability.startTime, endTime: availability.endTime, slotDurationMinutes: Number(availability.slotDurationMinutes), active: true })}>Adicionar disponibilidade</Button>{rules.data && <ul className="mt-3 grid gap-2 text-sm">{rules.data.map(rule => <li className="rounded-control bg-slate-50 px-3 py-2" key={rule.id}>{dayOfWeekLabel(rule.dayOfWeek)}: {rule.startTime.slice(0, 5)}–{rule.endTime.slice(0, 5)} · {rule.slotDurationMinutes} min</li>)}</ul>}</div>
    <PeriodAdministration add={addBlock} entries={blocks.data ?? []} idPrefix="block" label="Bloqueios" onRemove={id => removeBlock.mutate(id)} period={block} setPeriod={setBlock} submitLabel="Adicionar bloqueio" />
    <PeriodAdministration add={addVacation} entries={vacations.data ?? []} idPrefix="vacation" label="Férias" onRemove={id => removeVacation.mutate(id)} period={vacation} setPeriod={setVacation} submitLabel="Adicionar férias" />
  </section>;
}

function dayOfWeekLabel(value: number | string) {
  const labels = ["domingo", "segunda", "terça", "quarta", "quinta", "sexta", "sábado"];
  if (typeof value === "number") return labels[value] ?? "dia";
  const index: Record<string, number> = { Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 };
  return labels[index[value] ?? -1] ?? value.toLowerCase();
}

function PeriodAdministration({ add, entries, idPrefix, label, onRemove, period, setPeriod, submitLabel }: { add: { mutate: (request: SchedulePeriodRequest) => void; isPending: boolean }; entries: { id: string; startsAt: string; endsAt: string; reason?: string | null }[]; idPrefix: string; label: string; onRemove: (id: string) => void; period: { startsAt: string; endsAt: string; reason: string }; setPeriod: React.Dispatch<React.SetStateAction<{ startsAt: string; endsAt: string; reason: string }>>; submitLabel: string }) {
  const valid = Boolean(period.startsAt && period.endsAt && new Date(period.endsAt) > new Date(period.startsAt));
  return <div><h3 className="font-semibold text-slate-950">{label}</h3><div className="mt-3 grid gap-3 sm:grid-cols-2"><FormField htmlFor={`${idPrefix}-starts`} label="Início"><Input id={`${idPrefix}-starts`} onChange={event => setPeriod(value => ({ ...value, startsAt: event.target.value }))} type="datetime-local" value={period.startsAt} /></FormField><FormField htmlFor={`${idPrefix}-ends`} label="Fim"><Input id={`${idPrefix}-ends`} onChange={event => setPeriod(value => ({ ...value, endsAt: event.target.value }))} type="datetime-local" value={period.endsAt} /></FormField><FormField htmlFor={`${idPrefix}-reason`} label="Motivo"><Input id={`${idPrefix}-reason`} onChange={event => setPeriod(value => ({ ...value, reason: event.target.value }))} value={period.reason} /></FormField></div><Button className="mt-3" disabled={!valid} loading={add.isPending} onClick={() => add.mutate({ startsAt: new Date(period.startsAt).toISOString(), endsAt: new Date(period.endsAt).toISOString(), reason: period.reason || null })}>{submitLabel}</Button>{entries.length > 0 && <ul className="mt-3 grid gap-2 text-sm">{entries.map(entry => <li className="flex items-center justify-between gap-3 rounded-control bg-slate-50 px-3 py-2" key={entry.id}><span>{new Date(entry.startsAt).toLocaleString("pt-BR")}–{new Date(entry.endsAt).toLocaleString("pt-BR")}{entry.reason ? ` · ${entry.reason}` : ""}</span><Button onClick={() => onRemove(entry.id)} size="sm" variant="ghost">Remover</Button></li>)}</ul>}</div>;
}
