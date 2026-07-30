"use client";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useAuth } from "@/providers/providers";
const schema = z.object({ email: z.string().email("Informe um e-mail válido."), password: z.string().min(1, "Informe sua senha.") });
type Form = z.infer<typeof schema>;
export default function LoginPage() { const router = useRouter(); const { login } = useAuth(); const { register, handleSubmit, formState: { errors, isSubmitting }, setError } = useForm<Form>({ resolver: zodResolver(schema) }); const submit = async (data: Form) => { try { await login(data.email, data.password); router.replace("/dashboard"); } catch { setError("root", { message: "Não foi possível entrar. Verifique suas credenciais." }); } }; return <main className="grid min-h-screen place-items-center p-4"><form onSubmit={handleSubmit(submit)} className="w-full max-w-md space-y-4 rounded-xl bg-white p-8 shadow" aria-label="Entrar"><h1 className="text-2xl font-bold">Clinic Assistant</h1><label className="block">E-mail<input className="mt-1 w-full rounded border p-2" autoComplete="email" {...register("email")} /></label>{errors.email && <p role="alert">{errors.email.message}</p>}<label className="block">Senha<input className="mt-1 w-full rounded border p-2" type="password" autoComplete="current-password" {...register("password")} /></label>{errors.password && <p role="alert">{errors.password.message}</p>}{errors.root && <p role="alert">{errors.root.message}</p>}<button className="w-full rounded bg-blue-700 p-2 text-white disabled:opacity-50" disabled={isSubmitting}>{isSubmitting ? "Entrando..." : "Entrar"}</button></form></main>; }
