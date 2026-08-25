"use client";

import { useQueries, useQuery } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { Icon } from "@/components/ui/icon";
import { useApi, useAuth } from "@/providers/providers";
import { platformApi } from "@/lib/api/platform";
import type { PlatformOnboardingStatus } from "@/lib/api/types";

const checklist: ReadonlyArray<[keyof PlatformOnboardingStatus, string]> = [["clinicConfigured", "Clínica"], ["unitConfigured", "Unidade"], ["clinicAdminConfigured", "ClinicAdmin"], ["specialtiesConfigured", "Especialidades"], ["professionalsConfigured", "Profissionais"], ["availabilityConfigured", "Horários"], ["whatsAppConfigured", "WhatsApp"]];

export default function PlatformImplementationPage() {
  const api = useApi();
  const { user } = useAuth();
  const tenants = useQuery({ queryKey: ["platform", "tenants"], queryFn: () => platformApi.tenants(api), enabled: user?.role === "PlatformAdmin" });
  const statuses = useQueries({ queries: (tenants.data ?? []).map(tenant => ({ queryKey: ["platform", "onboarding", tenant.id], queryFn: () => platformApi.onboardingStatus(api, tenant.id) })) });
  if (user?.role !== "PlatformAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/platform/implementation" title="Acesso negado" description="Implantação é exclusiva da administração da plataforma." /></PageContainer></ProtectedShell>;
  return <ProtectedShell><PageContainer><PageHeader pathname="/platform/implementation" title="Implantação" description="Acompanhe o checklist de cada clínica até a validação operacional e o go-live." /><Card className="mt-6 border-brand-200 bg-brand-50"><div className="flex gap-3"><Icon className="mt-0.5 text-brand-700" name="history" /><div><h2 className="font-semibold text-brand-950">Roteiro recomendado</h2><p className="mt-1 text-sm leading-6 text-brand-900">Provisione a clínica, confirme dados e usuários, configure o catálogo e a agenda, valide o WhatsApp e só então ative a operação. O checklist abaixo usa o status real do backend.</p></div></div></Card><section className="mt-6 grid gap-5 lg:grid-cols-2">{tenants.data?.map((tenant, index) => { const status = statuses[index]?.data; const completed = status ? checklist.filter(([key]) => Boolean(status[key])).length : 0; return <Card key={tenant.id}><div className="flex flex-wrap items-start justify-between gap-3"><div><h2 className="font-semibold text-slate-950">{tenant.name}</h2><p className="mt-1 text-xs text-slate-500">{tenant.slug} · {tenant.clinicAdminName ?? "ClinicAdmin pendente"}</p></div><StatusBadge tone={tenant.status === "Active" ? "success" : "info"}>{tenant.status === "Active" ? "Ativa" : "Em configuração"}</StatusBadge></div><div className="mt-5 grid gap-2">{checklist.map(([key, label]) => { const done = Boolean(status?.[key]); return <div className="flex items-center justify-between rounded-control border border-border-system px-3 py-2 text-sm" key={key}><span>{label}</span><StatusBadge tone={done ? "success" : "warning"}>{done ? "Concluído" : "Pendente"}</StatusBadge></div>; })}</div><div className="mt-4 flex items-center justify-between text-xs text-slate-500"><span>{completed}/{checklist.length} itens concluídos</span><a className="font-medium text-brand-700 hover:underline" href={`/platform/clinics/${tenant.id}`}>Abrir clínica</a></div></Card>; })}{tenants.isLoading && <Card><p className="text-sm text-slate-500">Carregando clínicas...</p></Card>}{!tenants.isLoading && !tenants.data?.length && <Card><p className="text-sm text-slate-500">Nenhuma clínica encontrada.</p></Card>}</section></PageContainer></ProtectedShell>;
}
