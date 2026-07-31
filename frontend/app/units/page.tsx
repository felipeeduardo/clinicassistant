"use client";

import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { LoadingRow, RequestFeedback } from "@/components/request-feedback";
import { ProtectedShell } from "@/components/protected-shell";
import { UnitForm } from "@/features/catalog/catalog-forms";
import { catalogApi } from "@/lib/api/catalog";
import { can } from "@/lib/auth/permissions";
import type { Unit } from "@/lib/api/types";
import { useApi, useAuth } from "@/providers/providers";

export default function UnitsPage() {
  const api = useApi(); const { user } = useAuth(); const units = useQuery({ queryKey: ["units"], queryFn: () => catalogApi.listUnits(api) }); const [editing, setEditing] = useState<Unit | null | undefined>(); const allowed = can(user, "manageCatalog");
  return <ProtectedShell><header className="flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-bold">Unidades</h1><p className="mt-1 text-slate-600">Gerencie unidades da clínica do tenant atual.</p></div>{allowed && <button className="rounded bg-blue-700 px-4 py-2 text-white" onClick={() => setEditing(null)}>Nova unidade</button>}</header>{editing !== undefined && allowed && <UnitForm unit={editing ?? undefined} onComplete={() => setEditing(undefined)} onCancel={() => setEditing(undefined)} />}<div className="mt-6 overflow-x-auto rounded bg-white shadow"><table className="w-full text-left"><thead><tr className="border-b"><th className="p-3">Nome</th><th className="p-3">Endereço</th><th className="p-3">Status</th>{allowed && <th className="p-3"><span className="sr-only">Ações</span></th>}</tr></thead><tbody>{units.isLoading && <LoadingRow columns={allowed ? 4 : 3} />}{units.data?.map(unit => <tr className="border-b" key={unit.id}><td className="p-3">{unit.name}</td><td className="p-3">{unit.address}</td><td className="p-3">{unit.status}</td>{allowed && <td className="p-3"><button className="text-blue-700 underline" onClick={() => setEditing(unit)}>Editar</button></td>}</tr>)}</tbody></table><RequestFeedback error={units.error} /></div></ProtectedShell>;
}
