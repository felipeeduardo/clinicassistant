"use client";

import { useQuery } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { ErrorState, Skeleton } from "@/components/ui/states";
import { can } from "@/lib/auth/permissions";
import { whatsAppApi } from "@/lib/api/whatsapp";
import { useApi, useAuth } from "@/providers/providers";

export default function TwilioSettingsPage() {
  const api = useApi(); const { user } = useAuth(); const allowed = can(user, "manageCatalog");
  const configuration = useQuery({ enabled: allowed, queryKey: ["twilio-configuration"], queryFn: () => whatsAppApi.twilioConfiguration(api) });
  if (!allowed) return <ProtectedShell><PageContainer><PageHeader pathname="/settings/integrations/twilio" title="Acesso restrito" /><ErrorState description="Somente administradores da clínica podem consultar a configuração da integração." title="Permissão insuficiente" /></PageContainer></ProtectedShell>;
  const data = configuration.data;
  const checks = data ? [["Account SID configurado", data.accountSidMasked !== "Não configurado"], ["Auth Token configurado", data.authTokenConfigured], ["Sender configurado", data.whatsAppFromMasked !== "Não informado"], ["Webhook HTTPS", isHttps(data.incomingWebhookBaseUrl)], ["Status callback HTTPS", isHttps(data.statusCallbackBaseUrl)], ["Validação de assinatura", data.signatureValidationEnabled], ["Integração habilitada", data.enabled]] : [];
  return <ProtectedShell><PageContainer><PageHeader description="Visão administrativa sanitizada. Credenciais são configuradas somente pelo ambiente seguro da plataforma." pathname="/settings/integrations/twilio" title="Configuração segura do Twilio" />
    {configuration.isError ? <div className="mt-6"><ErrorState onRetry={() => void configuration.refetch()} title="Não foi possível consultar a configuração" /></div> : configuration.isLoading ? <section className="mt-6 grid gap-4 md:grid-cols-2"><Skeleton className="h-72" /><Skeleton className="h-72" /></section> : data && <section className="mt-6 grid gap-5 lg:grid-cols-2"><Card><h2 className="font-semibold text-slate-950">Configuração ativa</h2><dl className="mt-5 grid gap-4 text-sm">{[["Provider", data.provider], ["Account SID", data.accountSidMasked], ["Sender WhatsApp", data.whatsAppFromMasked], ["Ambiente", data.environment], ["Última validação", data.lastValidatedAt ? new Date(data.lastValidatedAt).toLocaleString("pt-BR") : "Nunca"]].map(([label, value], index) => <div className="flex items-start justify-between gap-4" key={index}><dt className="text-slate-500">{label}</dt><dd className="text-right font-medium text-slate-900">{value}</dd></div>)}</dl></Card><Card><h2 className="font-semibold text-slate-950">Verificações seguras</h2><ul className="mt-5 grid gap-3">{checks.map(([label, passed], index) => <li className="flex items-center justify-between gap-3 rounded-control bg-slate-50 px-3 py-2 text-sm" key={index}><span>{label}</span><StatusBadge tone={passed ? "success" : "warning"}>{passed ? "Configurado" : "Pendente"}</StatusBadge></li>)}</ul></Card><Card className="lg:col-span-2"><h2 className="font-semibold text-slate-950">URLs públicas</h2><p className="mt-1 text-sm text-slate-600">Estas URLs não contêm segredo e devem usar HTTPS em produção.</p><dl className="mt-4 grid gap-4 text-sm md:grid-cols-2"><div><dt className="text-slate-500">Webhook de entrada</dt><dd className="mt-1 break-all font-medium">{data.incomingWebhookBaseUrl || "Não configurado"}</dd></div><div><dt className="text-slate-500">Status callback</dt><dd className="mt-1 break-all font-medium">{data.statusCallbackBaseUrl || "Não configurado"}</dd></div></dl></Card><Card className="border-amber-200 bg-amber-50 lg:col-span-2"><h2 className="font-semibold text-amber-950">Gestão de credenciais</h2><p className="mt-1 text-sm text-amber-900">Por decisão arquitetural, Account SID e Auth Token são mantidos somente em variáveis de ambiente ou secret manager da plataforma. O token nunca é retornado, exibido, persistido pelo navegador ou enviado por esta tela. Para rotacionar credenciais, atualize o secret do ambiente e revalide a integração.</p></Card></section>}
  </PageContainer></ProtectedShell>;
}

function isHttps(value?: string | null) { try { return Boolean(value && new URL(value).protocol === "https:"); } catch { return false; } }
