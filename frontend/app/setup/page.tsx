"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { Icon } from "@/components/ui/icon";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { ErrorState, Skeleton } from "@/components/ui/states";
import { catalogApi } from "@/lib/api/catalog";
import { useApi, useAuth } from "@/providers/providers";

export default function ClinicSetupPage() {
  const api = useApi(); const { user } = useAuth();
  const setup = useQuery({ queryKey: ["clinic", "setup"], queryFn: () => catalogApi.getClinicSetup(api), enabled: user?.role === "ClinicAdmin" });
  if (user?.role !== "ClinicAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/setup" title="Acesso restrito" description="Esta configuração pertence ao administrador da clínica." /></PageContainer></ProtectedShell>;
  if (setup.isLoading) return <ProtectedShell><PageContainer><Skeleton className="h-96" /></PageContainer></ProtectedShell>;
  if (setup.isError || !setup.data) return <ProtectedShell><PageContainer><ErrorState title="Não foi possível carregar a configuração" onRetry={() => void setup.refetch()} /></PageContainer></ProtectedShell>;
  const status = setup.data;
  const items = [
    ["Dados básicos", status.clinicConfigured, "/clinics", "Revise os dados institucionais da clínica."],
    ["Unidades", status.unitConfigured, "/units", "Cadastre unidades adicionais quando necessário."],
    ["Especialidades", status.specialtiesConfigured, "/specialties", "Crie as áreas de atendimento."],
    ["Profissionais", status.professionalsConfigured, "/professionals", "Vincule profissionais às especialidades."],
    ["Disponibilidade", status.availabilityConfigured, "/professionals", "Configure horários, bloqueios e férias."],
    ["WhatsApp", status.whatsAppConfigured, "/integrations/whatsapp", "Conecte o canal de atendimento da clínica."],
  ] as const;
  const completed = items.filter(item => item[1]).length;
  return <ProtectedShell><PageContainer><PageHeader description="Configure sua operação no seu ritmo. O progresso é calculado pelos dados reais do tenant." pathname="/setup" title="Configuração da clínica" />
    <Card className="mt-6"><div className="flex flex-wrap items-center justify-between gap-4"><div><p className="text-sm text-slate-600">Olá, {user.name}.</p><h2 className="mt-1 text-xl font-semibold text-slate-950">Vamos configurar sua clínica</h2><p className="mt-1 text-sm text-slate-600">{completed} de {items.length} módulos concluídos.</p></div><div className="h-2 w-44 overflow-hidden rounded-full bg-slate-200"><div className="h-full rounded-full bg-brand-primary" style={{ width: `${completed / items.length * 100}%` }} /></div></div></Card>
    <section className="mt-5 grid gap-4 md:grid-cols-2">{items.map(([label, complete, href, description]) => <Card className="flex items-center justify-between gap-4" key={label}><div><div className="flex items-center gap-2"><h2 className="font-semibold text-slate-950">{label}</h2><StatusBadge tone={complete ? "success" : "neutral"}>{complete ? "Concluído" : "Pendente"}</StatusBadge></div><p className="mt-1 text-sm text-slate-600">{description}</p></div>{!complete && <Link className="inline-flex shrink-0 items-center gap-1 text-sm font-semibold text-brand-primary hover:underline" href={href}>Configurar <Icon name="chevronRight" /></Link>}</Card>)}</section>
  </PageContainer></ProtectedShell>;
}
