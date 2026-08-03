"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { RequestFeedback } from "@/components/request-feedback";
import { ProtectedShell } from "@/components/protected-shell";
import { platformApi } from "@/lib/api/platform";
import type { OnboardTenantRequest } from "@/lib/api/types";
import { useApi, useAuth } from "@/providers/providers";

const schema = z.object({ tenantName: z.string().min(2), tenantSlug: z.string().regex(/^[a-z0-9-]+$/), clinicLegalName: z.string().min(2), clinicTradeName: z.string().min(2), clinicDocument: z.string().min(5), clinicEmail: z.string().email(), clinicPhone: z.string().min(8), timeZone: z.string().min(3), unitName: z.string().min(2), unitAddress: z.string().min(5), unitPhone: z.string().min(8), adminName: z.string().min(2), adminEmail: z.string().email(), temporaryPassword: z.string().min(12) });

export default function OnboardingPage() {
  const api = useApi(); const { user } = useAuth(); const client = useQueryClient(); const key = useRef(crypto.randomUUID());
  const form = useForm<OnboardTenantRequest>({ resolver: zodResolver(schema), defaultValues: { timeZone: "America/Recife" } });
  const mutation = useMutation({ mutationFn: (request: OnboardTenantRequest) => platformApi.onboard(api, request, key.current), onSuccess: () => client.invalidateQueries({ queryKey: ["platform"] }) });
  if (user?.role !== "PlatformAdmin") return <ProtectedShell><h1 className="text-2xl font-bold">Acesso negado</h1></ProtectedShell>;
  return <ProtectedShell><div className="flex justify-between"><h1 className="text-2xl font-bold">Novo tenant</h1><Link className="underline" href="/platform">Voltar</Link></div><p className="mt-2 text-slate-600">A operação cria tenant, clínica, unidade, administrador e integração desabilitada em uma única transação.</p><form className="mt-6 grid gap-3 rounded bg-white p-5 shadow md:grid-cols-2" onSubmit={form.handleSubmit(request => mutation.mutate(request))}>{Object.entries({ tenantName: "Tenant", tenantSlug: "Slug", clinicLegalName: "Razão social", clinicTradeName: "Nome fantasia", clinicDocument: "Documento", clinicEmail: "E-mail clínica", clinicPhone: "Telefone clínica", timeZone: "Timezone", unitName: "Unidade", unitAddress: "Endereço", unitPhone: "Telefone unidade", adminName: "Administrador", adminEmail: "E-mail administrador", temporaryPassword: "Senha temporária" }).map(([name, label]) => { const error = form.formState.errors[name as keyof OnboardTenantRequest]?.message; return <label className="grid gap-1 text-sm font-medium" key={name}>{String(label)}<input className="rounded border p-2" type={name.includes("Password") ? "password" : name.includes("Email") ? "email" : "text"} {...form.register(name as keyof OnboardTenantRequest)} />{typeof error === "string" && <span className="text-red-700">{error}</span>}</label>; })}<div className="md:col-span-2"><button className="rounded bg-blue-700 px-4 py-2 text-white" disabled={mutation.isPending}>{mutation.isPending ? "Criando..." : "Criar tenant"}</button><RequestFeedback error={mutation.error} />{mutation.isSuccess && <p className="mt-3 text-green-700" role="status">Onboarding concluído.</p>}</div></form></ProtectedShell>;
}
