"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ProtectedShell } from "@/components/protected-shell";
import { Button } from "@/components/ui/button";
import { FormActions, FormErrorSummary, FormField, FormSection, Input } from "@/components/ui/form";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card } from "@/components/ui/surfaces";
import type { OnboardTenantRequest } from "@/lib/api/types";
import { platformApi } from "@/lib/api/platform";
import { useApi, useAuth } from "@/providers/providers";

type ProvisioningForm = Omit<OnboardTenantRequest, "adminName" | "adminEmail" | "temporaryPassword"> & { adminName: string; adminEmail: string; temporaryPassword: string };

const schema = z.object({
  tenantName: z.string().min(2, "Informe o nome da clínica."), tenantSlug: z.string().regex(/^[a-z0-9-]+$/, "Use apenas letras minúsculas, números e hífens."),
  clinicLegalName: z.string().min(2, "Informe a razão social."), clinicTradeName: z.string().min(2, "Informe o nome de exibição."), clinicDocument: z.string().min(5, "Informe o documento."), clinicEmail: z.string().email("Informe um e-mail válido."), clinicPhone: z.string().min(8, "Informe um telefone válido."), timeZone: z.string().min(3, "Informe o fuso horário."),
  unitName: z.string().min(2, "Informe o nome da unidade."), unitAddress: z.string().min(5, "Informe o endereço."), unitPhone: z.string().min(8, "Informe um telefone válido."),
  adminName: z.string().min(2, "Informe o nome do administrador."), adminEmail: z.string().email("Informe um e-mail válido."), temporaryPassword: z.string().min(12, "Use pelo menos 12 caracteres na senha temporária."),
});

export default function OnboardingPage() {
  const api = useApi(); const { user } = useAuth(); const router = useRouter(); const client = useQueryClient(); const key = useRef(crypto.randomUUID());
  const form = useForm<ProvisioningForm>({ resolver: zodResolver(schema), defaultValues: { timeZone: "America/Recife" } });
  const mutation = useMutation({ mutationFn: (request: ProvisioningForm) => platformApi.onboard(api, request, key.current), onSuccess: () => { void client.invalidateQueries({ queryKey: ["platform"] }); router.push("/platform"); } });
  const errors = Object.values(form.formState.errors).map(error => error?.message).filter((message): message is string => typeof message === "string");
  if (user?.role !== "PlatformAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/platform/onboarding" title="Acesso negado" description="Esta área é exclusiva da administração da plataforma." /></PageContainer></ProtectedShell>;
  return <ProtectedShell><PageContainer><PageHeader actions={<Button onClick={() => router.push("/platform")} variant="outline">Voltar</Button>} description="Provisionamento inicial: clínica, unidade e o administrador que assumirá a operação." pathname="/platform/onboarding" title="Nova clínica" />
    <Card className="mx-auto mt-6 max-w-5xl"><form className="grid gap-6" onSubmit={form.handleSubmit(request => mutation.mutate(request))}><FormErrorSummary errors={errors} />
      <FormSection description="Identificação global do tenant e dados institucionais mínimos." title="1. Clínica"><Field form={form} name="tenantName" label="Nome da clínica" placeholder="Clínica Vida" required /><Field form={form} name="tenantSlug" label="Identificador" hint="Letras minúsculas, números e hífens; deve ser único." placeholder="clinica-vida" required /><Field form={form} name="clinicLegalName" label="Razão social" placeholder="Clínica Vida Serviços de Saúde LTDA" required /><Field form={form} name="clinicTradeName" label="Nome de exibição" placeholder="Clínica Vida" required /><Field form={form} name="clinicDocument" label="Documento" placeholder="00.000.000/0000-00" required /><Field form={form} name="clinicEmail" label="E-mail institucional" placeholder="contato@clinicavida.com.br" required type="email" /><Field form={form} name="clinicPhone" label="Telefone" placeholder="(81) 99999-9999" required /><Field form={form} name="timeZone" label="Fuso horário" placeholder="America/Recife" required /></FormSection>
      <FormSection description="A primeira unidade criada junto com a clínica." title="2. Unidade inicial"><Field form={form} name="unitName" label="Nome da unidade" placeholder="Unidade principal" required /><Field className="md:col-span-2" form={form} name="unitAddress" label="Endereço" placeholder="Av. Exemplo, 100 — Recife, PE" required /><Field form={form} name="unitPhone" label="Telefone da unidade" placeholder="(81) 99999-9999" required /></FormSection>
      <FormSection description="Este acesso será sempre criado com o papel fixo ClinicAdmin." title="3. Administrador da clínica"><Field form={form} name="adminName" label="Nome completo" placeholder="Maria da Silva" required /><Field form={form} name="adminEmail" label="E-mail de acesso" placeholder="admin@clinicavida.com.br" required type="email" /><Field form={form} name="temporaryPassword" label="Senha temporária" hint="A senha será armazenada somente como hash. Oriente o administrador a trocá-la no primeiro acesso." placeholder="Mínimo de 12 caracteres" required type="password" /></FormSection>
      <FormErrorSummary errors={mutation.error instanceof Error ? [friendlyError(mutation.error.message)] : []} /><FormActions><Button loading={mutation.isPending} type="submit">{mutation.isPending ? "Provisionando..." : "Provisionar clínica"}</Button><Button onClick={() => router.push("/platform")} type="button" variant="outline">Cancelar</Button></FormActions>
    </form></Card>
  </PageContainer></ProtectedShell>;
}

function Field({ form, name, label, hint, placeholder, required, type = "text", className }: { form: ReturnType<typeof useForm<ProvisioningForm>>; name: keyof ProvisioningForm; label: string; hint?: string; placeholder?: string; required?: boolean; type?: string; className?: string }) { const error = form.formState.errors[name]?.message; return <div className={className}><FormField error={typeof error === "string" ? error : undefined} htmlFor={`onboarding-${name}`} hint={hint} label={label} required={required}><Input id={`onboarding-${name}`} placeholder={placeholder} type={type} {...form.register(name)} /></FormField></div>; }
function friendlyError(message: string) { if (/slug.*(use|exists)/i.test(message)) return "Este identificador já está em uso. Escolha outro slug."; if (/email.*(use|exists)/i.test(message)) return "Este e-mail já está associado a outro usuário."; if (/required|invalid|validation/i.test(message)) return "Revise os campos informados e tente novamente."; return "Não foi possível provisionar a clínica agora."; }
