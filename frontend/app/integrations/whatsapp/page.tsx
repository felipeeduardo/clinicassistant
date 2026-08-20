"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { whatsAppApi } from "@/lib/api/whatsapp";
import { can } from "@/lib/auth/permissions";
import { useApi, useAuth } from "@/providers/providers";
import { Button } from "@/components/ui/button";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatCard, StatusBadge } from "@/components/ui/surfaces";
import { ErrorState, Skeleton } from "@/components/ui/states";
import { ApiError } from "@/lib/api/client";

export default function WhatsAppIntegrationPage() {
  const api = useApi(); const { user } = useAuth(); const client = useQueryClient();
  const integration = useQuery({ queryKey: ["integration-status"], queryFn: () => whatsAppApi.status(api), refetchInterval: 15_000 }); const allowed = can(user, "manageCatalog");
  const operation = useMutation({ mutationFn: (action: "validate" | "enable" | "disable" | "testMessage") => whatsAppApi[action](api), onSuccess: () => client.invalidateQueries({ queryKey: ["integration-status"] }) });
  const data = integration.data; const connected = data?.status === "Conectado";

  return <ProtectedShell><PageContainer><PageHeader description="Acompanhe o estado operacional da integração autorizada para o tenant atual." pathname="/integrations/whatsapp" title="Integração WhatsApp" />
    {integration.data && <Card className="mt-6"><div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between"><div><h2 className="font-semibold text-slate-950">Configuração operacional</h2><p className="mt-1 text-sm text-slate-600">A plataforma mantém as credenciais protegidas. Esta visão mostra apenas a disponibilidade dos canais.</p></div><StatusBadge tone={connected ? "success" : "warning"}>{connected ? "Operacional" : "Requer verificação"}</StatusBadge></div><div className="mt-5 grid gap-3 sm:grid-cols-2"><OperationalItem label="Webhook de entrada" configured={integration.data.inboundWebhookConfigured} /><OperationalItem label="Callback de status" configured={integration.data.statusCallbackConfigured} /><OperationalItem label="Processamento de mensagens" configured={connected} /><OperationalItem label="Templates" configured={connected} /></div></Card>}
    {allowed && <Card className="mt-8"><div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between"><div><h2 className="font-semibold text-slate-950">Operações do WhatsApp</h2><p className="mt-1 max-w-2xl text-sm text-slate-600">Verifique a conexão, habilite ou desabilite o canal para esta clínica.</p><p className="mt-2 text-xs text-slate-500">O canal é gerenciado pela plataforma e processado pela Outbox.</p></div><StatusBadge tone={connected ? "success" : "warning"}>{connected ? "Pronta para uso" : "Verificação necessária"}</StatusBadge></div><div className="mt-5 flex flex-wrap gap-2"><Button loading={operation.isPending} onClick={() => operation.mutate("validate")} variant="outline">Verificar conexão</Button><Button loading={operation.isPending} onClick={() => operation.mutate("enable")} variant="success">Habilitar</Button><Button loading={operation.isPending} onClick={() => operation.mutate("disable")} variant="outline">Desativar</Button>{data?.canSendTest && <Button disabled={!connected} loading={operation.isPending} onClick={() => operation.mutate("testMessage")}><Icon name="messages" />Enviar teste</Button>}</div>{operation.isError && <div className="mt-4 rounded-control border border-red-200 bg-red-50 p-3 text-sm text-red-800" role="alert"><p>{friendlyOperationError(operation.error)}</p>{operation.error instanceof ApiError && operation.error.traceId && <p className="mt-1 text-xs text-red-700">Código de atendimento: <code>{operation.error.traceId}</code></p>}</div>}{operation.isSuccess && <><p className="mt-4 text-sm text-emerald-700" role="status">Operação solicitada.</p><p className="mt-1 text-xs text-slate-500">A mensagem será processada com segurança.</p></>}</Card>}
  </PageContainer></ProtectedShell>;
}
function formatDate(value?: string | null) { return value ? new Date(value).toLocaleString("pt-BR") : "—"; }
function OperationalItem({ label, configured }: { label: string; configured: boolean }) { return <div className="flex items-center justify-between rounded-control border border-slate-200 px-3 py-2 text-sm"><span className="text-slate-700">{label}</span><StatusBadge tone={configured ? "success" : "warning"}>{configured ? "Configurado" : "Pendente"}</StatusBadge></div>; }
function friendlyOperationError(error: unknown) {
  if (!(error instanceof ApiError)) return "Não foi possível concluir a operação. Tente novamente.";
  const messages: Record<string, string> = { invalid_operation: "O WhatsApp está temporariamente indisponível. Nossa equipe técnica foi informada.", resource_not_found: "Nenhum canal WhatsApp foi encontrado para esta clínica.", unauthorized: "Seu usuário não tem permissão para operar o WhatsApp." };
  return messages[error.code ?? ""] ?? error.message;
}
