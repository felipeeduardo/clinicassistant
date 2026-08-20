"use client";

import { useMutation, useQueries, useQueryClient } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { RequestFeedback } from "@/components/request-feedback";
import { Button } from "@/components/ui/button";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { platformApi } from "@/lib/api/platform";
import type { PlatformClinic, PlatformTenant, PlatformUser } from "@/lib/api/types";
import { useApi, useAuth } from "@/providers/providers";

const roleLabels: Record<string, string> = {
  PlatformAdmin: "Administrador da plataforma",
  ClinicAdmin: "Administrador da clínica",
  Receptionist: "Recepção",
  Professional: "Profissional",
  Viewer: "Visualização",
};

export default function PlatformPage() {
  const api = useApi();
  const { user } = useAuth();
  const client = useQueryClient();
  const [tenants, users, clinics] = useQueries({ queries: [
    { queryKey: ["platform", "tenants"], queryFn: () => platformApi.tenants(api), enabled: user?.role === "PlatformAdmin" },
    { queryKey: ["platform", "users"], queryFn: () => platformApi.users(api), enabled: user?.role === "PlatformAdmin" },
    { queryKey: ["platform", "clinics"], queryFn: () => platformApi.clinics(api), enabled: user?.role === "PlatformAdmin" },
  ] });
  const status = useMutation({
    mutationFn: ({ id, action }: { id: string; action: "activate" | "suspend" | "disable" }) => platformApi.status(api, id, action),
    onSuccess: () => client.invalidateQueries({ queryKey: ["platform", "tenants"] }),
  });

  if (user?.role !== "PlatformAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/platform" title="Acesso negado" description="Esta área é exclusiva da administração da plataforma." /></PageContainer></ProtectedShell>;

  const hasClinics = (clinics.data?.length ?? 0) > 0;
  const loading = tenants.isLoading || users.isLoading || clinics.isLoading;
  return <ProtectedShell><PageContainer>
    <PageHeader actions={<div className="flex flex-wrap gap-2"><Button aria-label="Abrir leads comerciais" title="Abrir leads comerciais" variant="outline" onClick={() => { window.location.href = "/platform/leads" }}><Icon name="messages" />Leads comerciais</Button><Button aria-label={hasClinics ? "Criar nova clínica" : "Provisionar primeira clínica"} onClick={() => { window.location.href = "/platform/onboarding" }}><Icon name="building" />{hasClinics ? "Nova clínica" : "Provisionar primeira clínica"}</Button></div>} description="Provisione clínicas, acompanhe o ciclo de vida e visualize o estado global da plataforma." pathname="/platform" title="Administração da plataforma" />
    <Card className="mt-6"><div className="flex flex-wrap items-start justify-between gap-3"><div><h2 className="font-semibold text-slate-950">Como interpretar os status</h2><p className="mt-1 text-sm text-slate-500">O status indica em que momento do provisionamento e da operação a clínica está.</p></div><Icon className="text-slate-400" name="history" /></div><div className="mt-5 grid gap-4 sm:grid-cols-2 xl:grid-cols-4"><StatusExplanation status="Provisioning" title="Em configuração" description="Clínica criada, mas ainda revisando os dados e módulos iniciais." /><StatusExplanation status="Active" title="Ativo" description="Operação liberada para uso pelos usuários da clínica." /><StatusExplanation status="Suspended" title="Suspenso" description="Acesso temporariamente interrompido; os dados permanecem preservados." /><StatusExplanation status="Disabled" title="Desativado" description="Clínica encerrada no ciclo de vida e sem operação disponível." /></div></Card>
    {!loading && !hasClinics && <Card className="mt-6 border-brand-200 bg-brand-50"><p className="text-xs font-semibold uppercase tracking-[0.16em] text-brand-primary">Primeiro passo</p><h2 className="mt-1 text-xl font-semibold text-slate-950">Configure sua primeira clínica</h2><p className="mt-2 max-w-xl text-sm text-slate-600">Cadastre os dados essenciais para começar a operar a IA Recepção. O processo é guiado e pode ser concluído por etapas.</p><Button className="mt-5" onClick={() => { window.location.href = "/platform/onboarding" }}><Icon name="arrowRight" />Começar configuração</Button></Card>}
    <Card className="mt-6 overflow-hidden p-0"><div className="flex flex-wrap items-center justify-between gap-3 border-b border-border-system px-5 py-4"><div><h2 className="font-semibold text-slate-950">Clínicas e tenants</h2><p className="mt-1 text-sm text-slate-500">Gerencie o ciclo de vida e acompanhe o contexto de cada operação.</p></div><span className="text-sm text-slate-500">{tenants.data?.length ?? 0} registros</span></div><div className="divide-y divide-border-system">{tenants.data?.map(item => <TenantRow actionError={status.error instanceof Error ? status.error.message : undefined} item={item} key={item.id} loading={status.isPending} onAction={action => status.mutate({ id: item.id, action })} />)}{!loading && !tenants.data?.length && <p className="px-5 py-8 text-sm text-slate-500">Nenhum tenant encontrado.</p>}{loading && <p className="px-5 py-8 text-sm text-slate-500">Carregando dados da plataforma...</p>}</div></Card>
    <section className="mt-6 grid gap-5 md:grid-cols-2"><SummaryCard icon="userCheck" title="Usuários globais" count={users.data?.length ?? 0} description="Identidade, papel e tenant associado.">{users.data?.map(item => <li className="flex items-start gap-3 border-b border-border-system pb-3 last:border-0 last:pb-0" key={item.id}><Icon className="mt-0.5 text-slate-400" name={item.role === "PlatformAdmin" ? "shield" : "userCheck"} /><span className="min-w-0 flex-1"><strong className="block truncate font-medium text-slate-950">{item.name}</strong><span className="block truncate text-xs text-slate-500">{item.email}</span><span className="block truncate text-xs text-slate-500">{roleLabels[item.role] ?? item.role} · tenant {shortId(item.tenantId)}</span></span><StatusBadge tone={item.status === "Active" ? "success" : "neutral"}>{statusLabel(item.status)}</StatusBadge></li>)}</SummaryCard><SummaryCard icon="building" title="Clínicas globais" count={clinics.data?.length ?? 0} description="Razão social, nome de operação e tenant.">{clinics.data?.map(item => <li className="flex items-start gap-3 border-b border-border-system pb-3 last:border-0 last:pb-0" key={item.id}><Icon className="mt-0.5 text-slate-400" name="clinic" /><span className="min-w-0 flex-1"><strong className="block truncate font-medium text-slate-950">{item.tradeName}</strong><span className="block truncate text-xs text-slate-500">{item.legalName}</span><span className="block truncate text-xs text-slate-500">tenant {shortId(item.tenantId)}</span></span><StatusBadge tone={item.status === "Active" ? "success" : "neutral"}>{statusLabel(item.status)}</StatusBadge></li>)}</SummaryCard></section>
    <RequestFeedback error={tenants.error || users.error || clinics.error || status.error} />
  </PageContainer></ProtectedShell>;
}

function TenantRow({ item, loading, onAction }: { item: PlatformTenant; loading: boolean; actionError?: string; onAction: (action: "activate" | "suspend" | "disable") => void }) {
  return <div className="grid gap-4 px-5 py-4 lg:grid-cols-[minmax(0,1fr)_auto] lg:items-center"><div className="min-w-0"><div className="flex flex-wrap items-center gap-2"><p className="font-medium text-slate-950">{item.name}</p><StatusBadge tone={statusTone(item.status)}>{statusLabel(item.status)}</StatusBadge></div><p className="mt-1 text-xs text-slate-500">{item.slug} · {item.initialUnitName ?? "Sem unidade inicial"} · {item.userCount} {item.userCount === 1 ? "usuário" : "usuários"}</p><p className="mt-1 text-xs text-slate-500">ClinicAdmin: {item.clinicAdminName ?? "Não associado"}{item.clinicAdminEmail ? ` · ${item.clinicAdminEmail}` : ""} · Criada em {new Date(item.createdAt).toLocaleDateString("pt-BR")}</p></div><div className="flex flex-wrap items-center justify-end gap-2"><Button aria-label={`Visualizar ${item.name}`} onClick={() => { window.location.href = `/platform/clinics/${item.id}`; }} size="icon" title="Visualizar detalhes" variant="outline"><Icon name="eye" /></Button><Button aria-label={`Ativar ${item.name}`} disabled={item.status === "Active"} loading={loading && item.status !== "Active"} onClick={() => onAction("activate")} size="icon" title="Ativar tenant" variant="outline"><Icon name="arrowRight" /></Button><Button aria-label={`Suspender ${item.name}`} disabled={item.status === "Suspended"} loading={loading && item.status !== "Suspended"} onClick={() => onAction("suspend")} size="icon" title="Suspender tenant" variant="outline"><Icon name="history" /></Button><Button aria-label={`Desativar ${item.name}`} loading={loading} onClick={() => onAction("disable")} size="icon" title="Desativar tenant" variant="danger"><Icon name="close" /></Button></div></div>;
}

function SummaryCard({ title, count, description, icon, children }: { title: string; count: number; description: string; icon: "building" | "userCheck"; children?: React.ReactNode }) { return <Card><div className="flex items-start justify-between gap-3"><div className="flex items-start gap-3"><span className="grid size-9 place-items-center rounded-control bg-brand-50 text-brand-primary"><Icon name={icon} /></span><div><h2 className="font-semibold text-slate-950">{title}</h2><p className="mt-1 text-xs text-slate-500">{description}</p></div></div><span className="grid size-8 place-items-center rounded-full bg-brand-50 text-sm font-semibold text-brand-primary">{count}</span></div>{children ? <ul className="mt-5 space-y-3 text-sm">{children}</ul> : <p className="mt-4 text-sm text-slate-500">Sem registros.</p>}</Card>; }
function StatusExplanation({ status, title, description }: { status: string; title: string; description: string }) { return <div className="rounded-control border border-border-system bg-slate-50/70 p-3"><StatusBadge tone={statusTone(status)}>{title}</StatusBadge><p className="mt-2 text-xs leading-5 text-slate-600">{description}</p></div>; }
function shortId(value: string) { return value.slice(0, 8); }
function statusTone(status: string) { return status === "Active" ? "success" : status === "Suspended" ? "warning" : status === "Provisioning" ? "info" : "danger" as const; }
function statusLabel(status: string) { return ({ Provisioning: "Em configuração", Active: "Ativo", Suspended: "Suspenso", Inactive: "Inativo", Disabled: "Desativado" } as Record<string, string>)[status] ?? status; }
