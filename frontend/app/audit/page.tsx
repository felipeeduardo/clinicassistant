"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { DataTable, Pagination, TableCell, TableHeader } from "@/components/ui/data-table";
import { Drawer } from "@/components/ui/drawer";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { ProtectedShell } from "@/components/protected-shell";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { ErrorState, Skeleton } from "@/components/ui/states";
import { auditApi } from "@/lib/api/audit";
import { can } from "@/lib/auth/permissions";
import { useDebouncedValue } from "@/hooks/use-debounced-value";
import { useApi, useAuth } from "@/providers/providers";
import type { AuditItem } from "@/lib/api/types";

export default function AuditPage() {
  const api = useApi(); const { user } = useAuth(); const allowed = can(user, "manageCatalog");
  const [action, setAction] = useState(""); const [resourceType, setResourceType] = useState(""); const [page, setPage] = useState(1); const [selected, setSelected] = useState<AuditItem | null>(null);
  const debouncedAction = useDebouncedValue(action); const debouncedResourceType = useDebouncedValue(resourceType);
  const audit = useQuery({ enabled: allowed, placeholderData: keepPreviousData, queryKey: ["audit", page, debouncedAction, debouncedResourceType], queryFn: () => auditApi.search(api, { page, pageSize: 25, action: debouncedAction, resourceType: debouncedResourceType }) });
  if (!allowed) return <ProtectedShell><PageContainer><PageHeader pathname="/audit" title="Auditoria" /><p className="mt-6">Acesso restrito à administração da clínica.</p></PageContainer></ProtectedShell>;
  return <ProtectedShell><PageContainer><PageHeader description="Eventos operacionais do tenant, sem exposição de payloads sensíveis." pathname="/audit" title="Auditoria" />
    <Card className="mt-6"><div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-[1fr_1fr_auto]"><label className="grid gap-1 text-sm font-medium">Ação<input className="rounded-control border border-slate-300 px-3 py-2" placeholder="Ex.: appointment.created" value={action} onChange={event => { setPage(1); setAction(event.target.value); }} /></label><label className="grid gap-1 text-sm font-medium">Recurso<input className="rounded-control border border-slate-300 px-3 py-2" placeholder="Ex.: Appointment" value={resourceType} onChange={event => { setPage(1); setResourceType(event.target.value); }} /></label><Button className="self-end" onClick={() => { setAction(""); setResourceType(""); setPage(1); }} variant="outline">Limpar filtros</Button></div></Card>
    <section className="mt-6">{audit.isError ? <ErrorState onRetry={() => void audit.refetch()} title="Não foi possível carregar a auditoria" /> : audit.isLoading ? <Card><Skeleton className="h-80" /></Card> : audit.data?.items.length === 0 ? <Card className="p-8 text-center text-slate-600">Nenhum evento encontrado.</Card> : <><DataTable><thead><tr><TableHeader>Data</TableHeader><TableHeader>Ação</TableHeader><TableHeader>Recurso</TableHeader><TableHeader>Usuário</TableHeader><TableHeader>Resultado</TableHeader><TableHeader><span className="sr-only">Detalhes</span></TableHeader></tr></thead><tbody>{audit.data?.items.map(item => <tr key={`${item.occurredAt}-${item.action}-${item.resourceId}`}><TableCell>{new Date(item.occurredAt).toLocaleString("pt-BR")}</TableCell><TableCell className="font-medium">{item.action}</TableCell><TableCell>{item.resourceType}</TableCell><TableCell>{item.actorName ?? "Sistema"}</TableCell><TableCell><StatusBadge tone={item.result === "Succeeded" ? "success" : "danger"}>{item.result}</StatusBadge></TableCell><TableCell><Button onClick={() => setSelected(item)} size="sm" variant="ghost">Detalhes</Button></TableCell></tr>)}</tbody></DataTable><div className="mt-4"><Pagination hasNext={(audit.data?.items.length ?? 0) === 25} hasPrevious={page > 1} onNext={() => setPage(value => value + 1)} onPrevious={() => setPage(value => Math.max(1, value - 1))} page={page} /></div></>}</section>
    <Drawer description="Metadados seguros do evento; o payload integral não é exibido." onClose={() => setSelected(null)} open={Boolean(selected)} title="Detalhes do evento">{selected && <dl className="grid gap-4 text-sm">{[["Data", new Date(selected.occurredAt).toLocaleString("pt-BR")], ["Ação", selected.action], ["Recurso", selected.resourceType], ["Identificador", selected.resourceId ?? "Não informado"], ["Usuário", selected.actorName ?? "Sistema"], ["Resultado", selected.result]].map(([label, value]) => <div key={label}><dt className="text-slate-500">{label}</dt><dd className="mt-1 break-all font-medium text-slate-950">{value}</dd></div>)}</dl>}</Drawer>
  </PageContainer></ProtectedShell>;
}
