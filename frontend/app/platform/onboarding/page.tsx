"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ProtectedShell } from "@/components/protected-shell";
import { Button } from "@/components/ui/button";
import { FormActions, FormErrorSummary, FormField, FormSection, Input } from "@/components/ui/form";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card } from "@/components/ui/surfaces";
import { platformApi } from "@/lib/api/platform";
import type { OnboardTenantRequest } from "@/lib/api/types";
import { useApi, useAuth } from "@/providers/providers";

const schema = z.object({ tenantName: z.string().min(2), tenantSlug: z.string().regex(/^[a-z0-9-]+$/), clinicLegalName: z.string().min(2), clinicTradeName: z.string().min(2), clinicDocument: z.string().min(5), clinicEmail: z.string().email(), clinicPhone: z.string().min(8), timeZone: z.string().min(3), unitName: z.string().min(2), unitAddress: z.string().min(5), unitPhone: z.string().min(8), adminName: z.string().min(2), adminEmail: z.string().email(), temporaryPassword: z.string().min(12) });

export default function OnboardingPage() {
  const api = useApi(); const { user } = useAuth(); const client = useQueryClient(); const key = useRef(crypto.randomUUID());
  const form = useForm<OnboardTenantRequest>({ resolver: zodResolver(schema), defaultValues: { timeZone: "America/Recife" } });
  const mutation = useMutation({ mutationFn: (request: OnboardTenantRequest) => platformApi.onboard(api, request, key.current), onSuccess: () => client.invalidateQueries({ queryKey: ["platform"] }) });
  const fields = { tenantName: "Tenant", tenantSlug: "Slug", clinicLegalName: "Razão social", clinicTradeName: "Nome fantasia", clinicDocument: "Documento", clinicEmail: "E-mail da clínica", clinicPhone: "Telefone da clínica", timeZone: "Fuso horário", unitName: "Unidade", unitAddress: "Endereço da unidade", unitPhone: "Telefone da unidade", adminName: "Administrador", adminEmail: "E-mail do administrador", temporaryPassword: "Senha temporária" } as const;
  const input = (name: keyof typeof fields, hint?: string) => { const error = form.formState.errors[name]?.message; return <FormField error={typeof error === "string" ? error : undefined} htmlFor={`onboarding-${name}`} hint={hint} key={name} label={fields[name]} required><Input autoComplete={name === "adminEmail" ? "email" : undefined} id={`onboarding-${name}`} type={name === "temporaryPassword" ? "password" : name.includes("Email") ? "email" : "text"} {...form.register(name)} /></FormField>; };
  const errors = Object.values(form.formState.errors).map(error => error?.message).filter((message): message is string => typeof message === "string");
  const feedback = mutation.error instanceof Error ? mutation.error.message : mutation.error ? "Não foi possível concluir o onboarding." : "";
  if (user?.role !== "PlatformAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/platform/onboarding" title="Acesso negado" /></PageContainer></ProtectedShell>;
  return <ProtectedShell><PageContainer><PageHeader actions={<Button onClick={() => history.back()} variant="outline">Voltar</Button>} description="Crie tenant, clínica, unidade e administrador em uma única operação controlada." pathname="/platform/onboarding" title="Novo tenant" /><Card className="mt-6"><form className="grid gap-6" onSubmit={form.handleSubmit(request => mutation.mutate(request))}><FormErrorSummary errors={errors} /><FormSection description="Use um slug único, em minúsculas, para identificar o tenant." title="Tenant">{input("tenantName")}{input("tenantSlug", "Use letras minúsculas, números e hífens.")}</FormSection><FormSection description="Dados institucionais e de contato da clínica." title="Clínica">{input("clinicLegalName")}{input("clinicTradeName")}{input("clinicDocument")}{input("clinicEmail")}{input("clinicPhone")}{input("timeZone", "Exemplo: America/Recife")}</FormSection><FormSection description="Local de atendimento inicial que será criado." title="Unidade">{input("unitName")}{input("unitAddress")}{input("unitPhone")}</FormSection><FormSection description="A credencial temporária deve ser trocada após o primeiro acesso." title="Administrador">{input("adminName")}{input("adminEmail")}{input("temporaryPassword", "Mínimo de 12 caracteres.")}</FormSection><FormActions><Button loading={mutation.isPending} type="submit">Criar tenant</Button><Button onClick={() => history.back()} variant="outline">Cancelar</Button></FormActions><FormErrorSummary errors={feedback ? [feedback] : []} />{mutation.isSuccess && <p className="rounded-control border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-900" role="status">Onboarding concluído.</p>}</form></Card></PageContainer></ProtectedShell>;
}
