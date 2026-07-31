"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { RequestFeedback } from "@/components/request-feedback";
import { patientsApi } from "@/lib/api/patients";
import type { Patient, PatientRequest } from "@/lib/api/types";
import { useApi } from "@/providers/providers";

export const patientSchema = z.object({
  name: z.string().trim().min(2, "Informe o nome do paciente."),
  phone: z.string().trim().min(8, "Informe um telefone válido."),
  email: z.union([z.literal(""), z.string().trim().email("Informe um e-mail válido.")]),
  birthDate: z.string(),
  consentStatus: z.string().trim().min(1, "Informe o consentimento."),
});
type PatientFormValues = z.infer<typeof patientSchema>;
const emptyValues: PatientFormValues = { name: "", phone: "", email: "", birthDate: "", consentStatus: "Granted" };

export function PatientForm({ patient, onComplete, onCancel }: { patient?: Patient; onComplete: () => void; onCancel: () => void }) {
  const api = useApi();
  const queryClient = useQueryClient();
  const form = useForm<PatientFormValues>({ resolver: zodResolver(patientSchema), defaultValues: emptyValues });

  useEffect(() => {
    form.reset(patient ? { name: patient.name, phone: patient.phone, email: patient.email ?? "", birthDate: patient.birthDate ?? "", consentStatus: patient.consentStatus } : emptyValues);
  }, [form, patient]);

  const mutation = useMutation({
    mutationFn: (values: PatientFormValues) => {
      const request: PatientRequest = { ...values, email: values.email || null, birthDate: values.birthDate || null };
      return patient ? patientsApi.update(api, patient.id, request) : patientsApi.create(api, request);
    },
    onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: ["patients"] }); onComplete(); },
  });

  return <form className="mt-5 grid gap-4 rounded border bg-slate-50 p-4 md:grid-cols-2" onSubmit={form.handleSubmit(values => mutation.mutate(values))} aria-label={patient ? "Editar paciente" : "Novo paciente"}>
    <Field label="Nome" error={form.formState.errors.name?.message}><input {...form.register("name")} /></Field>
    <Field label="Telefone" error={form.formState.errors.phone?.message}><input inputMode="tel" {...form.register("phone")} /></Field>
    <Field label="E-mail" error={form.formState.errors.email?.message}><input type="email" {...form.register("email")} /></Field>
    <Field label="Data de nascimento"><input type="date" {...form.register("birthDate")} /></Field>
    <Field label="Consentimento" error={form.formState.errors.consentStatus?.message}><select {...form.register("consentStatus")}><option value="Granted">Concedido</option><option value="Pending">Pendente</option><option value="Revoked">Revogado</option></select></Field>
    <div className="flex items-end gap-3"><button className="rounded bg-blue-700 px-4 py-2 text-white disabled:opacity-50" disabled={mutation.isPending}>{mutation.isPending ? "Salvando..." : "Salvar"}</button><button className="rounded border px-4 py-2" type="button" onClick={onCancel}>Cancelar</button></div>
    <div className="md:col-span-2"><RequestFeedback error={mutation.error} /></div>
  </form>;
}

function Field({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return <label className="grid gap-1 text-sm font-medium">{label}<span className="font-normal [&>input]:w-full [&>input]:rounded [&>input]:border [&>input]:p-2 [&>select]:w-full [&>select]:rounded [&>select]:border [&>select]:p-2">{children}</span>{error && <span className="text-red-700">{error}</span>}</label>;
}
