import { forwardRef } from "react";
import type { ButtonHTMLAttributes } from "react";
import { cn } from "./utils";

type ButtonVariant = "primary" | "secondary" | "outline" | "ghost" | "danger" | "success" | "link";
type ButtonSize = "sm" | "md" | "lg" | "icon";
export type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & { variant?: ButtonVariant; size?: ButtonSize; loading?: boolean };

const variants: Record<ButtonVariant, string> = {
  primary: "bg-brand-primary text-white hover:bg-brand-primary-hover",
  secondary: "bg-brand-dark text-white hover:bg-brand-dark-surface",
  outline: "border border-border-system-strong bg-white text-foreground hover:bg-surface-subtle",
  ghost: "text-current hover:bg-surface-subtle",
  danger: "bg-red-600 text-white hover:bg-red-700",
  success: "bg-emerald-600 text-white hover:bg-emerald-700",
  link: "text-brand-primary underline-offset-4 hover:underline",
};
const sizes: Record<ButtonSize, string> = { sm: "min-h-8 px-3 text-sm", md: "min-h-10 px-4 text-sm", lg: "min-h-11 px-5 text-base", icon: "size-10 p-0" };

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button({ className, variant = "primary", size = "md", loading = false, disabled, children, type = "button", ...props }, ref) {
  return <button ref={ref} type={type} className={cn("inline-flex items-center justify-center gap-2 rounded-control font-medium transition-colors duration-ui disabled:opacity-50", variants[variant], sizes[size], className)} disabled={disabled || loading} aria-busy={loading || undefined} {...props}>{loading && <span className="size-4 animate-spin rounded-full border-2 border-current border-r-transparent" aria-hidden="true" />}{children}</button>;
});
