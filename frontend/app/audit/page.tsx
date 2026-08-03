"use client";

import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { LoadingRow, RequestFeedback } from "@/components/request-feedback";
import { ProtectedShell } from "@/components/protected-shell";
import { auditApi } from "@/lib/api/audit";
import { can } from "@/lib/auth/permissions";
import { useApi, useAuth } from "@/providers/providers";

export default function AuditPage() {
  const api = useApi(); const { user } = useAuth(); const [action, setAction] = useState(""); const [resourceType, setResourceType] = useState(""); const allowed = can(user, "manageCatalog");
  const audit = useQuery({ enabled: allowed, queryKey: ["audit", action, resourceType], queryFn: () => auditApi.search(api, { page: 1, pageSize: 100, action, resourceType }) });
  return <ProtectedShell>{!allowed ? <p>Acesso restrito à administração da clínica.</p> : <><h1 className="text-2xl font-bold">Auditoria</h1><p className="mt-1 text-slate-600">Eventos operacionais do tenant atual, sem dados sensíveis.</p><div className="mt-5 flex flex-wrap gap-3"><input className="rounded border p-2" placeholder="Ação exata" value={action} onChange={event => setAction(event.target.value)} /><input className="rounded border p-2" placeholder="Tipo de recurso" value={resourceType} onChange={event => setResourceType(event.target.value)} /></div><div className="mt-5 overflow-x-auto rounded bg-white shadow"><table className="w-full text-left"><thead><tr className="border-b"><th className="p-3">Data</th><th className="p-3">Ação</th><th className="p-3">Recurso</th><th className="p-3">Usuário</th><th className="p-3">Resultado</th></tr></thead><tbody>{audit.isLoading && <LoadingRow columns={5} />}{audit.data?.items.map(item => <tr className="border-b" key={`${item.occurredAt}-${item.action}-${item.resourceId}`}><td className="p-3">{new Date(item.occurredAt).toLocaleString("pt-BR")}</td><td className="p-3">{item.action}</td><td className="p-3">{item.resourceType}</td><td className="p-3">{item.actorName ?? "Sistema"}</td><td className="p-3">{item.result}</td></tr>)}</tbody></table><RequestFeedback error={audit.error} /></div></>}</ProtectedShell>;
}
