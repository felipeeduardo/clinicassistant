"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { RequestFeedback } from "@/components/request-feedback";
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
  return <ManagedForm form={form as unknown as AnyForm} mutation={mutation} onCancel={onComplete}><TextField form={form as unknown as AnyForm} name="legalName" label="Razão social" /><TextField form={form as unknown as AnyForm} name="tradeName" label="Nome fantasia" /><TextField form={form as unknown as AnyForm} name="document" label="Documento" /><TextField form={form as unknown as AnyForm} name="email" label="E-mail" type="email" /><TextField form={form as unknown as AnyForm} name="phone" label="Telefone" /><TextField form={form as unknown as AnyForm} name="timeZone" label="Fuso horário" /></ManagedForm>;
}

export function UnitForm({ unit, onComplete, onCancel }: { unit?: Unit; onComplete: () => void; onCancel: () => void }) {
  const api = useApi(); const client = useQueryClient(); const form = useForm<UnitRequest>({ resolver: zodResolver(unitSchema), defaultValues: unit ?? { name: "", address: "", phone: "" } });
  useEffect(() => form.reset(unit ?? { name: "", address: "", phone: "" }), [form, unit]);
  const mutation = useMutation({ mutationFn: (request: UnitRequest) => unit ? catalogApi.updateUnit(api, unit.id, request) : catalogApi.createUnit(api, request), onSuccess: async () => { await client.invalidateQueries({ queryKey: ["units"] }); onComplete(); } });
  return <ManagedForm form={form as unknown as AnyForm} mutation={mutation} onCancel={onCancel}><TextField form={form as unknown as AnyForm} name="name" label="Nome" /><TextField form={form as unknown as AnyForm} name="address" label="Endereço" /><TextField form={form as unknown as AnyForm} name="phone" label="Telefone" /></ManagedForm>;
}

export function SpecialtyForm({ specialty, onComplete, onCancel }: { specialty?: Specialty; onComplete: () => void; onCancel: () => void }) {
  const api = useApi(); const client = useQueryClient(); const form = useForm<{ name: string; description: string }>({ resolver: zodResolver(specialtySchema), defaultValues: { name: specialty?.name ?? "", description: specialty?.description ?? "" } });
  useEffect(() => form.reset({ name: specialty?.name ?? "", description: specialty?.description ?? "" }), [form, specialty]);
  const mutation = useMutation({ mutationFn: (request: SpecialtyRequest) => specialty ? catalogApi.updateSpecialty(api, specialty.id, request) : catalogApi.createSpecialty(api, request), onSuccess: async () => { await client.invalidateQueries({ queryKey: ["specialties"] }); onComplete(); } });
  return <ManagedForm form={form as unknown as AnyForm} mutation={mutation} onCancel={onCancel}><TextField form={form as unknown as AnyForm} name="name" label="Nome" /><TextField form={form as unknown as AnyForm} name="description" label="Descrição" /></ManagedForm>;
}

export function ProfessionalForm({ professional, units, specialties, onComplete, onCancel }: { professional?: Professional; units: Unit[]; specialties: Specialty[]; onComplete: () => void; onCancel: () => void }) {
  const api = useApi(); const client = useQueryClient(); const form = useForm<ProfessionalRequest>({ resolver: zodResolver(professionalSchema), defaultValues: professional ?? { clinicUnitId: "", name: "", email: "", phone: "", registrationNumber: "", specialtyIds: [] } });
  useEffect(() => form.reset(professional ?? { clinicUnitId: "", name: "", email: "", phone: "", registrationNumber: "", specialtyIds: [] }), [form, professional]);
  const mutation = useMutation({ mutationFn: (request: ProfessionalRequest) => professional ? catalogApi.updateProfessional(api, professional.id, request) : catalogApi.createProfessional(api, request), onSuccess: async () => { await client.invalidateQueries({ queryKey: ["professionals"] }); onComplete(); } });
  return <ManagedForm form={form as unknown as AnyForm} mutation={mutation} onCancel={onCancel}><TextField form={form as unknown as AnyForm} name="name" label="Nome" /><TextField form={form as unknown as AnyForm} name="email" label="E-mail" type="email" /><TextField form={form as unknown as AnyForm} name="phone" label="Telefone" /><TextField form={form as unknown as AnyForm} name="registrationNumber" label="Registro profissional" /><SelectField form={form as unknown as AnyForm} name="clinicUnitId" label="Unidade" values={units.map(unit => [unit.id, unit.name])} /><fieldset className="grid gap-1 text-sm font-medium"><legend>Especialidades</legend>{specialties.map(specialty => <label className="flex gap-2 font-normal" key={specialty.id}><input type="checkbox" value={specialty.id} {...form.register("specialtyIds")} />{specialty.name}</label>)}{form.formState.errors.specialtyIds && <span className="text-red-700">{form.formState.errors.specialtyIds.message}</span>}</fieldset></ManagedForm>;
}

type AnyForm = ReturnType<typeof useForm<Record<string, unknown>>>;
function ManagedForm({ form, mutation, onCancel, children }: { form: AnyForm; mutation: { mutate: (value: never) => void; isPending: boolean; error: unknown }; onCancel: () => void; children: React.ReactNode }) {
  return <form className="mt-5 grid gap-4 rounded border bg-slate-50 p-4 md:grid-cols-2" onSubmit={form.handleSubmit(values => mutation.mutate(values as never))}><div className="contents">{children}</div><div className="flex items-end gap-3"><button className="rounded bg-blue-700 px-4 py-2 text-white disabled:opacity-50" disabled={mutation.isPending}>{mutation.isPending ? "Salvando..." : "Salvar"}</button><button className="rounded border px-4 py-2" type="button" onClick={onCancel}>Cancelar</button></div><div className="md:col-span-2"><RequestFeedback error={mutation.error} /></div></form>;
}
function TextField({ form, name, label, type = "text" }: { form: AnyForm; name: string; label: string; type?: string }) { return <label className="grid gap-1 text-sm font-medium">{label}<input className="rounded border p-2 font-normal" type={type} {...form.register(name)} />{form.formState.errors[name]?.message as string | undefined && <span className="text-red-700">{form.formState.errors[name]?.message as string}</span>}</label>; }
function SelectField({ form, name, label, values }: { form: AnyForm; name: string; label: string; values: string[][] }) { return <label className="grid gap-1 text-sm font-medium">{label}<select className="rounded border p-2 font-normal" {...form.register(name)}><option value="">Selecione</option>{values.map(([value, text]) => <option key={value} value={value}>{text}</option>)}</select>{form.formState.errors[name]?.message as string | undefined && <span className="text-red-700">{form.formState.errors[name]?.message as string}</span>}</label>; }
