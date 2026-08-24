"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { useEffect, useRef } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useApi, useOptionalAuth } from "@/providers/providers";
import { notificationKeys, notificationsApi } from "@/lib/api/notifications";
import { ApiClient } from "@/lib/api/client";
import type { OperationalNotification } from "@/lib/api/types";
import type { User } from "@/lib/api/types";
import { Button } from "@/components/ui/button";
import { Icon, type IconName } from "@/components/ui/icon";
import { cn } from "@/components/ui/utils";
import { BrandLockup } from "@/components/brand/brand-lockup";

type NavigationItem = { href: string; label: string; icon: IconName; roles?: string[] };
type NavigationGroup = { label: string; items: NavigationItem[] };

const groups: NavigationGroup[] = [
  { label: "Visão geral", items: [{ href: "/dashboard", label: "Dashboard", icon: "dashboard" }] },
  { label: "Atendimento", items: [{ href: "/conversations", label: "Conversas e fila", icon: "messages", roles: ["ClinicAdmin", "Receptionist"] }, { href: "/patients", label: "Pacientes", icon: "patients", roles: ["ClinicAdmin", "Receptionist", "Professional"] }] },
  { label: "Agenda", items: [{ href: "/appointments", label: "Agenda", icon: "calendar", roles: ["ClinicAdmin", "Receptionist", "Professional"] }, { href: "/professionals", label: "Profissionais", icon: "professionals", roles: ["ClinicAdmin"] }, { href: "/specialties", label: "Especialidades", icon: "specialties", roles: ["ClinicAdmin"] }, { href: "/units", label: "Unidades", icon: "units", roles: ["ClinicAdmin"] }] },
  { label: "Administração", items: [{ href: "/clinics", label: "Clínica", icon: "clinic", roles: ["ClinicAdmin"] }, { href: "/setup", label: "Configuração da clínica", icon: "clinic", roles: ["ClinicAdmin"] }, { href: "/platform", label: "Plataforma", icon: "platform", roles: ["PlatformAdmin"] }] },
  { label: "Integrações", items: [{ href: "/integrations/whatsapp", label: "WhatsApp", icon: "whatsapp", roles: ["ClinicAdmin"] }, { href: "/integrations/whatsapp/templates", label: "Templates", icon: "messages", roles: ["ClinicAdmin"] }] },
  { label: "Governança", items: [{ href: "/audit", label: "Auditoria", icon: "audit", roles: ["ClinicAdmin"] }] },
];

function canSee(item: NavigationItem, user: User) {
  return !item.roles || item.roles.includes(user.role);
}

function isActive(pathname: string, href: string) {
  return pathname === href || (href !== "/dashboard" && pathname.startsWith(`${href}/`));
}

function NavLinks({ user, collapsed, onNavigate, waitingCount = 0 }: { user: User; collapsed: boolean; onNavigate?: () => void; waitingCount?: number }) {
  const pathname = usePathname();

  return <nav className="grid gap-5" aria-label="Navegação principal">
    {groups.map(group => {
      const items = group.items.filter(item => canSee(item, user));
      if (!items.length) return null;

      return <section key={group.label}>
        <p className={cn("mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-slate-400", collapsed && "sr-only")}>{group.label}</p>
        <div className="grid gap-1">
          {items.map(item => <Link aria-current={isActive(pathname, item.href) ? "page" : undefined} className={cn("group flex items-center gap-3 rounded-control px-3 py-2.5 text-sm font-medium text-slate-300 transition-colors hover:bg-brand-dark-surface hover:text-white", isActive(pathname, item.href) && "bg-brand-primary text-white hover:bg-brand-primary-hover", collapsed && "justify-center px-2")} href={item.href} key={item.href} onClick={onNavigate} title={collapsed ? item.label : undefined}>
            <Icon name={item.icon} />
            <span className={collapsed ? "sr-only" : "truncate"}>{item.label}</span>{item.href === "/conversations" && waitingCount > 0 && <span className={cn("ml-auto rounded-full bg-amber-400 px-1.5 text-[10px] font-bold text-slate-950", collapsed && "sr-only")}>{waitingCount}</span>}
          </Link>)}
        </div>
      </section>;
    })}
  </nav>;
}

function Brand({ collapsed = false }: { collapsed?: boolean }) {
  return <div className="px-2"><BrandLockup collapsed={collapsed} /></div>;
}

export function AppShell({ user, realtimeStatus, onLogout, children }: { user: User; realtimeStatus: "offline" | "connecting" | "online"; onLogout: () => void; children: React.ReactNode }) {
  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const statusLabel = realtimeStatus === "online" ? "Tempo real conectado" : realtimeStatus === "connecting" ? "Reconectando em tempo real" : "Tempo real indisponível";

  const auth = useOptionalAuth();
  return <div className="min-h-screen bg-canvas md:flex">
    <aside className={cn("sticky top-0 hidden h-screen shrink-0 flex-col border-r border-brand-dark-border bg-brand-dark p-3 text-white transition-[width] duration-ui md:flex", collapsed ? "w-20" : "w-72")}>
      <div className="mb-7"><Brand collapsed={collapsed} /></div>
      <div className="brand-scrollbar min-h-0 flex-1 overflow-y-auto">{auth ? <NotificationNav collapsed={collapsed} user={user} /> : <NavLinks collapsed={collapsed} user={user} />}</div>
      <div className="mt-3 grid gap-2 border-t border-brand-dark-border pt-3">
        <p className={cn("truncate px-2 text-xs text-slate-400", collapsed && "sr-only")}>{user.name}</p>
        <Button aria-label={collapsed ? "Expandir menu" : "Recolher menu"} className={cn("w-full justify-start text-slate-300 hover:bg-slate-800 hover:text-white", collapsed && "justify-center")} onClick={() => setCollapsed(value => !value)} size="sm" variant="ghost">
          <Icon className={cn("transition-transform", !collapsed && "rotate-180")} name="collapse" />
          <span className={collapsed ? "sr-only" : ""}>Recolher menu</span>
        </Button>
      </div>
    </aside>

    <div className="min-w-0 flex-1">
      <header className="sticky top-0 z-navigation flex min-h-16 items-center justify-between border-b border-border-system bg-white/95 px-4 backdrop-blur sm:px-6">
        <div className="flex min-w-0 items-center gap-3">
          <Button aria-label="Abrir menu" className="md:hidden" onClick={() => setMobileOpen(true)} size="icon" variant="ghost"><Icon name="menu" /></Button>
          <div className="min-w-0"><p className="truncate text-sm font-semibold text-slate-900">{user.name}</p><p className="hidden truncate text-xs text-slate-500 sm:block">Tenant: {user.tenantId}</p></div>
        </div>
        <div className="flex items-center gap-2">
          <span className="hidden items-center gap-1.5 text-xs text-slate-600 sm:flex"><Icon className={cn("size-4", realtimeStatus === "online" && "text-emerald-600", realtimeStatus === "connecting" && "text-amber-600", realtimeStatus === "offline" && "text-slate-400")} name="signal" />{statusLabel}</span>
          {auth && (user.role === "ClinicAdmin" || user.role === "Receptionist") && <NotificationCenter />}
          <Button onClick={onLogout} size="sm" variant="ghost"><Icon name="logout" /><span className="hidden sm:inline">Sair</span></Button>
        </div>
      </header>
      {children}
    </div>

    {mobileOpen && <div className="fixed inset-0 z-overlay md:hidden" onKeyDown={event => { if (event.key === "Escape") setMobileOpen(false); }} role="dialog" aria-modal="true" aria-label="Menu de navegação">
      <button className="absolute inset-0 bg-slate-950/50" aria-label="Fechar menu ao tocar fora" onClick={() => setMobileOpen(false)} />
      <aside className="relative flex h-full w-72 flex-col bg-brand-dark p-4 text-white shadow-floating">
        <div className="mb-7 flex items-center justify-between"><Brand /><Button aria-label="Fechar menu" className="text-slate-300 hover:bg-slate-800 hover:text-white" onClick={() => setMobileOpen(false)} size="icon" variant="ghost"><Icon name="close" /></Button></div>
        <div className="brand-scrollbar min-h-0 flex-1 overflow-y-auto">{auth ? <NotificationNav collapsed={false} onNavigate={() => setMobileOpen(false)} user={user} /> : <NavLinks collapsed={false} onNavigate={() => setMobileOpen(false)} user={user} />}</div>
      </aside>
    </div>}
  </div>;
}

function NotificationNav({ user, collapsed, onNavigate }: { user: User; collapsed: boolean; onNavigate?: () => void }) {
  const auth = useOptionalAuth(); const api = auth ? new ApiClient(() => auth.token, () => undefined) : null;
  const summary = useQuery({ queryKey: notificationKeys.summary, queryFn: () => notificationsApi.summary(api!), enabled: Boolean(api) && (user.role === "ClinicAdmin" || user.role === "Receptionist"), refetchInterval: 60_000 });
  return <NavLinks collapsed={collapsed} onNavigate={onNavigate} user={user} waitingCount={summary.data?.waitingCount} />;
}

function NotificationCenter() {
  const api = useApi(); const client = useQueryClient(); const [open, setOpen] = useState(false); const [toast, setToast] = useState(false); const previous = useRef(0);
  const summaryQuery = useQuery({ queryKey: notificationKeys.summary, queryFn: () => notificationsApi.summary(api), refetchInterval: 60_000 });
  const summary = summaryQuery.data;
  const list = useQuery({ queryKey: notificationKeys.all, queryFn: () => notificationsApi.list(api), enabled: open });
  useEffect(() => { if ((summary?.waitingCount ?? 0) > previous.current) { setToast(true); const timer = window.setTimeout(() => setToast(false), 7000); return () => window.clearTimeout(timer); } previous.current = summary?.waitingCount ?? 0; }, [summary?.waitingCount]);
  const markRead = async (item: OperationalNotification) => { await notificationsApi.read(api, item.id); await client.invalidateQueries({ queryKey: notificationKeys.all }); await client.invalidateQueries({ queryKey: notificationKeys.summary }); window.location.assign(`/conversations?conversationId=${item.conversationId}`); };
  return <div className="relative"><Button aria-label="Notificações operacionais" className="relative" onClick={() => setOpen(value => !value)} size="icon" variant="ghost"><Icon name="bell" />{summary && summary.unreadCount > 0 && <span className="absolute -right-0.5 -top-0.5 min-w-4 rounded-full bg-red-500 px-1 text-[10px] font-bold leading-4 text-white">{summary.unreadCount}</span>}</Button>{open && <div className="absolute right-0 z-popover mt-2 w-[min(22rem,calc(100vw-2rem))] rounded-control border border-border-system bg-white p-3 shadow-floating"><div className="flex items-center justify-between"><strong>Notificações</strong><Button onClick={async () => { await notificationsApi.readAll(api); await client.invalidateQueries({ queryKey: notificationKeys.all }); await client.invalidateQueries({ queryKey: notificationKeys.summary }); }} size="sm" variant="ghost">Marcar como lidas</Button></div><div className="mt-2 max-h-96 space-y-2 overflow-y-auto">{list.data?.items?.length ? list.data.items.map(item => <button className="w-full rounded-control border border-slate-200 p-3 text-left hover:bg-slate-50" key={item.id} onClick={() => void markRead(item)}><div className="flex items-center justify-between gap-2"><span className="text-sm font-semibold">{item.type === "HumanQueueSlaExceeded" ? "Atendimento em atraso" : item.type === "HumanQueueReminder" ? "Atendimento ainda aguardando" : "Novo atendimento aguardando"}</span><span className="text-xs text-slate-500">{item.severity}</span></div><p className="mt-1 text-xs text-slate-600">{item.patientName} · {item.waitingSince ? `aguardando desde ${new Date(item.waitingSince).toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" })}` : "fila humana"}</p><span className="mt-2 inline-block text-xs font-medium text-brand-700">Ver conversa</span></button>) : <p className="p-4 text-sm text-slate-500">Nenhuma notificação pendente.</p>}</div></div>}{toast && <div aria-live="polite" role="status" className="fixed inset-x-4 top-20 z-toast sm:left-auto sm:right-5 sm:w-[min(24rem,calc(100vw-2rem))]"><div className="rounded-panel border border-amber-200 border-l-4 border-l-amber-400 bg-white p-4 shadow-floating"><div className="flex items-start gap-3"><div className="min-w-0 flex-1"><strong className="block text-sm text-slate-950">Novo atendimento aguardando</strong><p className="mt-1 text-xs leading-5 text-slate-600">Um paciente solicitou falar com a recepção.</p><Link className="mt-3 inline-flex min-h-9 items-center rounded-control bg-brand-primary px-3 text-sm font-semibold text-white hover:bg-brand-primary-hover" href="/conversations?queue=waiting" onClick={() => setToast(false)}>Ver fila</Link></div><Button aria-label="Fechar notificação" className="-mr-2 -mt-2 shrink-0 text-slate-500 hover:bg-slate-100 hover:text-slate-900" onClick={() => setToast(false)} size="icon" variant="ghost"><Icon name="close" /></Button></div></div></div>}</div>;
}
