"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { patientsApi } from "@/lib/api/patients";
import type { Patient, PatientRequest } from "@/lib/api/types";
import { useApi } from "@/providers/providers";
import { Button } from "@/components/ui/button";
import { FormActions, FormErrorSummary, FormField, FormSection, Input, Select } from "@/components/ui/form";
import { Card } from "@/components/ui/surfaces";

export const patientSchema = z.object({ name: z.string().trim().min(2, "Informe o nome do paciente."), phone: z.string().trim().min(8, "Informe um telefone válido."), email: z.union([z.literal(""), z.string().trim().email("Informe um e-mail válido.")]), birthDate: z.string(), consentStatus: z.string().trim().min(1, "Informe o consentimento.") });
type PatientFormValues = z.infer<typeof patientSchema>;
const emptyValues: PatientFormValues = { name: "", phone: "", email: "", birthDate: "", consentStatus: "Granted" };

export function PatientForm({ patient, onComplete, onCancel }: { patient?: Patient; onComplete: () => void; onCancel: () => void }) {
  const api = useApi(); const queryClient = useQueryClient();
  const form = useForm<PatientFormValues>({ resolver: zodResolver(patientSchema), defaultValues: emptyValues });
  useEffect(() => { form.reset(patient ? { name: patient.name, phone: patient.phone, email: patient.email ?? "", birthDate: patient.birthDate ?? "", consentStatus: patient.consentStatus } : emptyValues); }, [form, patient]);
  const mutation = useMutation({ mutationFn: (values: PatientFormValues) => { const request: PatientRequest = { ...values, email: values.email || null, birthDate: values.birthDate || null }; return patient ? patientsApi.update(api, patient.id, request) : patientsApi.create(api, request); }, onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: ["patients"] }); onComplete(); } });
  const errors = Object.values(form.formState.errors).map(error => error?.message).filter((message): message is string => Boolean(message));

  return <Card className="mt-6"><form className="grid gap-6" onSubmit={form.handleSubmit(values => mutation.mutate(values))} aria-label={patient ? "Editar paciente" : "Novo paciente"}><FormErrorSummary errors={errors} /><FormSection description="Identifique o paciente para o atendimento e a agenda." title="Informações principais"><FormField error={form.formState.errors.name?.message} htmlFor="patient-name" label="Nome" required><Input id="patient-name" {...form.register("name")} /></FormField><FormField error={form.formState.errors.birthDate?.message} htmlFor="patient-birth-date" label="Data de nascimento"><Input id="patient-birth-date" type="date" {...form.register("birthDate")} /></FormField></FormSection><FormSection description="Esses dados são usados apenas pelos fluxos administrativos e de comunicação autorizados." title="Contato e consentimento"><FormField error={form.formState.errors.phone?.message} htmlFor="patient-phone" label="Telefone" required><Input id="patient-phone" inputMode="tel" {...form.register("phone")} /></FormField><FormField error={form.formState.errors.email?.message} htmlFor="patient-email" label="E-mail"><Input id="patient-email" type="email" {...form.register("email")} /></FormField><FormField error={form.formState.errors.consentStatus?.message} htmlFor="patient-consent" label="Consentimento" required><Select id="patient-consent" {...form.register("consentStatus")}><option value="Granted">Concedido</option><option value="Pending">Pendente</option><option value="Revoked">Revogado</option></Select></FormField></FormSection><FormActions><Button loading={mutation.isPending} type="submit">{patient ? "Salvar alterações" : "Cadastrar paciente"}</Button><Button onClick={onCancel} variant="outline">Cancelar</Button></FormActions>{mutation.error && <FormErrorSummary errors={[mutation.error instanceof Error ? mutation.error.message : "Não foi possível salvar o paciente."]} />}</form></Card>;
}
