"use client";

import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { LoadingRow, RequestFeedback } from "@/components/request-feedback";
import { ProtectedShell } from "@/components/protected-shell";
import { PatientForm } from "@/features/patients/patient-form";
import { patientsApi } from "@/lib/api/patients";
import type { Patient, PatientDetail } from "@/lib/api/types";
import { can } from "@/lib/auth/permissions";
import { useApi, useAuth } from "@/providers/providers";

export default function PatientsPage() {
  const api = useApi(); const { user } = useAuth();
  const [search, setSearch] = useState(""); const [consentStatus, setConsentStatus] = useState(""); const [page, setPage] = useState(1);
  const patients = useQuery({ queryKey: ["patients", page, search, consentStatus], queryFn: () => patientsApi.search(api, { page, pageSize: 20, search, consentStatus }) });
  const [editing, setEditing] = useState<Patient | null | undefined>();
  const [selected, setSelected] = useState<PatientDetail | null>(null);
  const allowed = user ? can(user, "manageCatalog") : false;
  return <ProtectedShell><div className="flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-bold">Pacientes</h1><p className="mt-1 text-slate-600">Dados administrativos do tenant atual.</p></div>{allowed && <button className="rounded bg-blue-700 px-4 py-2 text-white" onClick={() => setEditing(null)}>Novo paciente</button>}</div>
    {editing !== undefined && allowed && <PatientForm patient={editing ?? undefined} onComplete={() => setEditing(undefined)} onCancel={() => setEditing(undefined)} />}
    <div className="mt-6 flex flex-wrap gap-3 rounded bg-white p-3 shadow"><input className="rounded border p-2" aria-label="Buscar pacientes" value={search} onChange={event => { setSearch(event.target.value); setPage(1); }} placeholder="Nome, telefone ou e-mail" /><select className="rounded border p-2" aria-label="Filtrar consentimento" value={consentStatus} onChange={event => { setConsentStatus(event.target.value); setPage(1); }}><option value="">Todos os consentimentos</option><option value="Granted">Concedido</option><option value="Revoked">Revogado</option><option value="Unknown">Não informado</option></select></div>
    <div className="mt-3 overflow-x-auto rounded bg-white shadow"><table className="w-full text-left"><thead><tr className="border-b"><th className="p-3">Nome</th><th className="p-3">Telefone</th><th className="p-3">E-mail</th><th className="p-3">Consentimento</th><th className="p-3">Origem</th>{allowed && <th className="p-3"><span className="sr-only">Ações</span></th>}</tr></thead><tbody>{patients.isLoading && <LoadingRow columns={allowed ? 6 : 5} />}{patients.data?.items.map(patient => <tr className="border-b" key={patient.id}><td className="p-3"><button className="text-left text-blue-700 underline" onClick={async () => setSelected(await patientsApi.detail(api, patient.id))}>{patient.name}</button></td><td className="p-3">{maskPhone(patient.phone)}</td><td className="p-3">{maskEmail(patient.email)}</td><td className="p-3">{patient.consentStatus}</td><td className="p-3">{patient.source}</td>{allowed && <td className="p-3"><button className="text-blue-700 underline" onClick={() => setEditing(patient)}>Editar</button></td>}</tr>)}{!patients.isLoading && patients.data?.items.length === 0 && <LoadingRow columns={allowed ? 6 : 5} />}</tbody></table><RequestFeedback error={patients.error} /></div>
    <div className="mt-3 flex items-center justify-between"><span>{patients.data?.totalCount ?? 0} paciente(s)</span><div className="flex gap-2"><button className="rounded border px-3 py-1 disabled:opacity-40" disabled={page === 1} onClick={() => setPage(value => value - 1)}>Anterior</button><button className="rounded border px-3 py-1 disabled:opacity-40" disabled={!patients.data || page * patients.data.pageSize >= patients.data.totalCount} onClick={() => setPage(value => value + 1)}>Próxima</button></div></div>
    {selected && <section className="mt-5 rounded bg-white p-4 shadow"><div className="flex justify-between gap-3"><h2 className="text-lg font-bold">{selected.patient.name}</h2><button className="underline" onClick={() => setSelected(null)}>Fechar</button></div><p className="mt-2 text-sm text-slate-600">Origem: {selected.source} · Último contato: {selected.lastContactAt ? new Date(selected.lastContactAt).toLocaleString("pt-BR") : "—"}</p><h3 className="mt-4 font-semibold">Próximos agendamentos</h3><ul className="mt-1 list-disc pl-5 text-sm">{selected.upcomingAppointments.map(item => <li key={item.id}>{new Date(item.startsAt).toLocaleString("pt-BR")} — {item.status}</li>)}{selected.upcomingAppointments.length === 0 && <li>Nenhum agendamento futuro.</li>}</ul><h3 className="mt-4 font-semibold">Conversas</h3><ul className="mt-1 list-disc pl-5 text-sm">{selected.conversations.map(item => <li key={item.id}>{item.status} · {item.automationMode}</li>)}{selected.conversations.length === 0 && <li>Nenhuma conversa.</li>}</ul></section>}
  </ProtectedShell>;
}

function maskPhone(phone: string) { return phone.length < 5 ? "••••" : `${phone.slice(0, 3)}••••${phone.slice(-2)}`; }
function maskEmail(email?: string | null) { if (!email) return "—"; const [local, domain] = email.split("@"); return domain ? `${local.slice(0, 1)}•••@${domain}` : "••••"; }
