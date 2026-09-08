import type { ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/surfaces";
import { Icon, type IconName } from "@/components/ui/icon";
import { cn } from "@/components/ui/utils";

export function FilterToolbar({ children, className }: { children: ReactNode; className?: string }) {
  return <Card className={cn("mt-6 p-4", className)}><div className="grid gap-3 sm:grid-cols-[minmax(14rem,2fr)_repeat(auto-fit,minmax(10rem,1fr))] sm:items-end">{children}</div></Card>;
}

export function RowActions({ children }: { children: ReactNode }) {
  return <div className="flex flex-wrap justify-end gap-1.5">{children}</div>;
}

type RowActionTone = "neutral" | "view" | "agenda" | "edit" | "activate" | "deactivate";

const rowActionTones: Record<RowActionTone, string> = {
  neutral: "text-slate-700 hover:bg-slate-100",
  view: "text-sky-700 hover:bg-sky-50",
  agenda: "text-indigo-700 hover:bg-indigo-50",
  edit: "text-slate-700 hover:bg-slate-100",
  activate: "text-emerald-700 hover:bg-emerald-50",
  deactivate: "text-rose-700 hover:bg-rose-50",
};

export function RowActionButton({ displayLabel, icon, label, loading = false, onClick, tone = "neutral", variant = "ghost" }: { displayLabel?: string; icon: IconName; label: string; loading?: boolean; onClick: () => void; tone?: RowActionTone; variant?: "ghost" | "outline" | "success" | "danger" }) {
  return <Button aria-label={label} className={rowActionTones[tone]} loading={loading} onClick={onClick} size="sm" variant={variant}><Icon className="size-4" name={icon} /><span className="hidden sm:inline">{displayLabel ?? label}</span></Button>;
}
