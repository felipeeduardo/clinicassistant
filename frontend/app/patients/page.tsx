"use client";

import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { LoadingRow, RequestFeedback } from "@/components/request-feedback";
import { ProtectedShell } from "@/components/protected-shell";
import { PatientForm } from "@/features/patients/patient-form";
import { patientsApi } from "@/lib/api/patients";
import type { Patient } from "@/lib/api/types";
import { can } from "@/lib/auth/permissions";
import { useApi, useAuth } from "@/providers/providers";

export default function PatientsPage() {
  const api = useApi(); const { user } = useAuth();
  const patients = useQuery({ queryKey: ["patients"], queryFn: () => patientsApi.list(api) });
  const [editing, setEditing] = useState<Patient | null | undefined>();
  const allowed = user ? can(user, "manageCatalog") : false;
  return <ProtectedShell><div className="flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-bold">Pacientes</h1><p className="mt-1 text-slate-600">Dados administrativos do tenant atual.</p></div>{allowed && <button className="rounded bg-blue-700 px-4 py-2 text-white" onClick={() => setEditing(null)}>Novo paciente</button>}</div>
    {editing !== undefined && allowed && <PatientForm patient={editing ?? undefined} onComplete={() => setEditing(undefined)} onCancel={() => setEditing(undefined)} />}
    <div className="mt-6 overflow-x-auto rounded bg-white shadow"><table className="w-full text-left"><thead><tr className="border-b"><th className="p-3">Nome</th><th className="p-3">Telefone</th><th className="p-3">E-mail</th><th className="p-3">Consentimento</th>{allowed && <th className="p-3"><span className="sr-only">Ações</span></th>}</tr></thead><tbody>{patients.isLoading && <LoadingRow columns={allowed ? 5 : 4} />}{patients.data?.map(patient => <tr className="border-b" key={patient.id}><td className="p-3">{patient.name}</td><td className="p-3">{maskPhone(patient.phone)}</td><td className="p-3">{maskEmail(patient.email)}</td><td className="p-3">{patient.consentStatus}</td>{allowed && <td className="p-3"><button className="text-blue-700 underline" onClick={() => setEditing(patient)}>Editar</button></td>}</tr>)}{!patients.isLoading && patients.data?.length === 0 && <LoadingRow columns={allowed ? 5 : 4} />}</tbody></table><RequestFeedback error={patients.error} /></div>
  </ProtectedShell>;
}

function maskPhone(phone: string) { return phone.length < 5 ? "••••" : `${phone.slice(0, 3)}••••${phone.slice(-2)}`; }
function maskEmail(email?: string | null) { if (!email) return "—"; const [local, domain] = email.split("@"); return domain ? `${local.slice(0, 1)}•••@${domain}` : "••••"; }
