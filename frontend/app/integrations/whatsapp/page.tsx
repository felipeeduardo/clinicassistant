"use client";

import { useQuery } from "@tanstack/react-query";
import { RequestFeedback } from "@/components/request-feedback";
import { ProtectedShell } from "@/components/protected-shell";
import { whatsAppApi } from "@/lib/api/whatsapp";
import { useApi } from "@/providers/providers";

export default function WhatsAppIntegrationPage() {
  const api = useApi(); const integration = useQuery({ queryKey: ["integration-status"], queryFn: () => whatsAppApi.status(api) });
  return <ProtectedShell><h1 className="text-2xl font-bold">Integração WhatsApp</h1><p className="mt-1 text-slate-600">Status operacional da integração autorizada para o tenant atual.</p>{integration.isLoading && <p className="mt-6">Carregando status...</p>}{integration.data && <section className="mt-6 grid gap-4 md:grid-cols-2"><Card label="Provider" value={integration.data.provider} /><Card label="Status" value={integration.data.status} /><Card label="Número" value={integration.data.displayPhoneNumber || "Não informado"} /><Card label="Último webhook" value={formatDate(integration.data.lastWebhookAt)} /><Card label="Último envio bem-sucedido" value={formatDate(integration.data.lastSuccessfulSendAt)} /><Card label="Última falha" value={formatDate(integration.data.lastFailureAt)} /></section>}{integration.data?.failureReason && <section className="mt-4 rounded border border-amber-200 bg-amber-50 p-4 text-amber-900"><h2 className="font-semibold">Diagnóstico operacional</h2><p className="mt-1 text-sm">{integration.data.failureReason}</p></section>}<section className="mt-8 rounded bg-white p-5 shadow"><h2 className="font-semibold">Operações indisponíveis</h2><p className="mt-2 text-slate-600">A API ainda não disponibiliza alteração de provider, credenciais, templates ou sincronização. Nenhum segredo é exibido nesta tela.</p></section><RequestFeedback error={integration.error} /></ProtectedShell>;
}
function Card({ label, value }: { label: string; value: string }) { return <article className="rounded bg-white p-5 shadow"><p className="text-sm text-slate-600">{label}</p><p className="mt-2 font-semibold">{value}</p></article>; }
function formatDate(value?: string) { return value ? new Date(value).toLocaleString("pt-BR") : "—"; }
