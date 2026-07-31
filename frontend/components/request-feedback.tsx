import { ApiError } from "@/lib/api/client";

export function RequestFeedback({ error }: { error: unknown }) {
  if (!error) return null;
  const message = error instanceof ApiError && error.isConflict
    ? "Este registro foi alterado por outra operação. Atualize a página e tente novamente."
    : error instanceof Error ? error.message : "Não foi possível concluir a solicitação.";
  return <p className="mt-3 rounded border border-red-200 bg-red-50 p-3 text-sm text-red-800" role="alert">{message}</p>;
}

export function LoadingRow({ columns = 1 }: { columns?: number }) {
  return <tr><td className="p-3 text-slate-600" colSpan={columns}>Carregando...</td></tr>;
}
