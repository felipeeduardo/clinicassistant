"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { ProtectedShell } from "@/components/protected-shell";
import { Button } from "@/components/ui/button";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { ErrorState, Skeleton } from "@/components/ui/states";
import { platformApi } from "@/lib/api/platform";
import { useApi, useAuth } from "@/providers/providers";

export default function PlatformClinicPage() {
  const api = useApi();
  const { user } = useAuth();
  const { tenantId } = useParams<{ tenantId: string }>();
  const client = useQueryClient();
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [adminEmail, setAdminEmail] = useState("");
  const [confirmation, setConfirmation] = useState("");
  const tenants = useQuery({ queryKey: ["platform", "tenants"], queryFn: () => platformApi.tenants(api), enabled: user?.role === "PlatformAdmin" });
  const readiness = useQuery({ queryKey: ["platform", "onboarding", tenantId], queryFn: () => platformApi.onboardingStatus(api, tenantId), enabled: user?.role === "PlatformAdmin" && Boolean(tenantId) });
  const whatsapp = useQuery({ queryKey: ["platform", "whatsapp", tenantId], queryFn: () => platformApi.whatsappStatus(api, tenantId), enabled: user?.role === "PlatformAdmin" && Boolean(tenantId) });
  const tenant = tenants.data?.find(item => item.id === tenantId);
  const lifecycle = useMutation({ mutationFn: (action: "activate" | "suspend" | "disable") => platformApi.status(api, tenantId, action), onSuccess: () => { void tenants.refetch(); void client.invalidateQueries({ queryKey: ["platform", "onboarding", tenantId] }); } });
  const deletion = useMutation({ mutationFn: () => platformApi.deleteTenant(api, tenantId, { clinicAdminEmail: adminEmail, confirmation }), onSuccess: () => { void client.invalidateQueries({ queryKey: ["platform", "tenants"] }); window.location.href = "/platform"; } });

  if (user?.role !== "PlatformAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/platform" title="Acesso negado" description="Esta área é exclusiva da administração da plataforma." /></PageContainer></ProtectedShell>;
  if (tenants.isLoading || readiness.isLoading || whatsapp.isLoading) return <ProtectedShell><PageContainer><Skeleton className="h-96" /></PageContainer></ProtectedShell>;
  if (tenants.isError || readiness.isError || whatsapp.isError || !tenant || !readiness.data || !whatsapp.data) return <ProtectedShell><PageContainer><ErrorState title="Não foi possível carregar a clínica" onRetry={() => { void tenants.refetch(); void readiness.refetch(); void whatsapp.refetch(); }} /></PageContainer></ProtectedShell>;

  const status = readiness.data;
  const configured = [status.clinicConfigured, status.unitConfigured, status.specialtiesConfigured, status.professionalsConfigured, status.availabilityConfigured, status.clinicAdminConfigured, status.whatsAppConfigured].filter(Boolean).length;
  const canDelete = confirmation === tenant.slug && adminEmail.trim().length > 0;
  return <ProtectedShell><PageContainer><PageHeader actions={<Link className="inline-flex items-center gap-2 rounded-control border border-border-system px-4 py-2 text-sm font-semibold text-slate-800 hover:bg-slate-50" href="/platform"><Icon name="chevronRight" className="rotate-180" />Voltar</Link>} description="Informações globais e estado do tenant. A configuração operacional é feita pelo ClinicAdmin." pathname="/platform/clinics" title={tenant.name} />
    <div className="mt-6 grid gap-5">
      <Card><div className="flex flex-wrap items-start justify-between gap-4"><div><p className="text-sm text-slate-500">Tenant: {tenant.id}</p><p className="mt-1 text-sm text-slate-600">Identificador: {tenant.slug}</p><div className="mt-3 flex items-center gap-2"><StatusBadge tone={statusTone(tenant.status)}>{statusLabel(tenant.status)}</StatusBadge><span className="text-sm text-slate-600">{configured}/7 módulos informativos concluídos</span></div></div><div className="flex flex-wrap gap-2"><Button disabled={tenant.status === "Active"} loading={lifecycle.isPending} onClick={() => lifecycle.mutate("activate")} size="sm" variant="outline"><Icon name="arrowRight" />Ativar</Button><Button disabled={tenant.status === "Suspended"} loading={lifecycle.isPending} onClick={() => lifecycle.mutate("suspend")} size="sm" variant="outline"><Icon name="history" />Suspender</Button><Button loading={lifecycle.isPending} onClick={() => lifecycle.mutate("disable")} size="sm" variant="danger"><Icon name="close" />Desativar</Button></div></div>{lifecycle.isError && <p className="mt-4 rounded-control border border-red-200 bg-red-50 p-3 text-sm text-red-800" role="alert">Não foi possível alterar o ciclo de vida do tenant.</p>}</Card>
      <section className="grid gap-4 md:grid-cols-2"><ReadOnly label="Estado da clínica" value={status.clinicConfigured ? "Dados básicos cadastrados" : "Pendente"} /><ReadOnly label="Unidade inicial" value={status.unitConfigured ? "Cadastrada" : "Pendente"} /><ReadOnly label="ClinicAdmin" value={status.clinicAdminConfigured ? `${tenant.clinicAdminName ?? "Associado"}${tenant.clinicAdminEmail ? ` · ${tenant.clinicAdminEmail}` : ""}` : "Não associado"} /><ReadOnly label="Configuração operacional" value={`${configured}/7 módulos concluídos pelo ClinicAdmin`} /><ReadOnly label="WhatsApp" value={whatsapp.data.configured ? `${whatsapp.data.provider} · ${whatsappStatusLabel(whatsapp.data.status)} · ${whatsapp.data.displayPhoneNumber ?? "telefone não informado"}` : "Ainda não configurado"} /><ReadOnly label="Usuários" value={`${tenant.userCount} usuário(s) associado(s)`} /></section>
      <Card><h2 className="font-semibold text-slate-950">Responsabilidade operacional</h2><p className="mt-1 text-sm text-slate-600">Especialidades, profissionais, disponibilidade, usuários internos, agenda e WhatsApp são configurados exclusivamente pelo ClinicAdmin dentro do próprio tenant.</p></Card>
      <Card className="border-red-200 bg-red-50/40"><div className="flex items-start gap-3"><Icon className="mt-0.5 text-red-700" name="shield" /><div className="min-w-0 flex-1"><h2 className="font-semibold text-red-950">Zona de exclusão permanente</h2><p className="mt-1 text-sm text-red-900">Exclui definitivamente todos os registros desta clínica, incluindo usuários, agenda, conversas, mensagens, integrações e auditoria. Esta ação não pode ser desfeita.</p>{!deleteOpen ? <Button className="mt-4" onClick={() => setDeleteOpen(true)} size="sm" variant="danger"><Icon name="close" />Excluir clínica definitivamente</Button> : <div className="mt-4 grid gap-3 rounded-control border border-red-200 bg-white p-4"><p className="text-sm font-semibold text-red-950">Confirme a exclusão informando o e-mail do ClinicAdmin ativo e digitando o slug completo:</p><label className="grid gap-1 text-sm font-medium text-slate-700">E-mail do usuário administrador<input className="min-h-10 rounded-control border border-border-system px-3" onChange={event => setAdminEmail(event.target.value)} placeholder="admin@clinica.com.br" type="email" value={adminEmail} /></label><label className="grid gap-1 text-sm font-medium text-slate-700">Digite <code className="font-mono text-red-800">{tenant.slug}</code> para confirmar<input className="min-h-10 rounded-control border border-border-system px-3 font-mono" onChange={event => setConfirmation(event.target.value)} placeholder={tenant.slug} value={confirmation} /></label>{deletion.isError && <p className="text-sm text-red-800" role="alert">Não foi possível excluir a clínica. Verifique o e-mail do ClinicAdmin e o slug informado.</p>}<div className="flex flex-wrap gap-2"><Button onClick={() => { setDeleteOpen(false); setAdminEmail(""); setConfirmation(""); }} size="sm" variant="outline">Cancelar</Button><Button disabled={!canDelete} loading={deletion.isPending} onClick={() => { if (window.confirm("A exclusão será permanente. Deseja continuar?")) deletion.mutate(); }} size="sm" variant="danger"><Icon name="close" />Confirmar exclusão permanente</Button></div></div>}</div></div></Card>
    </div>
  </PageContainer></ProtectedShell>;
}

function ReadOnly({ label, value }: { label: string; value: string }) { return <Card><p className="text-xs font-semibold uppercase tracking-wide text-slate-500">{label}</p><p className="mt-2 font-medium text-slate-950">{value}</p></Card>; }
function statusTone(status: string) { return (status === "Active" ? "success" : status === "Suspended" ? "warning" : status === "Provisioning" ? "info" : "danger") as "success" | "warning" | "info" | "danger"; }
function statusLabel(status: string) { return ({ Provisioning: "Em configuração", Active: "Ativa", Suspended: "Suspensa", Disabled: "Desativada" } as Record<string, string>)[status] ?? status; }
function whatsappStatusLabel(status: string | null) { return ({ Connected: "conectado", Pending: "pendente", InvalidCredentials: "credenciais inválidas", Suspended: "suspenso", Disabled: "desativado" } as Record<string, string>)[status ?? ""] ?? "não configurado"; }
