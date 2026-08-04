import { forwardRef } from "react";
import type { ButtonHTMLAttributes } from "react";
import { cn } from "./utils";

type ButtonVariant = "primary" | "secondary" | "outline" | "ghost" | "danger" | "success" | "link";
type ButtonSize = "sm" | "md" | "lg" | "icon";
export type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & { variant?: ButtonVariant; size?: ButtonSize; loading?: boolean };

const variants: Record<ButtonVariant, string> = {
  primary: "bg-brand-600 text-white hover:bg-brand-700",
  secondary: "bg-slate-900 text-white hover:bg-slate-800",
  outline: "border border-slate-300 bg-white text-slate-800 hover:bg-slate-50",
  ghost: "text-current hover:bg-slate-100",
  danger: "bg-red-600 text-white hover:bg-red-700",
  success: "bg-emerald-600 text-white hover:bg-emerald-700",
  link: "text-brand-700 underline-offset-4 hover:underline",
};
const sizes: Record<ButtonSize, string> = { sm: "min-h-8 px-3 text-sm", md: "min-h-10 px-4 text-sm", lg: "min-h-11 px-5 text-base", icon: "size-10 p-0" };

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button({ className, variant = "primary", size = "md", loading = false, disabled, children, type = "button", ...props }, ref) {
  return <button ref={ref} type={type} className={cn("inline-flex items-center justify-center gap-2 rounded-control font-medium transition-colors duration-ui disabled:opacity-50", variants[variant], sizes[size], className)} disabled={disabled || loading} aria-busy={loading || undefined} {...props}>{loading && <span className="size-4 animate-spin rounded-full border-2 border-current border-r-transparent" aria-hidden="true" />}{children}</button>;
});
