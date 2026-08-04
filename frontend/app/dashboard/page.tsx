"use client";

import { useQuery } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { ErrorState, Skeleton } from "@/components/ui/states";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatCard, StatusBadge } from "@/components/ui/surfaces";
import { useApi } from "@/providers/providers";
import type { DashboardSummary, IntegrationStatus } from "@/lib/api/types";

export default function DashboardPage() {
  const api = useApi();
  const integration = useQuery({ queryKey: ["integration-status"], queryFn: () => api.request<IntegrationStatus>("/api/whatsapp/integration/status") });
  const dashboard = useQuery({ queryKey: ["dashboard"], queryFn: () => api.request<DashboardSummary>("/api/dashboard") });
  const summary = dashboard.data;
  const hasError = integration.isError || dashboard.isError;

  return <ProtectedShell><PageContainer><PageHeader description="Acompanhe os principais indicadores do tenant atual." pathname="/dashboard" title="Dashboard operacional" />
    {hasError && <div className="mt-6"><ErrorState onRetry={() => { void integration.refetch(); void dashboard.refetch(); }} /></div>}
    <section className="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-3" aria-label="Indicadores operacionais">
      <StatCard label="Integração WhatsApp" value={integration.isLoading ? "…" : integration.data?.status ?? "Não configurada"}><StatusBadge tone={integration.data?.status === "Connected" ? "success" : "warning"}>{integration.data?.provider ?? "WhatsApp"}</StatusBadge></StatCard>
      <StatCard label="Consultas hoje" value={summary ? String(summary.appointmentsToday) : "—"}><Icon className="text-brand-600" name="calendar" /></StatCard>
      <StatCard label="Pendentes hoje" value={summary ? String(summary.pendingAppointmentsToday) : "—"}><Icon className="text-amber-600" name="calendar" /></StatCard>
      <StatCard label="Fila humana" value={summary ? String(summary.waitingHumanConversations) : "—"}><Icon className="text-violet-600" name="messages" /></StatCard>
      <StatCard label="Conversas ativas" value={summary ? String(summary.activeConversations) : "—"}><Icon className="text-sky-600" name="messages" /></StatCard>
      <StatCard label="Falhas na Outbox" value={summary ? String(summary.failedOutboxMessages) : "—"}><Icon className="text-red-600" name="audit" /></StatCard>
    </section>
    {dashboard.isLoading && <section className="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-3" aria-label="Carregando indicadores">{Array.from({ length: 3 }, (_, index) => <Skeleton className="h-32" key={index} />)}</section>}
    <Card className="mt-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between"><div><h2 className="font-semibold text-slate-950">Atualização em tempo real</h2><p className="mt-1 text-sm text-slate-600">Consultas, conversas e integrações atualizam os indicadores quando eventos operacionais chegam.</p></div><StatusBadge tone="info">SignalR ativo</StatusBadge></Card>
  </PageContainer></ProtectedShell>;
}
