"use client";

import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/surfaces";
import { FormField, Input } from "@/components/ui/form";

const schema = z.object({ email: z.string().email("Informe um e-mail válido.") });
type Form = z.infer<typeof schema>;
export default function ForgotPasswordPage() {
  const [sent, setSent] = useState(false);
  const { register, handleSubmit, formState: { errors, isSubmitting }, setError } = useForm<Form>({ resolver: zodResolver(schema) });
  const submit = async (data: Form) => { try { const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080"}/api/auth/forgot-password`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(data) }); if (!response.ok) throw new Error(); setSent(true); } catch { setError("root", { message: "Não foi possível concluir agora. Tente novamente em instantes." }); } };
  return <main className="grid min-h-screen place-items-center bg-slate-50 px-5"><Card className="w-full max-w-md p-8"><Link href="/login" className="text-sm text-brand-700">← Voltar para o login</Link><h1 className="mt-6 text-3xl font-semibold text-slate-950">Esqueci minha senha</h1><p className="mt-3 text-sm leading-6 text-slate-600">Informe seu e-mail. Se existir uma conta, enviaremos instruções para redefinir a senha.</p>{sent ? <p role="status" className="mt-6 rounded-control border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-900">Se existir uma conta com esse e-mail, as instruções foram enviadas.</p> : <form className="mt-6 grid gap-5" onSubmit={handleSubmit(submit)} noValidate><FormField error={errors.email?.message} htmlFor="email" label="E-mail" required><Input id="email" type="email" autoComplete="email" {...register("email")} /></FormField>{errors.root?.message && <p role="alert" className="text-sm text-red-700">{errors.root.message}</p>}<Button loading={isSubmitting} type="submit">{isSubmitting ? "Enviando..." : "Enviar instruções"}</Button></form>}</Card></main>;
}
