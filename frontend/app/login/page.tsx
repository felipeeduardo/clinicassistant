"use client";

import Link from "next/link";
import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { FormErrorSummary, FormField, Input } from "@/components/ui/form";
import { Card } from "@/components/ui/surfaces";
import { Icon } from "@/components/ui/icon";
import { useAuth } from "@/providers/providers";
import { BrandLockup } from "@/components/brand/brand-lockup";

const schema = z.object({ email: z.string().email("Informe um e-mail válido."), password: z.string().min(1, "Informe sua senha.") });
type Form = z.infer<typeof schema>;

const trustPoints = ["Agenda centralizada", "Atendimento humano quando necessário", "Histórico operacional"];

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();
  const [showPassword, setShowPassword] = useState(false);
  const { register, handleSubmit, formState: { errors, isSubmitting }, setError } = useForm<Form>({ resolver: zodResolver(schema) });
  const submit = async (data: Form) => {
    try {
      await login(data.email, data.password);
      router.replace("/dashboard");
    } catch {
      setError("root", { message: "Não foi possível entrar. Verifique seu e-mail e senha." });
    }
  };
  const fieldMessages = [errors.email?.message, errors.password?.message].filter((value): value is string => Boolean(value));

  return <main className="login-page">
    <section className="login-brand-panel" aria-labelledby="login-brand-title">
      <div className="login-brand-glow" aria-hidden="true" />
      <div className="relative z-10 flex h-full flex-col">
        <Link href="/" aria-label="Voltar para a página inicial"><BrandLockup compact /></Link>
        <div className="my-auto py-14">
          <p className="login-eyebrow">Operação clínica conectada</p>
          <h1 className="mt-4 max-w-xl text-4xl font-semibold tracking-tight sm:text-5xl" id="login-brand-title">Atendimento, agenda e conversas conectados à operação da clínica.</h1>
          <p className="mt-6 max-w-lg text-lg leading-8 text-slate-300">Entre para acompanhar a agenda, as conversas e o que precisa da atenção da sua equipe.</p>
          <ul className="mt-8 grid gap-3" aria-label="Recursos da operação"><li className="login-trust-point"><Icon name="calendar" />Agenda centralizada</li><li className="login-trust-point"><Icon name="userCheck" />Atendimento humano quando necessário</li><li className="login-trust-point"><Icon name="history" />Histórico operacional</li></ul>
          <div className="login-mini-preview" aria-label="Resumo visual da operação"><div className="flex items-center justify-between"><span className="font-semibold text-white">Hoje</span><span className="text-xs text-slate-400">Agenda</span></div><div className="mt-4 grid gap-2"><p><span>09:00</span><strong>Consulta confirmada</strong></p><p><span>10:30</span><strong>Atendimento em andamento</strong></p><p><span>14:00</span><strong>Recepção acompanha</strong></p></div></div>
        </div>
        <p className="text-sm text-slate-400">© {new Date().getFullYear()} IA Recepção</p>
      </div>
    </section>
    <section className="login-form-panel"><Card className="login-card"><div className="mb-7"><p className="login-eyebrow-light">Bem-vindo de volta</p><h2 className="mt-2 text-3xl font-semibold tracking-tight text-slate-950">Entre na sua conta</h2><p className="mt-3 text-sm leading-6 text-slate-600">Entre para acompanhar a operação da sua clínica.</p></div><form className="grid gap-5" onSubmit={handleSubmit(submit)} aria-label="Entrar" noValidate><FormErrorSummary errors={fieldMessages} />{errors.root?.message && <p className="rounded-control border border-red-200 bg-red-50 p-3 text-sm text-red-900" role="alert">{errors.root.message}</p>}<FormField error={errors.email?.message} htmlFor="email" label="E-mail" required><Input autoComplete="email" id="email" placeholder="seuemail@clinica.com.br" type="email" {...register("email")} /></FormField><FormField error={errors.password?.message} htmlFor="password" label="Senha" required><div className="relative"><Input aria-describedby={errors.password ? "password-error" : undefined} aria-invalid={errors.password ? true : undefined} autoComplete="current-password" className="pr-12" id="password" placeholder="••••••••" type={showPassword ? "text" : "password"} {...register("password")} /><button aria-label={showPassword ? "Ocultar senha" : "Mostrar senha"} className="absolute right-1 top-1/2 grid size-8 -translate-y-1/2 place-items-center rounded text-slate-500 hover:bg-slate-100 hover:text-slate-800 focus-visible:outline-brand-600" onClick={() => setShowPassword(value => !value)} type="button"><Icon name={showPassword ? "eyeOff" : "eye"} /></button></div></FormField><Link className="text-right text-sm text-brand-700" href="/esqueci-minha-senha">Esqueci minha senha</Link><Button className="mt-1 w-full" loading={isSubmitting} type="submit">{isSubmitting ? "Entrando..." : "Entrar"}</Button><p className="text-center text-xs text-slate-500">Seu acesso é restrito ao ambiente autorizado da sua clínica.</p></form><Link className="login-back-link" href="/">← Voltar ao site</Link></Card></section>
  </main>;
}
