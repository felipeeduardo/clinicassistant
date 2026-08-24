"use client";

import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Card } from "@/components/ui/surfaces";
import { ErrorState, Skeleton } from "@/components/ui/states";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { ProtectedShell } from "@/components/protected-shell";
import { Button } from "@/components/ui/button";
import { ClinicForm } from "@/features/catalog/catalog-forms";
import { catalogApi } from "@/lib/api/catalog";
import { can } from "@/lib/auth/permissions";
import { useApi, useAuth } from "@/providers/providers";

export default function ClinicsPage() {
  const api = useApi(); const { user } = useAuth(); const allowed = can(user, "manageCatalog");
  const clinic = useQuery({ queryKey: ["clinic"], queryFn: () => catalogApi.getClinic(api) });
  const [editing, setEditing] = useState(false);
  return <ProtectedShell><PageContainer><PageHeader description="Dados institucionais, contato e fuso horário usados pela operação." pathname="/clinics" title="Clínica" actions={allowed ? <Button onClick={() => setEditing(value => !value)}>{editing ? "Fechar edição" : "Editar clínica"}</Button> : undefined} />
    {clinic.isLoading && <Card className="mt-6"><Skeleton className="h-32" /></Card>}
    {clinic.isError && <div className="mt-6"><ErrorState onRetry={() => void clinic.refetch()} title="Não foi possível carregar a clínica" /></div>}
    {clinic.data && !editing && <Card className="mt-6"><dl className="grid gap-5 sm:grid-cols-2">{[["Nome fantasia", clinic.data.tradeName], ["Razão social", clinic.data.legalName], ["E-mail", clinic.data.email], ["Telefone", clinic.data.phone], ["Documento", clinic.data.document], ["Fuso horário", clinic.data.timeZone], ["Assistente virtual", clinic.data.assistantDisplayName || "IA Recepção"]].map(([label, value]) => <div className="rounded-control bg-slate-50 p-4" key={label}><dt className="text-sm text-slate-500">{label}</dt><dd className="mt-1 break-words font-medium text-slate-950">{value}</dd></div>)}</dl></Card>}
    {clinic.data && editing && allowed && <ClinicForm clinic={clinic.data} onComplete={() => setEditing(false)} />}
  </PageContainer></ProtectedShell>;
}
