import { Button } from "./button";
import { Icon } from "./icon";

export function Drawer({ open, title, description, onClose, children }: { open: boolean; title: string; description?: string; onClose: () => void; children: React.ReactNode }) {
  if (!open) return null;
  return <div className="fixed inset-0 z-overlay" role="dialog" aria-modal="true" aria-labelledby="drawer-title">
    <button className="absolute inset-0 bg-slate-950/40" aria-label="Fechar painel ao tocar fora" onClick={onClose} />
    <aside className="absolute inset-y-0 right-0 flex w-full max-w-xl flex-col bg-white shadow-floating">
      <header className="flex items-start justify-between gap-4 border-b border-slate-200 p-5"><div><h2 className="text-lg font-semibold text-slate-950" id="drawer-title">{title}</h2>{description && <p className="mt-1 text-sm text-slate-600">{description}</p>}</div><Button aria-label="Fechar painel" onClick={onClose} size="icon" variant="ghost"><Icon name="close" /></Button></header>
      <div className="min-h-0 flex-1 overflow-y-auto p-5">{children}</div>
    </aside>
  </div>;
}
