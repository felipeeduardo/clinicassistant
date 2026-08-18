"use client";

import Link from "next/link";
import { useSearchParams, useRouter } from "next/navigation";
import { Suspense, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/surfaces";
import { FormField, Input } from "@/components/ui/form";

const schema = z.object({ password: z.string().min(12, "Use pelo menos 12 caracteres.").regex(/[A-Z]/, "Inclua uma letra maiúscula.").regex(/[a-z]/, "Inclua uma letra minúscula.").regex(/[0-9]/, "Inclua um número.").regex(/[^A-Za-z0-9]/, "Inclua um símbolo.") });
type Form = z.infer<typeof schema>;
function ResetPasswordForm() {
  const token = useSearchParams().get("token") ?? ""; const router = useRouter(); const [show, setShow] = useState(false); const [done, setDone] = useState(false);
  const { register, handleSubmit, formState: { errors, isSubmitting }, setError } = useForm<Form>({ resolver: zodResolver(schema) });
  const submit = async (data: Form) => { try { const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080"}/api/auth/reset-password`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ token, newPassword: data.password }) }); if (!response.ok) { const body = await response.json().catch(() => ({})); throw new Error(typeof body.title === "string" ? body.title : "Link inválido ou expirado."); } setDone(true); setTimeout(() => router.replace("/login"), 1200); } catch (error) { setError("root", { message: error instanceof Error ? error.message : "Não foi possível redefinir a senha." }); } };
  return <main className="grid min-h-screen place-items-center bg-slate-50 px-5"><Card className="w-full max-w-md p-8"><Link href="/login" className="text-sm text-brand-700">← Voltar para o login</Link><h1 className="mt-6 text-3xl font-semibold text-slate-950">Redefinir senha</h1><p className="mt-3 text-sm leading-6 text-slate-600">A nova senha deve ter 12 caracteres, maiúscula, minúscula, número e símbolo.</p>{done ? <p role="status" className="mt-6 rounded-control border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-900">Senha atualizada. Redirecionando para o login...</p> : <form className="mt-6 grid gap-5" onSubmit={handleSubmit(submit)} noValidate><FormField error={errors.password?.message} htmlFor="password" label="Nova senha" required><div className="relative"><Input id="password" type={show ? "text" : "password"} autoComplete="new-password" className="pr-12" {...register("password")} /><button type="button" className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-slate-500" onClick={() => setShow(value => !value)}>{show ? "Ocultar" : "Mostrar"}</button></div></FormField>{errors.root?.message && <p role="alert" className="text-sm text-red-700">{errors.root.message}</p>}<Button loading={isSubmitting} type="submit">{isSubmitting ? "Salvando..." : "Salvar nova senha"}</Button></form>}</Card></main>;
}

export default function ResetPasswordPage() { return <Suspense fallback={<main className="grid min-h-screen place-items-center bg-slate-50"><p>Carregando...</p></main>}><ResetPasswordForm /></Suspense>; }
