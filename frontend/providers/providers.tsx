"use client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createContext, useCallback, useContext, useMemo, useState } from "react";
import { ApiClient } from "@/lib/api/client";
import type { AuthResponse, User } from "@/lib/api/types";
type Auth = { user: User | null; token: string | null; login: (email: string, password: string) => Promise<void>; logout: () => Promise<void> };
const AuthContext = createContext<Auth | null>(null);
export const useAuth = () => { const value = useContext(AuthContext); if (!value) throw new Error("AuthProvider ausente."); return value; };
export const useApi = () => { const { token, logout } = useAuth(); return useMemo(() => new ApiClient(() => token, () => { void logout(); }), [token, logout]); };
export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(() => new QueryClient({ defaultOptions: { queries: { retry: 1, staleTime: 30_000 } } }));
  const [session, setSession] = useState<AuthResponse | null>(null);
  const logout = useCallback(async () => { const refreshToken = session?.refreshToken; setSession(null); queryClient.clear(); if (refreshToken) await new ApiClient(() => null, () => undefined).request("/api/auth/logout", { method: "POST", body: JSON.stringify({ refreshToken }) }).catch(() => undefined); }, [queryClient, session]);
  const login = useCallback(async (email: string, password: string) => { const result = await new ApiClient(() => null, () => undefined).request<AuthResponse>("/api/auth/login", { method: "POST", body: JSON.stringify({ email, password }) }); setSession(result); }, []);
  return <QueryClientProvider client={queryClient}><AuthContext.Provider value={{ user: session?.user ?? null, token: session?.accessToken ?? null, login, logout }}>{children}</AuthContext.Provider></QueryClientProvider>;
}
