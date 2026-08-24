"use client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { HubConnectionBuilder, HttpTransportType, LogLevel } from "@microsoft/signalr";
import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import { ApiClient } from "@/lib/api/client";
import type { AuthResponse, User } from "@/lib/api/types";
type Auth = { user: User | null; token: string | null; authReady: boolean; realtimeStatus: "offline" | "connecting" | "online"; login: (email: string, password: string) => Promise<void>; logout: () => Promise<void> };
const AuthContext = createContext<Auth | null>(null);
export const useAuth = () => { const value = useContext(AuthContext); if (!value) throw new Error("AuthProvider ausente."); return value; };
export const useOptionalAuth = () => useContext(AuthContext);
export const useApi = () => { const { token, logout } = useAuth(); return useMemo(() => new ApiClient(() => token, () => { void logout(); }), [token, logout]); };
export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(() => new QueryClient({ defaultOptions: { queries: { retry: 1, staleTime: 30_000 } } }));
  const [session, setSession] = useState<AuthResponse | null>(null);
  const [authReady, setAuthReady] = useState(false);
  const [realtimeStatus, setRealtimeStatus] = useState<"offline" | "connecting" | "online">("offline");
  const logout = useCallback(async () => { setSession(null); queryClient.clear(); await new ApiClient(() => null, () => undefined).request("/api/auth/logout", { method: "POST" }).catch(() => undefined); }, [queryClient]);
  const login = useCallback(async (email: string, password: string) => { const result = await new ApiClient(() => null, () => undefined).request<AuthResponse>("/api/auth/login", { method: "POST", body: JSON.stringify({ email, password }) }); setSession(result); }, []);
  useEffect(() => { let active = true; void new ApiClient(() => null, () => undefined).request<AuthResponse>("/api/auth/refresh", { method: "POST" }).then(result => { if (active) setSession(result); }).catch(() => undefined).finally(() => { if (active) setAuthReady(true); }); return () => { active = false; }; }, []);
  return <QueryClientProvider client={queryClient}><AuthContext.Provider value={{ user: session?.user ?? null, token: session?.accessToken ?? null, authReady, realtimeStatus, login, logout }}><RealtimeBridge token={session?.accessToken ?? null} queryClient={queryClient} setStatus={setRealtimeStatus} />{children}</AuthContext.Provider></QueryClientProvider>;
}

type RealtimeEvent = { eventId?: string; payload?: unknown };
function RealtimeBridge({ token, queryClient, setStatus }: { token: string | null; queryClient: QueryClient; setStatus: (value: "offline" | "connecting" | "online") => void }) {
  const processed = useRef(new Set<string>());
  useEffect(() => {
    if (!token) { setStatus("offline"); return; }
    const connection = new HubConnectionBuilder().withUrl(`${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080"}/hubs/operations`, { accessTokenFactory: () => token, transport: HttpTransportType.WebSockets }).withAutomaticReconnect().configureLogging(LogLevel.Warning).build();
    const invalidate = (event: RealtimeEvent, keys: string[][]) => { if (event.eventId && processed.current.has(event.eventId)) return; if (event.eventId) { processed.current.add(event.eventId); if (processed.current.size > 200) processed.current.clear(); } keys.forEach(key => void queryClient.invalidateQueries({ queryKey: key })); };
    connection.on("conversation.updated", (event: RealtimeEvent) => invalidate(event, [["conversations"], ["conversation"], ["conversation-messages"]]));
    connection.on("appointment.created", (event: RealtimeEvent) => invalidate(event, [["appointments"], ["availability"]]));
    connection.on("appointment.updated", (event: RealtimeEvent) => invalidate(event, [["appointments"], ["availability"]]));
    connection.on("appointment.confirmed", (event: RealtimeEvent) => invalidate(event, [["appointments"], ["availability"], ["dashboard"]]));
    connection.on("appointment.rescheduled", (event: RealtimeEvent) => invalidate(event, [["appointments"], ["availability"], ["dashboard"]]));
    connection.on("appointment.cancelled", (event: RealtimeEvent) => invalidate(event, [["appointments"], ["availability"]]));
    connection.on("availability.updated", (event: RealtimeEvent) => invalidate(event, [["appointments"], ["availability"]]));
    connection.on("professional.block.created", (event: RealtimeEvent) => invalidate(event, [["professional-schedule"], ["professional-blocks"], ["appointments"], ["availability"]]));
    connection.on("professional.block.deleted", (event: RealtimeEvent) => invalidate(event, [["professional-schedule"], ["professional-blocks"], ["appointments"], ["availability"]]));
    connection.on("professional.vacation.created", (event: RealtimeEvent) => invalidate(event, [["professional-schedule"], ["professional-vacations"], ["appointments"], ["availability"]]));
    connection.on("professional.vacation.deleted", (event: RealtimeEvent) => invalidate(event, [["professional-schedule"], ["professional-vacations"], ["appointments"], ["availability"]]));
    connection.on("whatsapp.integration.updated", (event: RealtimeEvent) => invalidate(event, [["integration-status"]]));
    connection.on("whatsapp.inbound.received", (event: RealtimeEvent) => invalidate(event, [["conversations"], ["conversation"], ["conversation-messages"], ["dashboard"]]));
    connection.on("whatsapp.message.status.changed", (event: RealtimeEvent) => invalidate(event, [["conversations"], ["conversation"], ["conversation-messages"]]));
    connection.on("whatsapp.template.created", (event: RealtimeEvent) => invalidate(event, [["whatsapp-templates"]]));
    connection.on("whatsapp.template.updated", (event: RealtimeEvent) => invalidate(event, [["whatsapp-templates"], ["whatsapp-template"]]));
    connection.on("whatsapp.template.activated", (event: RealtimeEvent) => invalidate(event, [["whatsapp-templates"], ["whatsapp-template"]]));
    connection.on("whatsapp.template.deactivated", (event: RealtimeEvent) => invalidate(event, [["whatsapp-templates"], ["whatsapp-template"]]));
    connection.on("whatsapp.template.synced", (event: RealtimeEvent) => invalidate(event, [["whatsapp-templates"]]));
    connection.on("queue.item.created", (event: RealtimeEvent) => invalidate(event, [["conversation-queue"], ["conversations"], ["conversation"]]));
    connection.on("queue.item.updated", (event: RealtimeEvent) => invalidate(event, [["conversation-queue"], ["conversations"], ["conversation"]]));
    connection.on("queue.item.assigned", (event: RealtimeEvent) => invalidate(event, [["conversation-queue"], ["conversations"], ["conversation"]]));
    connection.on("queue.item.released", (event: RealtimeEvent) => invalidate(event, [["conversation-queue"], ["conversations"], ["conversation"]]));
    connection.on("queue.item.transferred", (event: RealtimeEvent) => invalidate(event, [["conversation-queue"], ["conversations"], ["conversation"]]));
    connection.on("queue.item.completed", (event: RealtimeEvent) => invalidate(event, [["conversation-queue"], ["conversations"], ["conversation"]]));
    connection.on("audit.created", (event: RealtimeEvent) => invalidate(event, [["audit"], ["clinic"], ["units"], ["specialties"], ["professionals"]]));
    connection.on("dashboard.invalidated", (event: RealtimeEvent) => invalidate(event, [["dashboard"]]));
    connection.on("human.handoff.requested", (event: RealtimeEvent) => invalidate(event, [["notifications"], ["notifications", "summary"], ["conversations"], ["conversation-queue"]]));
    connection.on("human.queue.reminder", (event: RealtimeEvent) => invalidate(event, [["notifications"], ["notifications", "summary"]]));
    connection.on("human.queue.sla.exceeded", (event: RealtimeEvent) => invalidate(event, [["notifications"], ["notifications", "summary"], ["conversations"], ["conversation-queue"]]));
    connection.on("human.conversation.resolved", (event: RealtimeEvent) => invalidate(event, [["notifications"], ["notifications", "summary"], ["conversations"], ["conversation-queue"]]));
    connection.onreconnecting(() => setStatus("connecting")); connection.onreconnected(() => setStatus("online")); connection.onclose(() => setStatus("offline"));
    setStatus("connecting"); void connection.start().then(() => setStatus("online")).catch(() => setStatus("offline"));
    return () => { void connection.stop(); setStatus("offline"); };
  }, [queryClient, setStatus, token]);
  return null;
}
