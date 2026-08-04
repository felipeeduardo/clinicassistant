import type { ReactNode, SVGProps } from "react";
import { cn } from "./utils";

export type IconName = "dashboard" | "messages" | "patients" | "calendar" | "professionals" | "specialties" | "units" | "clinic" | "whatsapp" | "audit" | "platform" | "menu" | "close" | "collapse" | "logout" | "chevronRight" | "signal";
export function Icon({ name, className, ...props }: SVGProps<SVGSVGElement> & { name: IconName }) {
  const common = { className: cn("size-5 shrink-0", className), fill: "none", stroke: "currentColor", strokeWidth: 1.8, strokeLinecap: "round" as const, strokeLinejoin: "round" as const, viewBox: "0 0 24 24", "aria-hidden": true };
  const paths: Record<IconName, ReactNode> = {
    dashboard: <><rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" /><rect x="3" y="14" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" /></>,
    messages: <><path d="M4 5.5A2.5 2.5 0 0 1 6.5 3h11A2.5 2.5 0 0 1 20 5.5v7a2.5 2.5 0 0 1-2.5 2.5H10l-4.5 4v-4.2A2.5 2.5 0 0 1 4 12.5z" /><path d="M8 8h8M8 11h5" /></>,
    patients: <><circle cx="12" cy="8" r="3" /><path d="M5 21a7 7 0 0 1 14 0" /></>,
    calendar: <><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M8 3v4M16 3v4M3 10h18" /></>,
    professionals: <><path d="M8 4h8v4h3v12H5V8h3z" /><path d="M10 13h4M12 11v4" /></>,
    specialties: <><path d="m12 3 2.2 4.8L19 10l-4.8 2.2L12 17l-2.2-4.8L5 10l4.8-2.2z" /><path d="m19 16 .8 1.7L21.5 19l-1.7.8L19 21.5l-.8-1.7-1.7-.8 1.7-.8z" /></>,
    units: <><path d="M4 21V5l8-3 8 3v16" /><path d="M8 21v-5h8v5M8 8h.01M12 8h.01M16 8h.01M8 12h.01M12 12h.01M16 12h.01" /></>,
    clinic: <><path d="M4 21V7l8-4 8 4v14" /><path d="M9 21v-5h6v5M8 9h8M12 6v6M9 12h6" /></>,
    whatsapp: <><path d="M20 11.5a8 8 0 1 1-14.8 4.2L4 20l4.5-1.2A8 8 0 0 1 20 11.5Z" /><path d="M9 8.5c.4 3 2.3 4.9 5.3 5.3l1-1.1-1.4-1.1-.9.3a5.5 5.5 0 0 1-1.9-1.9l.3-.9-1.1-1.4z" /></>,
    audit: <><path d="M7 3h10v18H7z" /><path d="M9 8h6M9 12h6M9 16h4M9 3v3h6V3" /></>,
    platform: <><path d="M3 21h18M5 21V8h14v13M3 8l9-5 9 5M9 12h.01M15 12h.01M9 16h.01M15 16h.01" /></>,
    menu: <path d="M4 7h16M4 12h16M4 17h16" />,
    close: <path d="m6 6 12 12M18 6 6 18" />,
    collapse: <path d="m14 6-6 6 6 6" />,
    logout: <><path d="M10 5H5v14h5" /><path d="m14 8 4 4-4 4M18 12H9" /></>,
    chevronRight: <path d="m9 18 6-6-6-6" />,
    signal: <><path d="M5 19v-2M9 19v-5M13 19v-8M17 19V7" /></>,
  };
  return <svg {...common} {...props}>{paths[name]}</svg>;
}
