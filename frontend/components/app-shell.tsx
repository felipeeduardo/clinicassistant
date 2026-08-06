"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import type { User } from "@/lib/api/types";
import { Button } from "@/components/ui/button";
import { Icon, type IconName } from "@/components/ui/icon";
import { cn } from "@/components/ui/utils";

type NavigationItem = { href: string; label: string; icon: IconName; roles?: string[] };
type NavigationGroup = { label: string; items: NavigationItem[] };

const groups: NavigationGroup[] = [
  { label: "Visão geral", items: [{ href: "/dashboard", label: "Dashboard", icon: "dashboard" }] },
  { label: "Atendimento", items: [{ href: "/conversations", label: "Conversas e fila", icon: "messages" }, { href: "/patients", label: "Pacientes", icon: "patients" }] },
  { label: "Agenda", items: [{ href: "/appointments", label: "Agenda", icon: "calendar" }, { href: "/professionals", label: "Profissionais", icon: "professionals" }, { href: "/specialties", label: "Especialidades", icon: "specialties" }, { href: "/units", label: "Unidades", icon: "units" }] },
  { label: "Administração", items: [{ href: "/clinics", label: "Clínica", icon: "clinic", roles: ["ClinicAdmin"] }, { href: "/platform", label: "Plataforma", icon: "platform", roles: ["PlatformAdmin"] }] },
  { label: "Integrações", items: [{ href: "/integrations/whatsapp", label: "WhatsApp", icon: "whatsapp", roles: ["ClinicAdmin"] }, { href: "/integrations/whatsapp/templates", label: "Templates", icon: "messages", roles: ["ClinicAdmin"] }, { href: "/settings/integrations/twilio", label: "Configuração Twilio", icon: "whatsapp", roles: ["ClinicAdmin"] }] },
  { label: "Governança", items: [{ href: "/audit", label: "Auditoria", icon: "audit", roles: ["ClinicAdmin"] }] },
];

function canSee(item: NavigationItem, user: User) {
  return !item.roles || item.roles.includes(user.role);
}

function isActive(pathname: string, href: string) {
  return pathname === href || (href !== "/dashboard" && pathname.startsWith(`${href}/`));
}

function NavLinks({ user, collapsed, onNavigate }: { user: User; collapsed: boolean; onNavigate?: () => void }) {
  const pathname = usePathname();

  return <nav className="grid gap-5" aria-label="Navegação principal">
    {groups.map(group => {
      const items = group.items.filter(item => canSee(item, user));
      if (!items.length) return null;

      return <section key={group.label}>
        <p className={cn("mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-slate-400", collapsed && "sr-only")}>{group.label}</p>
        <div className="grid gap-1">
          {items.map(item => <Link aria-current={isActive(pathname, item.href) ? "page" : undefined} className={cn("group flex items-center gap-3 rounded-control px-3 py-2.5 text-sm font-medium text-slate-300 transition-colors hover:bg-slate-800 hover:text-white", isActive(pathname, item.href) && "bg-brand-600 text-white hover:bg-brand-600", collapsed && "justify-center px-2")} href={item.href} key={item.href} onClick={onNavigate} title={collapsed ? item.label : undefined}>
            <Icon name={item.icon} />
            <span className={collapsed ? "sr-only" : "truncate"}>{item.label}</span>
          </Link>)}
        </div>
      </section>;
    })}
  </nav>;
}

function Brand({ collapsed = false }: { collapsed?: boolean }) {
  return <div className={cn("flex items-center gap-3 px-2", collapsed && "justify-center")}>
    <span className="grid size-9 place-items-center rounded-control bg-brand-600 font-bold">CA</span>
    <strong className={cn("text-sm", collapsed && "sr-only")}>Clinic Assistant</strong>
  </div>;
}

export function AppShell({ user, realtimeStatus, onLogout, children }: { user: User; realtimeStatus: "offline" | "connecting" | "online"; onLogout: () => void; children: React.ReactNode }) {
  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const statusLabel = realtimeStatus === "online" ? "Tempo real conectado" : realtimeStatus === "connecting" ? "Reconectando em tempo real" : "Tempo real indisponível";

  return <div className="min-h-screen bg-slate-50 md:flex">
    <aside className={cn("sticky top-0 hidden h-screen shrink-0 flex-col border-r border-slate-800 bg-slate-950 p-3 text-white transition-[width] duration-ui md:flex", collapsed ? "w-20" : "w-72")}>
      <div className="mb-7"><Brand collapsed={collapsed} /></div>
      <div className="min-h-0 flex-1 overflow-y-auto"><NavLinks collapsed={collapsed} user={user} /></div>
      <div className="mt-3 grid gap-2 border-t border-slate-800 pt-3">
        <p className={cn("truncate px-2 text-xs text-slate-400", collapsed && "sr-only")}>{user.name}</p>
        <Button aria-label={collapsed ? "Expandir menu" : "Recolher menu"} className={cn("w-full justify-start text-slate-300 hover:bg-slate-800 hover:text-white", collapsed && "justify-center")} onClick={() => setCollapsed(value => !value)} size="sm" variant="ghost">
          <Icon className={cn("transition-transform", !collapsed && "rotate-180")} name="collapse" />
          <span className={collapsed ? "sr-only" : ""}>Recolher menu</span>
        </Button>
      </div>
    </aside>

    <div className="min-w-0 flex-1">
      <header className="sticky top-0 z-navigation flex min-h-16 items-center justify-between border-b border-slate-200 bg-white/95 px-4 backdrop-blur sm:px-6">
        <div className="flex min-w-0 items-center gap-3">
          <Button aria-label="Abrir menu" className="md:hidden" onClick={() => setMobileOpen(true)} size="icon" variant="ghost"><Icon name="menu" /></Button>
          <div className="min-w-0"><p className="truncate text-sm font-semibold text-slate-900">{user.name}</p><p className="hidden truncate text-xs text-slate-500 sm:block">Tenant: {user.tenantId}</p></div>
        </div>
        <div className="flex items-center gap-2">
          <span className="hidden items-center gap-1.5 text-xs text-slate-600 sm:flex"><Icon className={cn("size-4", realtimeStatus === "online" && "text-emerald-600", realtimeStatus === "connecting" && "text-amber-600", realtimeStatus === "offline" && "text-slate-400")} name="signal" />{statusLabel}</span>
          <Button onClick={onLogout} size="sm" variant="ghost"><Icon name="logout" /><span className="hidden sm:inline">Sair</span></Button>
        </div>
      </header>
      {children}
    </div>

    {mobileOpen && <div className="fixed inset-0 z-overlay md:hidden" onKeyDown={event => { if (event.key === "Escape") setMobileOpen(false); }} role="dialog" aria-modal="true" aria-label="Menu de navegação">
      <button className="absolute inset-0 bg-slate-950/50" aria-label="Fechar menu ao tocar fora" onClick={() => setMobileOpen(false)} />
      <aside className="relative flex h-full w-72 flex-col bg-slate-950 p-4 text-white shadow-floating">
        <div className="mb-7 flex items-center justify-between"><Brand /><Button aria-label="Fechar menu" className="text-slate-300 hover:bg-slate-800 hover:text-white" onClick={() => setMobileOpen(false)} size="icon" variant="ghost"><Icon name="close" /></Button></div>
        <div className="min-h-0 flex-1 overflow-y-auto"><NavLinks collapsed={false} onNavigate={() => setMobileOpen(false)} user={user} /></div>
      </aside>
    </div>}
  </div>;
}
