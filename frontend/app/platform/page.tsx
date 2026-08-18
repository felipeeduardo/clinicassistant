"use client";

import { useMutation, useQueries, useQueryClient } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { RequestFeedback } from "@/components/request-feedback";
import { Button } from "@/components/ui/button";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { platformApi } from "@/lib/api/platform";
import { useApi, useAuth } from "@/providers/providers";

const roleLabels: Record<string, string> = { PlatformAdmin: "Administrador da plataforma", ClinicAdmin: "Administrador da clínica", Receptionist: "Recepção", Professional: "Profissional" };

export default function PlatformPage() {
  const api = useApi();
  const { user } = useAuth();
  const client = useQueryClient();
  const [tenants, users, clinics] = useQueries({ queries: [
    { queryKey: ["platform", "tenants"], queryFn: () => platformApi.tenants(api), enabled: user?.role === "PlatformAdmin" },
    { queryKey: ["platform", "users"], queryFn: () => platformApi.users(api), enabled: user?.role === "PlatformAdmin" },
    { queryKey: ["platform", "clinics"], queryFn: () => platformApi.clinics(api), enabled: user?.role === "PlatformAdmin" },
  ] });
  const status = useMutation({ mutationFn: ({ id, action }: { id: string; action: "activate" | "suspend" | "disable" }) => platformApi.status(api, id, action), onSuccess: () => client.invalidateQueries({ queryKey: ["platform", "tenants"] }) });
  if (user?.role !== "PlatformAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/platform" title="Acesso negado" description="Esta área é exclusiva da administração da plataforma." /></PageContainer></ProtectedShell>;

  const hasClinics = (clinics.data?.length ?? 0) > 0;
  const loading = tenants.isLoading || users.isLoading || clinics.isLoading;
  return <ProtectedShell><PageContainer>
    <PageHeader actions={<Button onClick={() => { window.location.href = "/platform/onboarding" }}>{hasClinics ? "Nova clínica" : "Configurar primeira clínica"}</Button>} description="Acompanhe os tenants e configure clínicas com um fluxo guiado." pathname="/platform" title="Administração da plataforma" />
    {!loading && !hasClinics && <Card className="mt-6 border-brand-200 bg-brand-50"><p className="text-xs font-semibold uppercase tracking-[0.16em] text-brand-primary">Primeiro passo</p><h2 className="mt-1 text-xl font-semibold text-slate-950">Configure sua primeira clínica</h2><p className="mt-2 max-w-xl text-sm text-slate-600">Cadastre os dados essenciais para começar a operar a IA Recepção. O processo é guiado e pode ser concluído por etapas.</p><Button className="mt-5" onClick={() => { window.location.href = "/platform/onboarding" }}>Começar configuração</Button></Card>}
    <Card className="mt-6 overflow-hidden p-0"><div className="flex flex-wrap items-center justify-between gap-3 border-b border-border-system px-5 py-4"><div><h2 className="font-semibold text-slate-950">Clínicas e tenants</h2><p className="mt-1 text-sm text-slate-500">Gerencie status e acompanhe a quantidade de usuários.</p></div><span className="text-sm text-slate-500">{tenants.data?.length ?? 0} registros</span></div><div className="divide-y divide-border-system">{tenants.data?.map(item => <TenantRow actionError={status.error instanceof Error ? status.error.message : undefined} item={item} key={item.id} loading={status.isPending} onAction={action => status.mutate({ id: item.id, action })} />)}{!loading && !tenants.data?.length && <p className="px-5 py-8 text-sm text-slate-500">Nenhum tenant encontrado.</p>}{loading && <p className="px-5 py-8 text-sm text-slate-500">Carregando dados da plataforma...</p>}</div></Card>
    <section className="mt-6 grid gap-5 md:grid-cols-2"><SummaryCard title="Usuários globais" count={users.data?.length ?? 0}>{users.data?.map(item => <li className="flex items-center justify-between gap-3" key={item.id}><span className="truncate">{item.name}<span className="ml-2 text-xs text-slate-500">{roleLabels[item.role] ?? item.role}</span></span><StatusBadge tone={item.status === "Active" ? "success" : "neutral"}>{statusLabel(item.status)}</StatusBadge></li>)}</SummaryCard><SummaryCard title="Clínicas globais" count={clinics.data?.length ?? 0}>{clinics.data?.map(item => <li className="flex items-center justify-between gap-3" key={item.id}><span className="truncate">{item.tradeName}</span><StatusBadge tone={item.status === "Active" ? "success" : "neutral"}>{statusLabel(item.status)}</StatusBadge></li>)}</SummaryCard></section>
    <RequestFeedback error={tenants.error || users.error || clinics.error || status.error} />
  </PageContainer></ProtectedShell>;
}

function TenantRow({ item, loading, onAction }: { item: { id: string; name: string; slug: string; status: string; userCount: number }; loading: boolean; actionError?: string; onAction: (action: "activate" | "suspend" | "disable") => void }) { return <div className="grid gap-4 px-5 py-4 lg:grid-cols-[minmax(0,1fr)_auto] lg:items-center"><div className="min-w-0"><div className="flex flex-wrap items-center gap-2"><p className="font-medium text-slate-950">{item.name}</p><StatusBadge tone={item.status === "Active" ? "success" : item.status === "Suspended" ? "warning" : "danger"}>{statusLabel(item.status)}</StatusBadge></div><p className="mt-1 text-xs text-slate-500">{item.slug} · {item.userCount} {item.userCount === 1 ? "usuário" : "usuários"}</p></div><div className="flex flex-wrap gap-2"><Button disabled={item.status === "Active"} loading={loading && item.status !== "Active"} onClick={() => onAction("activate")} size="sm" variant="outline">Ativar</Button><Button disabled={item.status === "Suspended"} loading={loading && item.status !== "Suspended"} onClick={() => onAction("suspend")} size="sm" variant="outline">Suspender</Button><Button onClick={() => onAction("disable")} size="sm" variant="danger">Desativar</Button></div></div>; }

function SummaryCard({ title, count, children }: { title: string; count: number; children?: React.ReactNode }) { return <Card><div className="flex items-center justify-between gap-3"><h2 className="font-semibold text-slate-950">{title}</h2><span className="grid size-8 place-items-center rounded-full bg-brand-50 text-sm font-semibold text-brand-primary">{count}</span></div>{children ? <ul className="mt-4 space-y-3 text-sm">{children}</ul> : <p className="mt-4 text-sm text-slate-500">Sem registros.</p>}</Card>; }
function statusLabel(status: string) { return ({ Active: "Ativo", Suspended: "Suspenso", Inactive: "Inativo", Disabled: "Desativado" } as Record<string, string>)[status] ?? status; }
