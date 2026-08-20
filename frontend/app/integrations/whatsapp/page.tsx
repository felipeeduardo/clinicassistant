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
    {integration.isError ? <div className="mt-6"><ErrorState onRetry={() => void integration.refetch()} /></div> : integration.isLoading ? <section className="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">{Array.from({ length: 6 }, (_, index) => <Skeleton className="h-32" key={index} />)}</section> : data && <><section className="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-3"><StatCard label="Status do WhatsApp" value={data.status}><Icon className="text-brand-600" name="whatsapp" /></StatCard><StatCard label="Canal conectado" value={data.displayPhoneNumber || "Em configuração"} /><StatCard label="Última atividade" value={formatDate(data.lastWebhookAt || data.lastSuccessfulSendAt)} /><StatCard label="Último envio" value={formatDate(data.lastSuccessfulSendAt)} /><StatCard label="Última falha" value={formatDate(data.lastFailureAt)} /><StatCard label="Templates" value="Disponíveis" /></section>{data.failureReason && <Card className="mt-4 border-amber-200 bg-amber-50"><h2 className="font-semibold text-amber-950">Atenção necessária</h2><p className="mt-1 text-sm text-amber-900">O WhatsApp está temporariamente indisponível. Nossa equipe técnica foi informada.</p></Card>}</>}
    {allowed && <Card className="mt-8"><div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between"><div><h2 className="font-semibold text-slate-950">Operações do WhatsApp</h2><p className="mt-1 max-w-2xl text-sm text-slate-600">Verifique a conexão, habilite o canal para esta clínica ou envie uma mensagem de teste segura.</p><p className="mt-2 text-xs text-slate-500">O canal é gerenciado pela plataforma e processado pela Outbox.</p></div><StatusBadge tone={connected ? "success" : "warning"}>{connected ? "Pronta para teste" : "Verificação necessária"}</StatusBadge></div><div className="mt-5 flex flex-wrap gap-2"><Button loading={operation.isPending} onClick={() => operation.mutate("validate")} variant="outline">Verificar conexão</Button><Button loading={operation.isPending} onClick={() => operation.mutate("enable")} variant="success">Habilitar</Button><Button loading={operation.isPending} onClick={() => operation.mutate("disable")} variant="outline">Desativar</Button><Button disabled={!connected} loading={operation.isPending} onClick={() => operation.mutate("testMessage")}><Icon name="messages" />Enviar teste</Button></div>{operation.isError && <div className="mt-4 rounded-control border border-red-200 bg-red-50 p-3 text-sm text-red-800" role="alert"><p>{friendlyOperationError(operation.error)}</p>{operation.error instanceof ApiError && operation.error.traceId && <p className="mt-1 text-xs text-red-700">Código de atendimento: <code>{operation.error.traceId}</code></p>}</div>}{operation.isSuccess && <><p className="mt-4 text-sm text-emerald-700" role="status">Operação solicitada.</p><p className="mt-1 text-xs text-slate-500">A mensagem será processada com segurança.</p></>}</Card>}
  </PageContainer></ProtectedShell>;
}
function formatDate(value?: string | null) { return value ? new Date(value).toLocaleString("pt-BR") : "—"; }
function friendlyOperationError(error: unknown) {
  if (!(error instanceof ApiError)) return "Não foi possível concluir a operação. Tente novamente.";
  const messages: Record<string, string> = { invalid_operation: "O WhatsApp está temporariamente indisponível. Nossa equipe técnica foi informada.", resource_not_found: "Nenhum canal WhatsApp foi encontrado para esta clínica.", unauthorized: "Seu usuário não tem permissão para operar o WhatsApp." };
  return messages[error.code ?? ""] ?? error.message;
}
