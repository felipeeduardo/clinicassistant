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
import { FormField, Input, Select } from "@/components/ui/form";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { StatusBadge } from "@/components/ui/surfaces";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/states";
import { FilterToolbar, RowActionButton, RowActions } from "@/components/ui/admin";

export default function SpecialtiesPage() {
  const api = useApi(); const { user } = useAuth(); const queryClient = useQueryClient();
  const specialties = useQuery({ queryKey: ["specialties"], enabled: user?.role !== "PlatformAdmin", queryFn: () => catalogApi.listSpecialties(api) }); const [editing, setEditing] = useState<Specialty | null | undefined>(); const [dependencies, setDependencies] = useState<{ specialty: Specialty; data: SpecialtyDependencies } | null>(null); const allowed = can(user, "manageCatalog");
  const [search, setSearch] = useState(""); const [statusFilter, setStatusFilter] = useState("");
  const status = useMutation({ mutationFn: ({ id, next }: { id: string; next: "Active" | "Inactive" }) => catalogApi.setSpecialtyStatus(api, id, next), onSuccess: () => queryClient.invalidateQueries({ queryKey: ["specialties"] }) });
  const visibleSpecialties = specialties.data?.filter(item => {
    const term = search.trim().toLocaleLowerCase();
    return (!term || [item.name, item.description ?? ""].some(value => value.toLocaleLowerCase().includes(term))) && (!statusFilter || item.status === statusFilter);
  });
  const viewDependencies = async (item: Specialty) => setDependencies({ specialty: item, data: await catalogApi.getSpecialtyDependencies(api, item.id) });

  return <ProtectedShell><PageContainer><PageHeader actions={allowed ? <Button onClick={() => setEditing(null)}><Icon name="specialties" />Nova especialidade</Button> : undefined} description="Gerencie as especialidades disponíveis para atendimento." pathname="/specialties" title="Especialidades" />
    {editing !== undefined && allowed && <SpecialtyForm specialty={editing ?? undefined} onCancel={() => setEditing(undefined)} onComplete={() => setEditing(undefined)} />}
    <section className="grid gap-4"><FilterToolbar><FormField htmlFor="specialty-search" label="Buscar especialidade"><Input id="specialty-search" onChange={event => setSearch(event.target.value)} placeholder="Nome ou descrição" value={search} /></FormField><FormField htmlFor="specialty-status" label="Status"><Select id="specialty-status" onChange={event => setStatusFilter(event.target.value)} value={statusFilter}><option value="">Todos</option><option value="Active">Ativa</option><option value="Inactive">Inativa</option></Select></FormField></FilterToolbar>{user?.role === "PlatformAdmin" ? <ErrorState title="Acesso restrito à clínica" description="O administrador da plataforma gerencia o provisionamento. Entre com um usuário da clínica para configurar especialidades." /> : specialties.isError || status.isError ? <ErrorState title="Não foi possível carregar as especialidades" onRetry={() => void specialties.refetch()} /> : specialties.isLoading ? <Skeleton className="h-80" /> : visibleSpecialties?.length === 0 ? <EmptyState action={allowed ? <Button onClick={() => setEditing(null)}><Icon name="specialties" />Nova especialidade</Button> : undefined} description="Ajuste os filtros ou cadastre a primeira especialidade para atendimento." title="Nenhuma especialidade encontrada" /> : <DataTable><thead><tr><TableHeader>Especialidade</TableHeader><TableHeader className="hidden md:table-cell">Descrição</TableHeader><TableHeader>Status</TableHeader><TableHeader className="text-right">Ações</TableHeader></tr></thead><tbody>{visibleSpecialties?.map(item => <tr className="transition-colors hover:bg-slate-50" key={item.id}><TableCell className="font-medium">{item.name}</TableCell><TableCell className="hidden max-w-md truncate md:table-cell">{item.description || "—"}</TableCell><TableCell><StatusBadge tone={item.status === "Active" ? "success" : "neutral"}>{item.status === "Active" ? "Ativa" : "Inativa"}</StatusBadge></TableCell><TableCell><RowActions><RowActionButton displayLabel="Ver" icon="eye" tone="view" label={`Visualizar ${item.name}`} onClick={() => void viewDependencies(item)} />{allowed && <RowActionButton displayLabel="Editar" icon="edit" tone="edit" label={`Editar ${item.name}`} onClick={() => setEditing(item)} />}{allowed && <RowActionButton displayLabel={item.status === "Active" ? "Desativar" : "Ativar"} icon="shield" tone={item.status === "Active" ? "deactivate" : "activate"} label={item.status === "Active" ? `Desativar ${item.name}` : `Ativar ${item.name}`} loading={status.isPending} onClick={() => status.mutate({ id: item.id, next: item.status === "Active" ? "Inactive" : "Active" })} />}</RowActions></TableCell></tr>)}</tbody></DataTable>}</section>
    <Drawer description="Impacto administrativo da alteração de status." onClose={() => setDependencies(null)} open={Boolean(dependencies)} title={dependencies ? `Dependências de ${dependencies.specialty.name}` : "Dependências"}>{dependencies && <div className="grid gap-4"><div className="grid gap-3 sm:grid-cols-2"><Metric label="Profissionais vinculados" value={dependencies.data.professionals} /><Metric label="Consultas futuras" value={dependencies.data.futureAppointments} /></div><StatusBadge tone={dependencies.data.canDeactivate ? "success" : "warning"}>{dependencies.data.canDeactivate ? "Pode ser desativada com segurança" : "Existem dependências ativas"}</StatusBadge></div>}</Drawer>
  </PageContainer></ProtectedShell>;
}
function Metric({ label, value }: { label: string; value: number }) { return <div className="rounded-control bg-slate-50 p-4"><p className="text-sm text-slate-600">{label}</p><p className="mt-1 text-2xl font-semibold text-slate-950">{value}</p></div>; }
