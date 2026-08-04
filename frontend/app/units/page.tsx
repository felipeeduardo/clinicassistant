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
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { StatusBadge } from "@/components/ui/surfaces";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/states";

export default function UnitsPage() {
  const api = useApi(); const { user } = useAuth(); const queryClient = useQueryClient(); const units = useQuery({ queryKey: ["units"], queryFn: () => catalogApi.listUnits(api) }); const [editing, setEditing] = useState<Unit | null | undefined>(); const [selected, setSelected] = useState<UnitDetail | null>(null); const allowed = can(user, "manageCatalog");
  const status = useMutation({ mutationFn: ({ id, next }: { id: string; next: "Active" | "Inactive" }) => catalogApi.setUnitStatus(api, id, next), onSuccess: () => queryClient.invalidateQueries({ queryKey: ["units"] }) });

  return <ProtectedShell><PageContainer><PageHeader actions={allowed ? <Button onClick={() => setEditing(null)}><Icon name="units" />Nova unidade</Button> : undefined} description="Gerencie unidades, horários de funcionamento e profissionais vinculados." pathname="/units" title="Unidades" />
    {editing !== undefined && allowed && <UnitForm unit={editing ?? undefined} onCancel={() => setEditing(undefined)} onComplete={() => setEditing(undefined)} />}
    <section className="mt-6">{units.isError || status.isError ? <ErrorState onRetry={() => void units.refetch()} /> : units.isLoading ? <Skeleton className="h-80" /> : units.data?.length === 0 ? <EmptyState action={allowed ? <Button onClick={() => setEditing(null)}>Cadastrar unidade</Button> : undefined} description="Crie uma unidade para organizar a operação da clínica." title="Nenhuma unidade encontrada" /> : <DataTable><thead><tr><TableHeader>Unidade</TableHeader><TableHeader className="hidden md:table-cell">Endereço</TableHeader><TableHeader>Status</TableHeader><TableHeader><span className="sr-only">Ações</span></TableHeader></tr></thead><tbody>{units.data?.map(unit => <tr className="transition-colors hover:bg-slate-50" key={unit.id}><TableCell className="font-medium">{unit.name}</TableCell><TableCell className="hidden md:table-cell">{unit.address}</TableCell><TableCell><StatusBadge tone={unit.status === "Active" ? "success" : "neutral"}>{unit.status === "Active" ? "Ativa" : "Inativa"}</StatusBadge></TableCell><TableCell className="whitespace-nowrap"><Button onClick={async () => setSelected(await catalogApi.getUnitDetail(api, unit.id))} size="sm" variant="ghost">Detalhes</Button>{allowed && <><Button onClick={() => setEditing(unit)} size="sm" variant="ghost">Editar</Button><Button loading={status.isPending} onClick={() => status.mutate({ id: unit.id, next: unit.status === "Active" ? "Inactive" : "Active" })} size="sm" variant={unit.status === "Active" ? "outline" : "success"}>{unit.status === "Active" ? "Desativar" : "Ativar"}</Button></>}</TableCell></tr>)}</tbody></DataTable>}</section>
    <Drawer description={selected ? `Fuso horário da clínica: ${selected.timeZone}` : undefined} onClose={() => setSelected(null)} open={Boolean(selected)} title={selected?.unit.name ?? "Unidade"}>{selected && <UnitDetailPanel detail={selected} />}</Drawer>
  </PageContainer></ProtectedShell>;
}
function UnitDetailPanel({ detail }: { detail: UnitDetail }) { return <div className="grid gap-6"><section><h3 className="font-semibold text-slate-950">Horários de funcionamento</h3><p className="mt-2 text-sm text-slate-600">{detail.businessHours.length ? detail.businessHours.map(hour => `${day(hour.dayOfWeek)} ${hour.opensAt}–${hour.closesAt}`).join(" · ") : "Não configurados"}</p></section><section><h3 className="font-semibold text-slate-950">Profissionais vinculados</h3><ul className="mt-3 grid gap-2 text-sm">{detail.professionals.map(professional => <li className="flex items-center justify-between rounded-control bg-slate-50 p-3" key={professional.id}><span>{professional.name}</span><StatusBadge tone={professional.status === "Active" ? "success" : "neutral"}>{professional.status}</StatusBadge></li>)}{detail.professionals.length === 0 && <li className="text-slate-600">Nenhum profissional vinculado.</li>}</ul></section></div>; }
function day(value: number) { return ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"][value] ?? "—"; }
