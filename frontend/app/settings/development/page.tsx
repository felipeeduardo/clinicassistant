"use client";

import { useQuery } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { ErrorState, Skeleton } from "@/components/ui/states";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { can } from "@/lib/auth/permissions";
import { whatsAppApi } from "@/lib/api/whatsapp";
import { useApi, useAuth } from "@/providers/providers";

export default function DevelopmentDiagnosticsPage() {
  const api = useApi();
  const { user } = useAuth();
  const allowed = can(user, "manageCatalog");
  const configuration = useQuery({ enabled: allowed, queryKey: ["development-diagnostics", "twilio"], queryFn: () => whatsAppApi.twilioConfiguration(api) });
  const health = useQuery({ enabled: allowed, queryKey: ["development-diagnostics", "health"], queryFn: async () => { const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080"}/health/ready`); return { ok: response.ok, status: response.status }; } });

  if (!allowed) return <ProtectedShell><PageContainer><PageHeader pathname="/settings/development" title="Acesso restrito" /><ErrorState description="Somente administradores da clínica podem consultar o diagnóstico local." title="Permissão insuficiente" /></PageContainer></ProtectedShell>;
  if (configuration.isLoading || health.isLoading) return <ProtectedShell><PageContainer><PageHeader pathname="/settings/development" title="Diagnóstico local" /><Skeleton className="mt-6 h-80" /></PageContainer></ProtectedShell>;
  if (configuration.isError || health.isError || !configuration.data) return <ProtectedShell><PageContainer><PageHeader pathname="/settings/development" title="Diagnóstico local" /><div className="mt-6"><ErrorState onRetry={() => { void configuration.refetch(); void health.refetch(); }} title="Não foi possível carregar o diagnóstico" /></div></PageContainer></ProtectedShell>;

  const data = configuration.data;
  const checks = [["API /health/ready", health.data?.ok ?? false], ["Provider efetivo", data.provider === "Fake" || data.provider === "Twilio"], ["Ambiente seguro", data.environment !== "Production"], ["Auth Token configurado", data.authTokenConfigured], ["Integração habilitada", data.enabled], ["Webhook configurado", Boolean(data.incomingWebhookBaseUrl)], ["Status callback configurado", Boolean(data.statusCallbackBaseUrl)]] as const;
  return <ProtectedShell><PageContainer><PageHeader description="Diagnóstico sanitizado para desenvolvimento e testes. Nenhum segredo é exibido." pathname="/settings/development" title="Diagnóstico local" /><section className="mt-6 grid gap-5 lg:grid-cols-2"><Card><h2 className="font-semibold text-slate-950">Ambiente</h2><dl className="mt-4 grid gap-3 text-sm">{[["Ambiente", data.environment], ["Provider", data.provider], ["Sender", data.whatsAppFromMasked], ["Última validação", data.lastValidatedAt ? new Date(data.lastValidatedAt).toLocaleString("pt-BR") : "Nunca"]].map(([label, value]) => <div className="flex justify-between gap-4" key={label}><dt className="text-slate-500">{label}</dt><dd className="font-medium text-slate-900">{value}</dd></div>)}</dl></Card><Card><h2 className="font-semibold text-slate-950">Health checks e integração</h2><ul className="mt-4 grid gap-2">{checks.map(([label, passed]) => <li className="flex items-center justify-between gap-3 rounded-control bg-slate-50 px-3 py-2 text-sm" key={label}><span>{label}</span><StatusBadge tone={passed ? "success" : "warning"}>{passed ? "OK" : "Pendente"}</StatusBadge></li>)}</ul></Card><Card className="lg:col-span-2"><h2 className="font-semibold text-slate-950">URLs operacionais</h2><dl className="mt-4 grid gap-4 text-sm md:grid-cols-2"><div><dt className="text-slate-500">Inbound webhook</dt><dd className="mt-1 break-all font-medium">{data.incomingWebhookBaseUrl || "Não configurado"}</dd></div><div><dt className="text-slate-500">Status callback</dt><dd className="mt-1 break-all font-medium">{data.statusCallbackBaseUrl || "Não configurado"}</dd></div></dl><p className="mt-4 text-xs text-slate-500">O Auth Token e demais credenciais permanecem somente no ambiente seguro.</p></Card></section></PageContainer></ProtectedShell>;
}
