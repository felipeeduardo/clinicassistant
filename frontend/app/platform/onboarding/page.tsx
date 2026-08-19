"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ProtectedShell } from "@/components/protected-shell";
import { Button } from "@/components/ui/button";
import { FormActions, FormErrorSummary, FormField, FormSection, Input } from "@/components/ui/form";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { platformApi } from "@/lib/api/platform";
import type { OnboardTenantRequest, PlatformOnboardingStatus } from "@/lib/api/types";
import { useApi, useAuth } from "@/providers/providers";

const schema = z.object({
  tenantName: z.string().min(2, "Informe o nome da clínica."),
  tenantSlug: z.string().regex(/^[a-z0-9-]+$/, "Use apenas letras minúsculas, números e hífens."),
  clinicLegalName: z.string().min(2, "Informe a razão social."),
  clinicTradeName: z.string().min(2, "Informe o nome de exibição."),
  clinicDocument: z.string().min(5, "Informe o documento da clínica."),
  clinicEmail: z.string().email("Informe um e-mail válido."),
  clinicPhone: z.string().min(8, "Informe um telefone válido."),
  timeZone: z.string().min(3, "Informe o fuso horário."),
  unitName: z.string().min(2, "Informe o nome da unidade."),
  unitAddress: z.string().min(5, "Informe o endereço da unidade."),
  unitPhone: z.string().min(8, "Informe um telefone válido."),
  adminName: z.string().min(2, "Informe o nome do administrador."),
  adminEmail: z.string().email("Informe um e-mail válido."),
  temporaryPassword: z.string().min(12, "Use uma senha com pelo menos 12 caracteres."),
});

const steps = ["Clínica", "Unidade", "Especialidades", "Profissionais", "Horários", "Administrador", "WhatsApp", "Revisão"] as const;

export default function OnboardingPage() {
  const api = useApi();
  const { user } = useAuth();
  const client = useQueryClient();
  const key = useRef(crypto.randomUUID());
  const [tenantId, setTenantId] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const form = useForm<OnboardTenantRequest>({ resolver: zodResolver(schema), defaultValues: { timeZone: "America/Recife" } });
  const mutation = useMutation({ mutationFn: (request: OnboardTenantRequest) => platformApi.onboard(api, request, key.current), onSuccess: result => { setTenantId(result.tenantId); void client.invalidateQueries({ queryKey: ["platform"] }); } });
  const readiness = useQuery({ queryKey: ["platform", "onboarding", tenantId], queryFn: () => platformApi.onboardingStatus(api, tenantId!), enabled: Boolean(tenantId), refetchInterval: tenantId ? 5_000 : false });
  const activation = useMutation({ mutationFn: () => platformApi.status(api, tenantId!, "activate"), onSuccess: () => { void readiness.refetch(); void client.invalidateQueries({ queryKey: ["platform"] }); } });
  const errors = Object.values(form.formState.errors).map(error => error?.message).filter((message): message is string => typeof message === "string");
  const feedback = mutation.error instanceof Error ? friendlyError(mutation.error.message) : mutation.error ? "Não foi possível salvar a clínica agora." : "";

  if (user?.role !== "PlatformAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/platform/onboarding" title="Acesso negado" description="Esta área é exclusiva da administração da plataforma." /></PageContainer></ProtectedShell>;

  const completed = readiness.data ? countCompleted(readiness.data) : 0;
  const currentStep = readiness.data ? Math.min(steps.length, completed + 1) : 1;
  return <ProtectedShell><PageContainer>
    <PageHeader actions={<Button onClick={() => history.back()} variant="outline">Voltar</Button>} description="Configure uma clínica em etapas simples e acompanhe o progresso até a ativação." pathname="/platform/onboarding" title="Configurar nova clínica" />
    <div className="mx-auto mt-6 grid max-w-6xl gap-6">
      <Card className="overflow-hidden p-0">
        <div className="border-b border-border-system bg-surface-subtle px-5 py-5 sm:px-7">
          <div className="flex flex-wrap items-end justify-between gap-3"><div><p className="text-xs font-semibold uppercase tracking-[0.16em] text-brand-primary">Etapa {currentStep} de {steps.length}</p><h2 className="mt-1 text-xl font-semibold text-slate-950">Comece pelos dados essenciais</h2><p className="mt-1 max-w-2xl text-sm text-slate-600">Depois desta etapa, você poderá completar catálogos, horários e canais da clínica.</p></div><StatusBadge tone={readiness.data?.canActivate ? "success" : "info"}>{readiness.data?.canActivate ? "Pronta para ativar" : `${completed} de ${steps.length} etapas`}</StatusBadge></div>
          <Stepper current={currentStep} status={readiness.data} />
        </div>
        <form className="grid gap-6 p-5 sm:p-7" onSubmit={form.handleSubmit(request => mutation.mutate(request))}>
          <p className="text-sm text-slate-600">Campos marcados com <span className="font-semibold text-red-700">*</span> são obrigatórios.</p>
          <FormErrorSummary errors={errors} />
          <FormSection description="Como a clínica será identificada na plataforma." title="Identidade da clínica"><Field form={form} name="tenantName" label="Nome da clínica" placeholder="Clínica Vida" required /><Field form={form} name="tenantSlug" label="Identificador da clínica" hint="Use letras minúsculas, números e hífens; ele deve ser único." placeholder="clinica-vida" required /><Field form={form} name="clinicLegalName" label="Razão social" placeholder="Clínica Vida Serviços de Saúde LTDA" required /><Field form={form} name="clinicTradeName" label="Nome de exibição" placeholder="Clínica Vida" required /></FormSection>
          <FormSection description="Dados usados para contato, agenda e documentos." title="Contato e operação"><Field form={form} name="clinicDocument" label="Documento" placeholder="00.000.000/0000-00" required /><Field form={form} name="clinicEmail" label="E-mail da clínica" placeholder="contato@clinicavida.com.br" required type="email" /><Field form={form} name="clinicPhone" label="Telefone da clínica" placeholder="(81) 99999-9999" required /><Field form={form} name="timeZone" label="Fuso horário" hint="Usado para agenda, horários e notificações da clínica." placeholder="America/Recife" required /></FormSection>
          <FormSection description="Local onde os primeiros atendimentos serão realizados." title="Unidade principal"><Field form={form} name="unitName" label="Nome da unidade" placeholder="Unidade Boa Viagem" required /><Field className="md:col-span-2" form={form} name="unitAddress" label="Endereço" placeholder="Av. Exemplo, 100 — Recife, PE" required /><Field form={form} name="unitPhone" label="Telefone da unidade" placeholder="(81) 99999-9999" required /></FormSection>
          <FormSection description="A credencial é temporária e deve ser trocada no primeiro acesso." title="Administrador da clínica"><Field form={form} name="adminName" label="Nome completo" placeholder="Nome do administrador" required /><Field form={form} name="adminEmail" label="E-mail de acesso" placeholder="admin@clinicavida.com.br" required type="email" autoComplete="email" /><Field className="md:col-span-2" form={form} name="temporaryPassword" label="Senha temporária" hint="Mínimo de 12 caracteres. O valor não será exibido novamente." placeholder="••••••••••••" required type={showPassword ? "text" : "password"} autoComplete="new-password" trailing={<button aria-label={showPassword ? "Ocultar senha" : "Mostrar senha"} className="text-xs font-semibold text-brand-primary hover:underline" onClick={() => setShowPassword(value => !value)} type="button">{showPassword ? "Ocultar" : "Mostrar"}</button>} /></FormSection>
          <FormActions><Button loading={mutation.isPending} type="submit">{mutation.isPending ? "Salvando..." : "Salvar e continuar"}</Button><Button onClick={() => history.back()} variant="outline">Cancelar</Button></FormActions>
          <FormErrorSummary errors={feedback ? [feedback] : []} />
          {mutation.isSuccess && <p className="rounded-control border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-900" role="status">Clínica salva. Continue pelos itens abaixo para concluir a configuração.</p>}
        </form>
      </Card>
      {readiness.data && <ReadinessCard activationError={activation.error instanceof Error ? friendlyError(activation.error.message) : undefined} activating={activation.isPending} onActivate={() => activation.mutate()} status={readiness.data} />}
    </div>
  </PageContainer></ProtectedShell>;
}

function Stepper({ current, status }: { current: number; status?: PlatformOnboardingStatus }) { return <div className="mt-6"><div className="mb-2 flex items-center justify-between text-xs text-slate-500"><span>Progresso da configuração</span><span>{status ? `${countCompleted(status)} de ${steps.length} concluídas` : "Primeira etapa"}</span></div><div aria-label={`Etapa ${current} de ${steps.length}`} className="h-2 overflow-hidden rounded-full bg-slate-200" role="progressbar" aria-valuemax={steps.length} aria-valuemin={0} aria-valuenow={status ? countCompleted(status) : 0}><div className="h-full rounded-full bg-brand-primary transition-all" style={{ width: `${(status ? countCompleted(status) : 0) / steps.length * 100}%` }} /></div><ol className="mt-4 hidden gap-2 md:grid md:grid-cols-8">{steps.map((step, index) => { const complete = status ? isStepComplete(status, index) : false; const active = index + 1 === current; return <li className={`flex items-center gap-2 text-xs ${active ? "font-semibold text-brand-primary" : complete ? "text-emerald-700" : "text-slate-500"}`} key={step}><span aria-hidden="true" className={`grid size-5 shrink-0 place-items-center rounded-full border text-[10px] ${complete ? "border-emerald-600 bg-emerald-600 text-white" : active ? "border-brand-primary bg-brand-primary text-white" : "border-slate-300 bg-white"}`}>{complete ? "✓" : index + 1}</span><span>{step}</span></li>; })}</ol></div>; }

function ReadinessCard({ status, activating, activationError, onActivate }: { status: PlatformOnboardingStatus; activating: boolean; activationError?: string; onActivate: () => void }) { const items = [["Clínica", status.clinicConfigured, "/clinics"], ["Unidade", status.unitConfigured, "/units"], ["Especialidades", status.specialtiesConfigured, "/specialties"], ["Profissionais", status.professionalsConfigured, "/professionals"], ["Horários", status.availabilityConfigured, "/professionals"], ["Administrador", status.clinicAdminConfigured, "/platform"], ["WhatsApp", status.whatsAppConfigured, "/integrations/whatsapp"]] as const; return <Card><div className="flex flex-wrap items-start justify-between gap-3"><div><p className="text-xs font-semibold uppercase tracking-[0.16em] text-brand-primary">Próximos passos</p><h2 className="mt-1 text-lg font-semibold text-slate-950">Revise a configuração da clínica</h2><p className="mt-1 text-sm text-slate-600">Você pode concluir os itens pendentes agora ou configurar o WhatsApp depois.</p></div>{status.canActivate && <StatusBadge tone="success">Pronta para ativar</StatusBadge>}</div><ul className="mt-5 grid gap-3 sm:grid-cols-2">{items.map(([label, complete, href]) => <li className="flex items-center justify-between gap-3 rounded-control border border-border-system p-3 text-sm" key={label}><span className="font-medium text-slate-800">{label}</span>{complete ? <span className="font-semibold text-emerald-700">Concluído</span> : <Link className="font-semibold text-brand-primary underline-offset-4 hover:underline" href={href}>Configurar</Link>}</li>)}</ul><p className="mt-4 text-sm text-slate-600">{status.canActivate ? "Os requisitos mínimos foram atendidos. A ativação libera a operação da clínica." : "Conclua os itens obrigatórios antes de ativar a clínica."}</p>{activationError && <p className="mt-3 rounded-control border border-red-200 bg-red-50 p-3 text-sm text-red-800" role="alert">{activationError}</p>}{status.canActivate && <Button className="mt-4" loading={activating} onClick={onActivate}>Ativar clínica</Button>}</Card>; }

function Field({ form, name, label, hint, placeholder, required, type = "text", autoComplete, className, trailing }: { form: ReturnType<typeof useForm<OnboardTenantRequest>>; name: keyof OnboardTenantRequest; label: string; hint?: string; placeholder?: string; required?: boolean; type?: string; autoComplete?: string; className?: string; trailing?: React.ReactNode }) { const error = form.formState.errors[name]?.message; return <div className={className}><FormField error={typeof error === "string" ? error : undefined} htmlFor={`onboarding-${name}`} hint={hint} label={label} required={required}><div className="relative"> <Input autoComplete={autoComplete} id={`onboarding-${name}`} placeholder={placeholder} type={type} {...form.register(name)} />{trailing && <span className="absolute inset-y-0 right-3 flex items-center">{trailing}</span>}</div></FormField></div>; }

function countCompleted(status: PlatformOnboardingStatus) { return [status.clinicConfigured, status.unitConfigured, status.specialtiesConfigured, status.professionalsConfigured, status.availabilityConfigured, status.clinicAdminConfigured, status.whatsAppConfigured, status.canActivate].filter(Boolean).length; }
function isStepComplete(status: PlatformOnboardingStatus, index: number) { return [status.clinicConfigured, status.unitConfigured, status.specialtiesConfigured, status.professionalsConfigured, status.availabilityConfigured, status.clinicAdminConfigured, status.whatsAppConfigured, status.canActivate][index] ?? false; }
function friendlyError(message: string) { if (/tenant slug is already in use|slug.*already exists|slug.*já existe/i.test(message)) return "Este identificador já está em uso. Escolha outro slug, por exemplo, clínica-2026."; if (/conflict|already exists|já existe/i.test(message)) return "Já existe uma clínica com esses dados."; if (/validation|invalid|obrigat|required/i.test(message)) return "Revise os campos informados e tente novamente."; return "Não foi possível concluir agora. Tente novamente."; }
