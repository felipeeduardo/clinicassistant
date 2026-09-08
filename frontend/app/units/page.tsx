"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { ProtectedShell } from "@/components/protected-shell";
import { UnitForm } from "@/features/catalog/catalog-forms";
import { catalogApi } from "@/lib/api/catalog";
import { can } from "@/lib/auth/permissions";
import type { Unit, UnitDetail } from "@/lib/api/types";
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

export default function UnitsPage() {
  const api = useApi(); const { user } = useAuth(); const queryClient = useQueryClient(); const units = useQuery({ queryKey: ["units"], queryFn: () => catalogApi.listUnits(api) }); const [editing, setEditing] = useState<Unit | null | undefined>(); const [selected, setSelected] = useState<UnitDetail | null>(null); const allowed = can(user, "manageCatalog");
  const [search, setSearch] = useState(""); const [statusFilter, setStatusFilter] = useState("");
  const status = useMutation({ mutationFn: ({ id, next }: { id: string; next: "Active" | "Inactive" }) => catalogApi.setUnitStatus(api, id, next), onSuccess: () => queryClient.invalidateQueries({ queryKey: ["units"] }) });
  const visibleUnits = units.data?.filter(unit => {
    const term = search.trim().toLocaleLowerCase();
    return (!term || [unit.name, unit.address].some(value => value.toLocaleLowerCase().includes(term))) && (!statusFilter || unit.status === statusFilter);
  });
  const viewUnit = async (unit: Unit) => setSelected(await catalogApi.getUnitDetail(api, unit.id));

  return <ProtectedShell><PageContainer><PageHeader actions={allowed ? <Button onClick={() => setEditing(null)}><Icon name="units" />Nova unidade</Button> : undefined} description="Gerencie as unidades e locais de atendimento da clínica." pathname="/units" title="Unidades" />
    {editing !== undefined && allowed && <UnitForm unit={editing ?? undefined} onCancel={() => setEditing(undefined)} onComplete={() => setEditing(undefined)} />}
    <section className="grid gap-4"><FilterToolbar><FormField htmlFor="unit-search" label="Buscar unidade"><Input id="unit-search" onChange={event => setSearch(event.target.value)} placeholder="Nome ou endereço" value={search} /></FormField><FormField htmlFor="unit-status" label="Status"><Select id="unit-status" onChange={event => setStatusFilter(event.target.value)} value={statusFilter}><option value="">Todos</option><option value="Active">Ativa</option><option value="Inactive">Inativa</option></Select></FormField></FilterToolbar>{units.isError || status.isError ? <ErrorState title="Não foi possível carregar as unidades" onRetry={() => void units.refetch()} /> : units.isLoading ? <Skeleton className="h-80" /> : visibleUnits?.length === 0 ? <EmptyState action={allowed ? <Button onClick={() => setEditing(null)}><Icon name="units" />Nova unidade</Button> : undefined} description="Ajuste os filtros ou cadastre a primeira unidade da clínica." title="Nenhuma unidade encontrada" /> : <DataTable><thead><tr><TableHeader>Unidade</TableHeader><TableHeader className="hidden md:table-cell">Endereço</TableHeader><TableHeader>Status</TableHeader><TableHeader className="text-right">Ações</TableHeader></tr></thead><tbody>{visibleUnits?.map(unit => <tr className="transition-colors hover:bg-slate-50" key={unit.id}><TableCell className="font-medium">{unit.name}</TableCell><TableCell className="hidden md:table-cell">{unit.address}</TableCell><TableCell><StatusBadge tone={unit.status === "Active" ? "success" : "neutral"}>{unit.status === "Active" ? "Ativa" : "Inativa"}</StatusBadge></TableCell><TableCell><RowActions><RowActionButton displayLabel="Ver" icon="eye" tone="view" label={`Visualizar ${unit.name}`} onClick={() => void viewUnit(unit)} />{allowed && <RowActionButton displayLabel="Editar" icon="edit" tone="edit" label={`Editar ${unit.name}`} onClick={() => setEditing(unit)} />}{allowed && <RowActionButton displayLabel={unit.status === "Active" ? "Desativar" : "Ativar"} icon="shield" tone={unit.status === "Active" ? "deactivate" : "activate"} label={unit.status === "Active" ? `Desativar ${unit.name}` : `Ativar ${unit.name}`} loading={status.isPending} onClick={() => status.mutate({ id: unit.id, next: unit.status === "Active" ? "Inactive" : "Active" })} />}</RowActions></TableCell></tr>)}</tbody></DataTable>}</section>
    <Drawer description={selected ? `Fuso horário da clínica: ${selected.timeZone}` : undefined} onClose={() => setSelected(null)} open={Boolean(selected)} title={selected?.unit.name ?? "Unidade"}>{selected && <UnitDetailPanel detail={selected} />}</Drawer>
  </PageContainer></ProtectedShell>;
}
function UnitDetailPanel({ detail }: { detail: UnitDetail }) { return <div className="grid gap-6"><section><h3 className="font-semibold text-slate-950">Horários de funcionamento</h3><p className="mt-2 text-sm text-slate-600">{detail.businessHours.length ? detail.businessHours.map(hour => `${day(hour.dayOfWeek)} ${hour.opensAt}–${hour.closesAt}`).join(" · ") : "Não configurados"}</p></section><section><h3 className="font-semibold text-slate-950">Profissionais vinculados</h3><ul className="mt-3 grid gap-2 text-sm">{detail.professionals.map(professional => <li className="flex items-center justify-between rounded-control bg-slate-50 p-3" key={professional.id}><span>{professional.name}</span><StatusBadge tone={professional.status === "Active" ? "success" : "neutral"}>{professional.status}</StatusBadge></li>)}{detail.professionals.length === 0 && <li className="text-slate-600">Nenhum profissional vinculado.</li>}</ul></section></div>; }
function day(value: number) { return ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"][value] ?? "—"; }
