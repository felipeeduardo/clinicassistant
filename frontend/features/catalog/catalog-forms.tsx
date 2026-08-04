"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { FormActions, FormErrorSummary, FormField, FormSection, Input, Select, Textarea } from "@/components/ui/form";
import { Card } from "@/components/ui/surfaces";
import { catalogApi } from "@/lib/api/catalog";
import type { Clinic, ClinicRequest, Professional, ProfessionalRequest, Specialty, SpecialtyRequest, Unit, UnitRequest } from "@/lib/api/types";
import { useApi } from "@/providers/providers";

const text = (label: string) => z.string().trim().min(1, `Informe ${label}.`);
const clinicSchema = z.object({ legalName: text("a razão social"), tradeName: text("o nome fantasia"), document: text("o documento"), email: z.string().email("Informe um e-mail válido."), phone: text("o telefone"), timeZone: text("o fuso horário") });
const unitSchema = z.object({ name: text("o nome"), address: text("o endereço"), phone: text("o telefone") });
const specialtySchema = z.object({ name: text("o nome"), description: z.string() });
const professionalSchema = z.object({ clinicUnitId: text("a unidade"), name: text("o nome"), email: z.string().email("Informe um e-mail válido."), phone: text("o telefone"), registrationNumber: text("o registro profissional"), specialtyIds: z.array(z.string()).min(1, "Selecione ao menos uma especialidade.") });

export function ClinicForm({ clinic, onComplete }: { clinic: Clinic; onComplete: () => void }) {
  const api = useApi(); const client = useQueryClient(); const form = useForm<ClinicRequest>({ resolver: zodResolver(clinicSchema), defaultValues: clinic });
  const mutation = useMutation({ mutationFn: (request: ClinicRequest) => catalogApi.updateClinic(api, request), onSuccess: async () => { await client.invalidateQueries({ queryKey: ["clinic"] }); onComplete(); } });
  return <ManagedForm form={form as unknown as AnyForm} mutation={mutation} onCancel={onComplete}><FormSection description="Dados institucionais exibidos nos fluxos administrativos." title="Identificação da clínica"><TextField form={form as unknown as AnyForm} name="legalName" label="Razão social" /><TextField form={form as unknown as AnyForm} name="tradeName" label="Nome fantasia" /><TextField form={form as unknown as AnyForm} name="document" label="Documento" /></FormSection><FormSection description="Informações usadas para comunicação e agenda." title="Contato e localização"><TextField form={form as unknown as AnyForm} name="email" label="E-mail" type="email" /><TextField form={form as unknown as AnyForm} name="phone" label="Telefone" /><TextField form={form as unknown as AnyForm} name="timeZone" label="Fuso horário" hint="Exemplo: America/Recife" /></FormSection></ManagedForm>;
}

export function UnitForm({ unit, onComplete, onCancel }: { unit?: Unit; onComplete: () => void; onCancel: () => void }) {
  const api = useApi(); const client = useQueryClient(); const form = useForm<UnitRequest>({ resolver: zodResolver(unitSchema), defaultValues: unit ?? { name: "", address: "", phone: "" } });
  useEffect(() => form.reset(unit ?? { name: "", address: "", phone: "" }), [form, unit]);
  const mutation = useMutation({ mutationFn: (request: UnitRequest) => unit ? catalogApi.updateUnit(api, unit.id, request) : catalogApi.createUnit(api, request), onSuccess: async () => { await client.invalidateQueries({ queryKey: ["units"] }); onComplete(); } });
  return <ManagedForm form={form as unknown as AnyForm} mutation={mutation} onCancel={onCancel}><FormSection description="Defina a identificação e os canais de contato da unidade." title="Dados da unidade"><TextField form={form as unknown as AnyForm} name="name" label="Nome" /><TextField form={form as unknown as AnyForm} name="address" label="Endereço" /><TextField form={form as unknown as AnyForm} name="phone" label="Telefone" /></FormSection></ManagedForm>;
}

export function SpecialtyForm({ specialty, onComplete, onCancel }: { specialty?: Specialty; onComplete: () => void; onCancel: () => void }) {
  const api = useApi(); const client = useQueryClient(); const form = useForm<{ name: string; description: string }>({ resolver: zodResolver(specialtySchema), defaultValues: { name: specialty?.name ?? "", description: specialty?.description ?? "" } });
  useEffect(() => form.reset({ name: specialty?.name ?? "", description: specialty?.description ?? "" }), [form, specialty]);
  const mutation = useMutation({ mutationFn: (request: SpecialtyRequest) => specialty ? catalogApi.updateSpecialty(api, specialty.id, request) : catalogApi.createSpecialty(api, request), onSuccess: async () => { await client.invalidateQueries({ queryKey: ["specialties"] }); onComplete(); } });
  return <ManagedForm form={form as unknown as AnyForm} mutation={mutation} onCancel={onCancel}><FormSection description="Use uma descrição clara para facilitar a escolha na agenda." title="Especialidade"><TextField form={form as unknown as AnyForm} name="name" label="Nome" /><TextAreaField form={form as unknown as AnyForm} name="description" label="Descrição" /></FormSection></ManagedForm>;
}

export function ProfessionalForm({ professional, units, specialties, onComplete, onCancel }: { professional?: Professional; units: Unit[]; specialties: Specialty[]; onComplete: () => void; onCancel: () => void }) {
  const api = useApi(); const client = useQueryClient(); const form = useForm<ProfessionalRequest>({ resolver: zodResolver(professionalSchema), defaultValues: professional ?? { clinicUnitId: "", name: "", email: "", phone: "", registrationNumber: "", specialtyIds: [] } });
  useEffect(() => form.reset(professional ?? { clinicUnitId: "", name: "", email: "", phone: "", registrationNumber: "", specialtyIds: [] }), [form, professional]);
  const mutation = useMutation({ mutationFn: (request: ProfessionalRequest) => professional ? catalogApi.updateProfessional(api, professional.id, request) : catalogApi.createProfessional(api, request), onSuccess: async () => { await client.invalidateQueries({ queryKey: ["professionals"] }); onComplete(); } });
  return <ManagedForm form={form as unknown as AnyForm} mutation={mutation} onCancel={onCancel}><FormSection description="Informe os dados profissionais usados na agenda." title="Identificação"><TextField form={form as unknown as AnyForm} name="name" label="Nome" /><TextField form={form as unknown as AnyForm} name="registrationNumber" label="Registro profissional" /><SelectField form={form as unknown as AnyForm} name="clinicUnitId" label="Unidade" values={units.map(unit => [unit.id, unit.name])} /></FormSection><FormSection description="Canais disponíveis para os fluxos administrativos." title="Contato"><TextField form={form as unknown as AnyForm} name="email" label="E-mail" type="email" /><TextField form={form as unknown as AnyForm} name="phone" label="Telefone" /></FormSection><FormSection description="Selecione ao menos uma especialidade para permitir o agendamento." title="Especialidades"><fieldset className="grid gap-2 text-sm"><legend className="font-medium text-slate-800">Atuação</legend>{specialties.map(specialty => <label className="flex items-center gap-2 rounded-control border border-slate-200 bg-white px-3 py-2" key={specialty.id}><input className="size-4 accent-brand-600" type="checkbox" value={specialty.id} {...form.register("specialtyIds")} />{specialty.name}</label>)}{form.formState.errors.specialtyIds && <span className="text-sm text-red-700" role="alert">{form.formState.errors.specialtyIds.message}</span>}</fieldset></FormSection></ManagedForm>;
}

type AnyForm = ReturnType<typeof useForm<Record<string, unknown>>>;
function ManagedForm({ form, mutation, onCancel, children }: { form: AnyForm; mutation: { mutate: (value: never) => void; isPending: boolean; error: unknown }; onCancel: () => void; children: React.ReactNode }) {
  const errors = Object.values(form.formState.errors).map(error => error?.message).filter((message): message is string => typeof message === "string");
  const feedback = mutation.error instanceof Error ? mutation.error.message : mutation.error ? "Não foi possível salvar as alterações." : "";
  return <Card className="mt-5"><form className="grid gap-6" onSubmit={form.handleSubmit(values => mutation.mutate(values as never))}><FormErrorSummary errors={errors} />{children}<FormActions><Button loading={mutation.isPending} type="submit">Salvar alterações</Button><Button onClick={onCancel} variant="outline">Cancelar</Button></FormActions><FormErrorSummary errors={feedback ? [feedback] : []} /></form></Card>;
}
function TextField({ form, name, label, type = "text", hint }: { form: AnyForm; name: string; label: string; type?: string; hint?: string }) { const error = form.formState.errors[name]?.message; return <FormField error={typeof error === "string" ? error : undefined} htmlFor={`catalog-${name}`} hint={hint} label={label} required><Input id={`catalog-${name}`} type={type} {...form.register(name)} /></FormField>; }
function TextAreaField({ form, name, label }: { form: AnyForm; name: string; label: string }) { const error = form.formState.errors[name]?.message; return <FormField error={typeof error === "string" ? error : undefined} htmlFor={`catalog-${name}`} label={label}><Textarea id={`catalog-${name}`} {...form.register(name)} /></FormField>; }
function SelectField({ form, name, label, values }: { form: AnyForm; name: string; label: string; values: string[][] }) { const error = form.formState.errors[name]?.message; return <FormField error={typeof error === "string" ? error : undefined} htmlFor={`catalog-${name}`} label={label} required><Select id={`catalog-${name}`} {...form.register(name)}><option value="">Selecione</option>{values.map(([value, text]) => <option key={value} value={value}>{text}</option>)}</Select></FormField>; }
