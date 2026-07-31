"use client";

import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { RequestFeedback } from "@/components/request-feedback";
import { ProtectedShell } from "@/components/protected-shell";
import { ClinicForm } from "@/features/catalog/catalog-forms";
import { catalogApi } from "@/lib/api/catalog";
import { can } from "@/lib/auth/permissions";
import { useApi, useAuth } from "@/providers/providers";

export default function ClinicsPage() {
  const api = useApi(); const { user } = useAuth(); const clinic = useQuery({ queryKey: ["clinic"], queryFn: () => catalogApi.getClinic(api) }); const [editing, setEditing] = useState(false); const allowed = can(user, "manageCatalog");
  return <ProtectedShell><h1 className="text-2xl font-bold">Clínica</h1><p className="mt-1 text-slate-600">A API atual expõe somente a clínica associada à sua sessão.</p>{clinic.isLoading && <p className="mt-6">Carregando...</p>}{clinic.data && <section className="mt-6 rounded bg-white p-5 shadow"><dl className="grid gap-3 md:grid-cols-2"><Item label="Nome fantasia" value={clinic.data.tradeName} /><Item label="Razão social" value={clinic.data.legalName} /><Item label="E-mail" value={clinic.data.email} /><Item label="Fuso horário" value={clinic.data.timeZone} /></dl>{allowed && <button className="mt-5 rounded bg-blue-700 px-4 py-2 text-white" onClick={() => setEditing(value => !value)}>{editing ? "Fechar edição" : "Editar clínica"}</button>}{editing && allowed && <ClinicForm clinic={clinic.data} onComplete={() => setEditing(false)} />}</section>}<RequestFeedback error={clinic.error} /></ProtectedShell>;
}
function Item({ label, value }: { label: string; value: string }) { return <div><dt className="text-sm text-slate-600">{label}</dt><dd>{value}</dd></div>; }
