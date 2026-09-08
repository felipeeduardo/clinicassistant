"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { ProtectedShell } from "@/components/protected-shell";
import { PatientForm } from "@/features/patients/patient-form";
import { patientsApi } from "@/lib/api/patients";
import { useDebouncedValue } from "@/hooks/use-debounced-value";
import type { Patient, PatientDetail } from "@/lib/api/types";
import { can } from "@/lib/auth/permissions";
import { useApi, useAuth } from "@/providers/providers";
import { Button } from "@/components/ui/button";
import { DataTable, Pagination, TableCell, TableHeader } from "@/components/ui/data-table";
import { Drawer } from "@/components/ui/drawer";
import { FormField, Input, Select } from "@/components/ui/form";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { StatusBadge } from "@/components/ui/surfaces";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/states";
import { FilterToolbar, RowActionButton, RowActions } from "@/components/ui/admin";

export default function PatientsPage() {
  const api = useApi(); const { user } = useAuth();
  const [search, setSearch] = useState(""); const [consentStatus, setConsentStatus] = useState(""); const [page, setPage] = useState(1);
  const [editing, setEditing] = useState<Patient | null | undefined>(); const [selected, setSelected] = useState<PatientDetail | null>(null);
  const debouncedSearch = useDebouncedValue(search);
  const patients = useQuery({ placeholderData: keepPreviousData, queryKey: ["patients", page, debouncedSearch, consentStatus], queryFn: () => patientsApi.search(api, { page, pageSize: 20, search: debouncedSearch, consentStatus }) });
  const allowed = user ? can(user, "manageCatalog") : false;

  const viewPatient = async (patient: Patient) => setSelected(await patientsApi.detail(api, patient.id));

  return <ProtectedShell><PageContainer><PageHeader actions={allowed ? <Button onClick={() => setEditing(null)}><Icon name="patients" />Novo paciente</Button> : undefined} description="Consulte e gerencie os pacientes vinculados à clínica." pathname="/patients" title="Pacientes" />
    {editing !== undefined && allowed && <PatientForm patient={editing ?? undefined} onCancel={() => setEditing(undefined)} onComplete={() => setEditing(undefined)} />}
    <FilterToolbar><FormField htmlFor="patient-search" label="Buscar paciente"><Input id="patient-search" onChange={event => { setSearch(event.target.value); setPage(1); }} placeholder="Nome, telefone ou e-mail" value={search} /></FormField><FormField htmlFor="patient-consent" label="Status"><Select id="patient-consent" onChange={event => { setConsentStatus(event.target.value); setPage(1); }} value={consentStatus}><option value="">Todos</option><option value="Granted">Concedido</option><option value="Revoked">Revogado</option><option value="Unknown">Não informado</option></Select></FormField></FilterToolbar>
    <section className="mt-4">{patients.isError ? <ErrorState title="Não foi possível carregar os pacientes" onRetry={() => void patients.refetch()} /> : patients.isLoading ? <Skeleton className="h-80" /> : patients.data?.items.length === 0 ? <EmptyState action={allowed ? <Button onClick={() => setEditing(null)}><Icon name="patients" />Novo paciente</Button> : undefined} description="Ajuste os filtros ou cadastre o primeiro paciente para começar." title="Nenhum paciente encontrado" /> : <DataTable><thead><tr><TableHeader>Paciente</TableHeader><TableHeader>Telefone</TableHeader><TableHeader className="hidden lg:table-cell">E-mail</TableHeader><TableHeader className="hidden md:table-cell">Último contato</TableHeader><TableHeader>Status</TableHeader><TableHeader className="text-right"><span>Ações</span></TableHeader></tr></thead><tbody>{patients.data?.items.map(patient => <tr className="transition-colors hover:bg-slate-50" key={patient.id}><TableCell className="font-medium">{patient.name}</TableCell><TableCell>{maskPhone(patient.phone)}</TableCell><TableCell className="hidden lg:table-cell">{maskEmail(patient.email)}</TableCell><TableCell className="hidden md:table-cell">{formatDate(patient.lastContactAt)}</TableCell><TableCell><StatusBadge tone={patient.consentStatus === "Granted" ? "success" : patient.consentStatus === "Revoked" ? "danger" : "warning"}>{consentLabel(patient.consentStatus)}</StatusBadge></TableCell><TableCell><RowActions><RowActionButton displayLabel="Ver" icon="eye" tone="view" label={`Visualizar ${patient.name}`} onClick={() => void viewPatient(patient)} />{allowed && <RowActionButton displayLabel="Editar" icon="edit" tone="edit" label={`Editar ${patient.name}`} onClick={() => setEditing(patient)} />}</RowActions></TableCell></tr>)}</tbody></DataTable>}</section>
    <div className="mt-4"><Pagination hasNext={Boolean(patients.data && page * patients.data.pageSize < patients.data.totalCount)} hasPrevious={page > 1} onNext={() => setPage(value => value + 1)} onPrevious={() => setPage(value => value - 1)} page={page} /></div>
    <Drawer description={selected ? `Origem: ${selected.source}` : undefined} onClose={() => setSelected(null)} open={Boolean(selected)} title={selected?.patient.name ?? "Paciente"}>{selected && <PatientDetailPanel detail={selected} />}</Drawer>
  </PageContainer></ProtectedShell>;
}

function PatientDetailPanel({ detail }: { detail: PatientDetail }) { return <div className="grid gap-6"><section><h3 className="font-semibold text-slate-950">Contato</h3><dl className="mt-3 grid gap-3 text-sm sm:grid-cols-2"><div><dt className="text-slate-500">Telefone</dt><dd>{maskPhone(detail.patient.phone)}</dd></div><div><dt className="text-slate-500">E-mail</dt><dd>{maskEmail(detail.patient.email)}</dd></div><div><dt className="text-slate-500">Último contato</dt><dd>{detail.lastContactAt ? new Date(detail.lastContactAt).toLocaleString("pt-BR") : "—"}</dd></div></dl></section><section><h3 className="font-semibold text-slate-950">Próximos agendamentos</h3><ul className="mt-3 grid gap-2 text-sm">{detail.upcomingAppointments.map(item => <li className="rounded-control bg-slate-50 p-3" key={item.id}>{new Date(item.startsAt).toLocaleString("pt-BR")} · {item.status}</li>)}{detail.upcomingAppointments.length === 0 && <li className="text-slate-600">Nenhum agendamento futuro.</li>}</ul></section><section><h3 className="font-semibold text-slate-950">Conversas</h3><ul className="mt-3 grid gap-2 text-sm">{detail.conversations.map(item => <li className="rounded-control bg-slate-50 p-3" key={item.id}>{item.status} · {item.automationMode}</li>)}{detail.conversations.length === 0 && <li className="text-slate-600">Nenhuma conversa.</li>}</ul></section></div>; }
function consentLabel(status: string) { return status === "Granted" ? "Concedido" : status === "Revoked" ? "Revogado" : status === "Pending" ? "Pendente" : "Não informado"; }
function maskPhone(phone: string) { return phone.length < 5 ? "••••" : `${phone.slice(0, 3)}••••${phone.slice(-2)}`; }
function maskEmail(email?: string | null) { if (!email) return "—"; const [local, domain] = email.split("@"); return domain ? `${local.slice(0, 1)}•••@${domain}` : "••••"; }
function formatDate(value?: string | null) { return value ? new Date(value).toLocaleDateString("pt-BR") : "—"; }
