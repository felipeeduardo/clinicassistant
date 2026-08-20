"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { ProtectedShell } from "@/components/protected-shell";
import { SpecialtyForm } from "@/features/catalog/catalog-forms";
import { catalogApi } from "@/lib/api/catalog";
import { can } from "@/lib/auth/permissions";
import type { Specialty, SpecialtyDependencies } from "@/lib/api/types";
import { useApi, useAuth } from "@/providers/providers";
import { Button } from "@/components/ui/button";
import { DataTable, TableCell, TableHeader } from "@/components/ui/data-table";
import { Drawer } from "@/components/ui/drawer";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { StatusBadge } from "@/components/ui/surfaces";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/states";

export default function SpecialtiesPage() {
  const api = useApi(); const { user } = useAuth(); const queryClient = useQueryClient();
  const specialties = useQuery({ queryKey: ["specialties"], enabled: user?.role !== "PlatformAdmin", queryFn: () => catalogApi.listSpecialties(api) }); const [editing, setEditing] = useState<Specialty | null | undefined>(); const [dependencies, setDependencies] = useState<{ specialty: Specialty; data: SpecialtyDependencies } | null>(null); const allowed = can(user, "manageCatalog");
  const status = useMutation({ mutationFn: ({ id, next }: { id: string; next: "Active" | "Inactive" }) => catalogApi.setSpecialtyStatus(api, id, next), onSuccess: () => queryClient.invalidateQueries({ queryKey: ["specialties"] }) });

  return <ProtectedShell><PageContainer><PageHeader actions={<div className="flex flex-wrap gap-2">{allowed && <Button onClick={() => setEditing(null)}><Icon name="specialties" />Nova especialidade</Button>}</div>} description="Organize especialidades e verifique dependências antes de alterar o status." pathname="/specialties" title="Especialidades" />
    {editing !== undefined && allowed && <SpecialtyForm specialty={editing ?? undefined} onCancel={() => setEditing(undefined)} onComplete={() => setEditing(undefined)} />}
    <section className="mt-6">{user?.role === "PlatformAdmin" ? <ErrorState title="Acesso restrito à clínica" description="O administrador da plataforma gerencia o provisionamento. Entre com um usuário da clínica para configurar especialidades." /> : specialties.isError || status.isError ? <ErrorState onRetry={() => void specialties.refetch()} /> : specialties.isLoading ? <Skeleton className="h-80" /> : specialties.data?.length === 0 ? <EmptyState action={allowed ? <Button onClick={() => setEditing(null)}>Cadastrar especialidade</Button> : undefined} description="Crie a primeira especialidade para associá-la a profissionais e consultas." title="Nenhuma especialidade encontrada" /> : <DataTable><thead><tr><TableHeader>Especialidade</TableHeader><TableHeader className="hidden md:table-cell">Descrição</TableHeader><TableHeader>Status</TableHeader><TableHeader><span className="sr-only">Ações</span></TableHeader></tr></thead><tbody>{specialties.data?.map(item => <tr className="transition-colors hover:bg-slate-50" key={item.id}><TableCell className="font-medium">{item.name}</TableCell><TableCell className="hidden max-w-md truncate md:table-cell">{item.description || "—"}</TableCell><TableCell><StatusBadge tone={item.status === "Active" ? "success" : "neutral"}>{item.status === "Active" ? "Ativa" : "Inativa"}</StatusBadge></TableCell><TableCell className="whitespace-nowrap"><Button onClick={async () => setDependencies({ specialty: item, data: await catalogApi.getSpecialtyDependencies(api, item.id) })} size="sm" variant="ghost">Dependências</Button>{can(user, "manageCatalog") && <><Button onClick={() => setEditing(item)} size="sm" variant="ghost">Editar</Button><Button loading={status.isPending} onClick={() => status.mutate({ id: item.id, next: item.status === "Active" ? "Inactive" : "Active" })} size="sm" variant={item.status === "Active" ? "outline" : "success"}>{item.status === "Active" ? "Desativar" : "Ativar"}</Button></>}</TableCell></tr>)}</tbody></DataTable>}</section>
    <Drawer description="Impacto administrativo da alteração de status." onClose={() => setDependencies(null)} open={Boolean(dependencies)} title={dependencies ? `Dependências de ${dependencies.specialty.name}` : "Dependências"}>{dependencies && <div className="grid gap-4"><div className="grid gap-3 sm:grid-cols-2"><Metric label="Profissionais vinculados" value={dependencies.data.professionals} /><Metric label="Consultas futuras" value={dependencies.data.futureAppointments} /></div><StatusBadge tone={dependencies.data.canDeactivate ? "success" : "warning"}>{dependencies.data.canDeactivate ? "Pode ser desativada com segurança" : "Existem dependências ativas"}</StatusBadge></div>}</Drawer>
  </PageContainer></ProtectedShell>;
}
function Metric({ label, value }: { label: string; value: number }) { return <div className="rounded-control bg-slate-50 p-4"><p className="text-sm text-slate-600">{label}</p><p className="mt-1 text-2xl font-semibold text-slate-950">{value}</p></div>; }
