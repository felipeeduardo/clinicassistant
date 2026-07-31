"use client";

import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { LoadingRow, RequestFeedback } from "@/components/request-feedback";
import { ProtectedShell } from "@/components/protected-shell";
import { SpecialtyForm } from "@/features/catalog/catalog-forms";
import { catalogApi } from "@/lib/api/catalog";
import { can } from "@/lib/auth/permissions";
import type { Specialty } from "@/lib/api/types";
import { useApi, useAuth } from "@/providers/providers";

export default function SpecialtiesPage() { const api = useApi(); const { user } = useAuth(); const specialties = useQuery({ queryKey: ["specialties"], queryFn: () => catalogApi.listSpecialties(api) }); const [editing, setEditing] = useState<Specialty | null | undefined>(); const allowed = can(user, "manageCatalog"); return <ProtectedShell><header className="flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-bold">Especialidades</h1><p className="mt-1 text-slate-600">Cadastros do tenant atual.</p></div>{allowed && <button className="rounded bg-blue-700 px-4 py-2 text-white" onClick={() => setEditing(null)}>Nova especialidade</button>}</header>{editing !== undefined && allowed && <SpecialtyForm specialty={editing ?? undefined} onComplete={() => setEditing(undefined)} onCancel={() => setEditing(undefined)} />}<div className="mt-6 overflow-x-auto rounded bg-white shadow"><table className="w-full text-left"><thead><tr className="border-b"><th className="p-3">Nome</th><th className="p-3">Descrição</th><th className="p-3">Status</th>{allowed && <th className="p-3"><span className="sr-only">Ações</span></th>}</tr></thead><tbody>{specialties.isLoading && <LoadingRow columns={allowed ? 4 : 3} />}{specialties.data?.map(item => <tr className="border-b" key={item.id}><td className="p-3">{item.name}</td><td className="p-3">{item.description || "—"}</td><td className="p-3">{item.status}</td>{allowed && <td className="p-3"><button className="text-blue-700 underline" onClick={() => setEditing(item)}>Editar</button></td>}</tr>)}</tbody></table><RequestFeedback error={specialties.error} /></div></ProtectedShell>; }
