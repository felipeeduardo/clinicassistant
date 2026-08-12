import { cn } from "@/components/ui/utils";

type ReceptionMarkProps = {
  className?: string;
  monochrome?: boolean;
};

/** IA Recepção · direção D, variação 1 (abraço equilibrado). */
export function ReceptionMark({ className, monochrome = false }: ReceptionMarkProps) {
  return <svg aria-hidden="true" className={cn("size-10", className)} fill="none" viewBox="0 0 64 64">
    <path d="M25 10C15.6 13.6 10 22.1 10 32c0 10.8 6.7 20.1 16.2 23.9" fill="none" stroke={monochrome ? "currentColor" : "#172f57"} strokeLinecap="round" strokeWidth="10" />
    <path d="M39 10c9.4 3.6 15 12.1 15 22 0 10.8-6.7 20.1-16.2 23.9" fill="none" stroke={monochrome ? "currentColor" : "#18a77a"} strokeLinecap="round" strokeWidth="10" />
    <circle cx="32" cy="32" fill={monochrome ? "currentColor" : "#2463d8"} r="7" />
  </svg>;
}
