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
import Link from "next/link";

export default function WhatsAppIntegrationPage() {
  const api = useApi(); const { user } = useAuth(); const client = useQueryClient();
  const integration = useQuery({ queryKey: ["integration-status"], queryFn: () => whatsAppApi.status(api) }); const allowed = can(user, "manageCatalog");
  const operation = useMutation({ mutationFn: (action: "validate" | "enable" | "disable" | "testMessage") => whatsAppApi[action](api), onSuccess: () => client.invalidateQueries({ queryKey: ["integration-status"] }) });
  const data = integration.data; const connected = data?.status === "Connected";

  return <ProtectedShell><PageContainer><PageHeader description="Acompanhe o estado operacional da integração autorizada para o tenant atual." pathname="/integrations/whatsapp" title="Integração WhatsApp" />
    {integration.isError ? <div className="mt-6"><ErrorState onRetry={() => void integration.refetch()} /></div> : integration.isLoading ? <section className="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">{Array.from({ length: 6 }, (_, index) => <Skeleton className="h-32" key={index} />)}</section> : data && <><section className="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-3"><StatCard label="Provider" value={data.provider}><Icon className="text-brand-600" name="whatsapp" /></StatCard><StatCard label="Status" value={data.status}><StatusBadge tone={connected ? "success" : "warning"}>{connected ? "Conectada" : "Atenção"}</StatusBadge></StatCard><StatCard label="Número autorizado" value={data.displayPhoneNumber || "Não informado"} /><StatCard label="Último webhook" value={formatDate(data.lastWebhookAt)} /><StatCard label="Último envio" value={formatDate(data.lastSuccessfulSendAt)} /><StatCard label="Última falha" value={formatDate(data.lastFailureAt)} /></section>{data.failureReason && <Card className="mt-4 border-amber-200 bg-amber-50"><h2 className="font-semibold text-amber-950">Diagnóstico operacional</h2><p className="mt-1 text-sm text-amber-900">{data.failureReason}</p></Card>}</>}
    {allowed && <Card className="mt-8"><div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between"><div><h2 className="font-semibold text-slate-950">Operações administrativas</h2><p className="mt-1 max-w-2xl text-sm text-slate-600">As operações não expõem credenciais. A mensagem de teste usa exclusivamente o destinatário permitido no ambiente e é enviada pela Outbox.</p></div><StatusBadge tone={connected ? "success" : "warning"}>{connected ? "Pronta para teste" : "Validação necessária"}</StatusBadge></div><div className="mt-5 flex flex-wrap gap-2"><Button loading={operation.isPending} onClick={() => operation.mutate("validate")} variant="outline">Validar configuração</Button><Button loading={operation.isPending} onClick={() => operation.mutate("enable")} variant="success">Habilitar</Button><Button loading={operation.isPending} onClick={() => operation.mutate("disable")} variant="outline">Desabilitar</Button><Button disabled={!connected} loading={operation.isPending} onClick={() => operation.mutate("testMessage")}><Icon name="messages" />Enviar teste</Button><Link className="inline-flex min-h-10 items-center rounded-control border border-slate-300 bg-white px-4 text-sm font-medium text-slate-800 hover:bg-slate-50" href="/settings/integrations/twilio">Configuração segura</Link></div>{operation.isError && <p className="mt-4 text-sm text-red-700" role="alert">Não foi possível concluir a operação. Tente novamente.</p>}{operation.isSuccess && <p className="mt-4 text-sm text-emerald-700" role="status">Operação solicitada.</p>}</Card>}
  </PageContainer></ProtectedShell>;
}
function formatDate(value?: string) { return value ? new Date(value).toLocaleString("pt-BR") : "—"; }
