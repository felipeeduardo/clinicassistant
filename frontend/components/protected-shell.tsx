"use client";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { AppShell } from "@/components/app-shell";
import { useAuth } from "@/providers/providers";
export function ProtectedShell({ children }: { children: React.ReactNode }) { const { user, authReady, logout, realtimeStatus } = useAuth(); const router = useRouter(); useEffect(() => { if (authReady && !user) router.replace("/login"); }, [authReady, router, user]); if (!authReady || !user) return <main className="p-6">Carregando sessão...</main>; return <AppShell onLogout={() => void logout()} realtimeStatus={realtimeStatus} user={user}>{children}</AppShell>; }
