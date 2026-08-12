import { cn } from "@/components/ui/utils";
import { ReceptionMark } from "@/components/brand/reception-mark";

export function BrandLockup({ collapsed = false, inverse = true, compact = false }: { collapsed?: boolean; inverse?: boolean; compact?: boolean }) {
  return <div aria-label="IA Recepção" className={cn("flex items-center gap-3", collapsed && "justify-center")}><ReceptionMark className={cn(compact ? "size-10" : "size-9", inverse && "brightness-125 saturate-110")} /><strong className={cn("text-sm tracking-tight", inverse ? "text-white" : "text-slate-950", collapsed && "sr-only")}>IA Recepção</strong></div>;
}
