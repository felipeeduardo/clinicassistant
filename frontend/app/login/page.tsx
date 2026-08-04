"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { FormErrorSummary, FormField, Input } from "@/components/ui/form";
import { Card } from "@/components/ui/surfaces";
import { useAuth } from "@/providers/providers";

const schema = z.object({ email: z.string().email("Informe um e-mail válido."), password: z.string().min(1, "Informe sua senha.") });
type Form = z.infer<typeof schema>;

export default function LoginPage() {
  const router = useRouter(); const { login } = useAuth();
  const { register, handleSubmit, formState: { errors, isSubmitting }, setError } = useForm<Form>({ resolver: zodResolver(schema) });
  const submit = async (data: Form) => { try { await login(data.email, data.password); router.replace("/dashboard"); } catch { setError("root", { message: "Não foi possível entrar. Verifique suas credenciais." }); } };
  const messages = [errors.email?.message, errors.password?.message, errors.root?.message].filter((value): value is string => Boolean(value));

  return <main className="grid min-h-screen bg-slate-100 p-4 lg:grid-cols-[1.1fr_0.9fr] lg:p-8">
    <section className="hidden rounded-panel bg-slate-950 p-10 text-white lg:flex lg:flex-col lg:justify-between"><div className="grid size-11 place-items-center rounded-control bg-brand-600 text-sm font-bold">CA</div><div><p className="text-sm font-medium text-brand-100">Operação clínica conectada</p><h1 className="mt-3 max-w-lg text-4xl font-semibold tracking-tight">Atendimento, agenda e conversas em um único lugar.</h1><p className="mt-5 max-w-md text-slate-300">Acesse o ambiente da sua clínica para acompanhar a operação do dia com segurança.</p></div><p className="text-sm text-slate-400">Clinic Assistant</p></section>
    <section className="grid place-items-center py-6 lg:py-0"><Card className="w-full max-w-md p-6 sm:p-8"><div className="mb-7"><div className="mb-5 grid size-10 place-items-center rounded-control bg-brand-600 text-sm font-bold text-white lg:hidden">CA</div><p className="text-sm font-medium text-brand-700">Bem-vindo de volta</p><h2 className="mt-1 text-2xl font-semibold tracking-tight text-slate-950">Entre na sua conta</h2><p className="mt-2 text-sm text-slate-600">Use suas credenciais para continuar.</p></div><form className="grid gap-5" onSubmit={handleSubmit(submit)} aria-label="Entrar"><FormErrorSummary errors={messages} /><FormField error={errors.email?.message} htmlFor="email" label="E-mail" required><Input autoComplete="email" id="email" type="email" {...register("email")} /></FormField><FormField error={errors.password?.message} htmlFor="password" label="Senha" required><Input autoComplete="current-password" id="password" type="password" {...register("password")} /></FormField><Button className="w-full" loading={isSubmitting} type="submit">Entrar</Button></form></Card></section>
  </main>;
}
