import { cloneElement, forwardRef, isValidElement, useId } from "react";
import type { InputHTMLAttributes, SelectHTMLAttributes, TextareaHTMLAttributes } from "react";
import { cn } from "./utils";

export type ControlSize = "sm" | "md" | "lg";

const control = "w-full rounded-control border border-slate-300 bg-white text-slate-900 shadow-sm transition-colors placeholder:text-slate-400 hover:border-slate-400 disabled:bg-slate-100 disabled:text-slate-500";
const controlSizes: Record<ControlSize, string> = {
  sm: "h-9 px-3 text-sm",
  md: "h-10 px-3 text-sm",
  lg: "h-11 px-4 text-base",
};

type InputProps = Omit<InputHTMLAttributes<HTMLInputElement>, "size"> & { size?: ControlSize };
type SelectProps = Omit<SelectHTMLAttributes<HTMLSelectElement>, "size"> & { size?: ControlSize };
type TextareaProps = TextareaHTMLAttributes<HTMLTextAreaElement> & { size?: ControlSize };

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input({ className, size = "md", ...props }, ref) { return <input ref={ref} className={cn(control, controlSizes[size], className)} {...props} />; });
export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select({ className, size = "md", ...props }, ref) { return <select ref={ref} className={cn(control, controlSizes[size], className)} {...props} />; });
export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(function Textarea({ className, size = "md", ...props }, ref) { return <textarea ref={ref} className={cn(control, controlSizes[size], "min-h-24 resize-y", className)} {...props} />; });

export function FormField({ label, hint, error, required, htmlFor, children }: { label: string; hint?: string; error?: string; required?: boolean; htmlFor?: string; children: React.ReactNode }) {
  const generatedId = useId(); const fieldId = htmlFor ?? generatedId; const hintId = `${fieldId}-hint`; const errorId = `${fieldId}-error`; const describedBy = [hint ? hintId : undefined, error ? errorId : undefined].filter(Boolean).join(" ") || undefined;
  const control = isValidElement<{ "aria-describedby"?: string; "aria-invalid"?: boolean }>(children) ? cloneElement(children, { "aria-describedby": describedBy, "aria-invalid": error ? true : undefined }) : children;
  return <div className="grid gap-1.5"><label className="text-sm font-medium text-slate-800" htmlFor={htmlFor}>{label}{required && <span className="ml-1 text-red-700" aria-label="obrigatório">*</span>}</label>{control}{hint && <p id={hintId} className="text-xs text-slate-500">{hint}</p>}{error && <p id={errorId} className="text-sm text-red-700" role="alert">{error}</p>}</div>;
}
export function FormSection({ title, description, children }: { title: string; description?: string; children: React.ReactNode }) { return <section className="grid gap-4 border-t border-slate-200 pt-5 first:border-t-0 first:pt-0"><div><h2 className="font-semibold text-slate-900">{title}</h2>{description && <p className="mt-1 text-sm text-slate-600">{description}</p>}</div><div className="grid gap-4 md:grid-cols-2">{children}</div></section>; }
export function FormActions({ children }: { children: React.ReactNode }) { return <div className="flex flex-wrap items-center gap-3 border-t border-slate-200 pt-5">{children}</div>; }
export function FormErrorSummary({ errors }: { errors: string[] }) { if (!errors.length) return null; return <section className="rounded-control border border-red-200 bg-red-50 p-4 text-sm text-red-900" role="alert" aria-labelledby="form-errors-title"><h2 className="font-semibold" id="form-errors-title">Revise os campos destacados</h2><ul className="mt-2 list-disc space-y-1 pl-5">{errors.map(error => <li key={error}>{error}</li>)}</ul></section>; }
