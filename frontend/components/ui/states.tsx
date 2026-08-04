import { Button } from "./button";
import { Card } from "./surfaces";

export function EmptyState({ title, description, action }: { title: string; description: string; action?: React.ReactNode }) { return <Card className="grid place-items-center gap-2 py-10 text-center"><h2 className="font-semibold text-slate-900">{title}</h2><p className="max-w-md text-sm text-slate-600">{description}</p>{action}</Card>; }
export function ErrorState({ title = "Não foi possível carregar os dados", description = "Tente novamente em alguns instantes.", onRetry }: { title?: string; description?: string; onRetry?: () => void }) { return <Card className="border-red-200 bg-red-50"><h2 className="font-semibold text-red-950">{title}</h2><p className="mt-1 text-sm text-red-800">{description}</p>{onRetry && <Button className="mt-4" variant="outline" onClick={onRetry}>Tentar novamente</Button>}</Card>; }
export function Skeleton({ className = "" }: { className?: string }) { return <div className={`animate-pulse rounded-control bg-slate-200 ${className}`} aria-hidden="true" />; }
