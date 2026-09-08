"use client";

import { useRef, useState } from "react";
import { catalogApi } from "@/lib/api/catalog";
import type { ScheduleImportPreview, ScheduleImportResult } from "@/lib/api/types";
import { useApi } from "@/providers/providers";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/surfaces";
import { Drawer } from "@/components/ui/drawer";
import { Icon } from "@/components/ui/icon";

const MAX_FILE_SIZE = 5 * 1024 * 1024;
const typeLabel: Record<string, string> = { availability_rule: "Disponibilidade", schedule_block: "Bloqueio", vacation: "Férias" };

export function ScheduleImportPanel({ mode = "card" }: { mode?: "card" | "button" }) {
  const api = useApi();
  const input = useRef<HTMLInputElement>(null);
  const [open, setOpen] = useState(false);
  const [file, setFile] = useState<File>();
  const [preview, setPreview] = useState<ScheduleImportPreview>();
  const [error, setError] = useState<string>();
  const [loading, setLoading] = useState(false);
  const [confirmed, setConfirmed] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [result, setResult] = useState<ScheduleImportResult>();

  const reset = () => { setFile(undefined); setPreview(undefined); setError(undefined); setConfirmed(false); setConfirming(false); setResult(undefined); if (input.current) input.current.value = ""; };
  const choose = (selected?: File) => {
    if (!selected) return;
    if (!selected.name.toLowerCase().endsWith(".xlsx")) { setError("Escolha uma planilha XLSX."); return; }
    if (selected.size > MAX_FILE_SIZE) { setError("O arquivo excede o limite de 5 MB."); return; }
    setFile(selected); setPreview(undefined); setError(undefined); setConfirmed(false); setResult(undefined);
  };
  const validate = async () => {
    if (!file) return;
    setLoading(true); setError(undefined);
    try { setPreview(await catalogApi.previewScheduleImport(api, file)); } catch (cause) { setError(cause instanceof Error ? cause.message : "Não foi possível validar o arquivo."); } finally { setLoading(false); }
  };
  const commit = async () => {
    if (!file || !preview || preview.errors.length > 0) return;
    setLoading(true); setError(undefined);
    try { setResult(await catalogApi.commitScheduleImport(api, file, crypto.randomUUID())); setConfirmed(true); setConfirming(false); } catch (cause) { setError(cause instanceof Error ? cause.message : "Não foi possível concluir a importação."); } finally { setLoading(false); }
  };
  const downloadTemplate = async () => {
    try { const blob = await catalogApi.downloadScheduleImportTemplate(api); const url = URL.createObjectURL(blob); const anchor = document.createElement("a"); anchor.href = url; anchor.download = "modelo-importacao-agenda-ia-recepcao.xlsx"; anchor.click(); URL.revokeObjectURL(url); } catch (cause) { setError(cause instanceof Error ? cause.message : "Não foi possível baixar o modelo."); }
  };

  const trigger = mode === "button" ? <Button onClick={() => { reset(); setOpen(true); }} variant="outline"><Icon name="arrowUp" />Importar agenda</Button> : <Card className="flex flex-wrap items-center justify-between gap-4 p-5">
      <div><h2 className="font-semibold text-slate-950">Importar agenda dos profissionais</h2><p className="mt-1 max-w-2xl text-sm text-slate-600">Configure horários recorrentes, bloqueios e férias usando uma planilha modelo, com revisão antes de salvar.</p><div className="mt-3 flex flex-wrap gap-3 text-xs text-slate-500"><span><Icon className="mr-1 inline size-4" name="download" />Baixe o modelo</span><span><Icon className="mr-1 inline size-4" name="edit" />Preencha em português</span><span><Icon className="mr-1 inline size-4" name="shield" />Revise antes de confirmar</span></div></div>
      <div className="flex flex-wrap gap-2"><Button onClick={() => void downloadTemplate()} size="sm" variant="outline"><Icon name="download" />Baixar modelo</Button><Button onClick={() => { reset(); setOpen(true); }} size="sm"><Icon name="arrowUp" />Importar agenda</Button></div>
    </Card>;

  return <>
    {trigger}
    {error && !open && <p className="mt-3 rounded-control bg-red-50 p-3 text-sm text-red-800" role="alert">{error}</p>}
    <Drawer description="Planilha em português · máximo de 5 MB e 5.000 linhas" onClose={() => setOpen(false)} open={open} title="Importar agenda">
      <div className="mb-6 grid grid-cols-4 gap-2 text-center text-xs"><Step active={!file} done={Boolean(file)} label="Modelo" number="1" /><Step active={Boolean(file) && !preview} done={Boolean(preview)} label="Arquivo" number="2" /><Step active={Boolean(preview) && !confirmed} done={confirmed} label="Revisão" number="3" /><Step active={confirmed} done={confirmed} label="Conclusão" number="4" /></div>
      {!file && <section className="grid gap-4"><div className="rounded-xl border border-blue-100 bg-blue-50 p-4 text-sm text-blue-950"><p className="font-medium">Como funciona?</p><p className="mt-1">Baixe o modelo, preencha Disponibilidade, Bloqueio ou Férias, envie o arquivo e revise o preview. Nada é alterado até você confirmar.</p></div><UploadDropzone input={input} onSelect={choose} /></section>}
      {file && !preview && <section className="grid gap-4"><FileCard file={file} onRemove={reset} /><Button disabled={!file} loading={loading} onClick={() => void validate()}><Icon name="shield" />Validar arquivo</Button><p className="text-xs text-slate-500">Datas: dd/mm/aaaa · Horas: HH:mm · Consultas existentes nunca são alteradas.</p></section>}
      {preview && !confirmed && <Preview preview={preview} error={error} loading={loading} onConfirm={() => setConfirming(true)} onBack={() => setPreview(undefined)} />}
      {confirming && preview && <div className="rounded-xl border border-blue-200 bg-blue-50 p-4" role="alertdialog" aria-modal="true"><p className="font-semibold text-slate-950">Confirmar importação?</p><p className="mt-1 text-sm text-slate-700">{preview.availabilityRows ?? 0} disponibilidades, {preview.blockRows ?? 0} bloqueios e {preview.vacationRows ?? 0} períodos de férias serão incluídos. Consultas existentes não serão alteradas.</p><div className="mt-4 flex justify-end gap-2"><Button onClick={() => setConfirming(false)} size="sm" variant="ghost">Cancelar</Button><Button loading={loading} onClick={() => void commit()} size="sm">Confirmar importação</Button></div></div>}
      {confirmed && result && <section className="grid gap-4"><div className="rounded-xl border border-emerald-200 bg-emerald-50 p-5"><p className="font-semibold text-emerald-900">Importação concluída</p><p className="mt-1 text-sm text-emerald-800">{result.createdRules} disponibilidades, {result.createdBlocks} bloqueios e {result.createdVacations} períodos de férias cadastrados.</p></div><div className="flex flex-wrap gap-2"><Button href="/audit?resourceType=ScheduleImport" variant="outline">Ver histórico</Button><Button onClick={() => setOpen(false)} variant="ghost">Fechar</Button></div></section>}
      {error && <p className="mt-4 rounded-control bg-red-50 p-3 text-sm text-red-800" role="alert">{error}</p>}
    </Drawer>
  </>;
}

function Step({ number, label, active, done }: { number: string; label: string; active: boolean; done: boolean }) { return <div className={active || done ? "text-blue-700" : "text-slate-400"}><span className={`mx-auto flex size-7 items-center justify-center rounded-full text-xs font-semibold ${done ? "bg-emerald-100 text-emerald-700" : active ? "bg-blue-100 text-blue-700" : "bg-slate-100"}`}>{done ? "✓" : number}</span><span className="mt-1 block">{label}</span></div>; }

function UploadDropzone({ input, onSelect }: { input: React.RefObject<HTMLInputElement | null>; onSelect: (file?: File) => void }) { const [dragging, setDragging] = useState(false); return <button className={`grid min-h-48 place-items-center rounded-xl border-2 border-dashed p-6 text-center transition ${dragging ? "border-blue-500 bg-blue-50" : "border-slate-300 bg-slate-50 hover:border-blue-400"}`} onClick={() => input.current?.click()} onDragEnter={event => { event.preventDefault(); setDragging(true); }} onDragLeave={() => setDragging(false)} onDragOver={event => event.preventDefault()} onDrop={event => { event.preventDefault(); setDragging(false); onSelect(event.dataTransfer.files?.[0]); }} type="button"><span><Icon className="mx-auto size-9 text-blue-600" name="arrowUp" /><span className="mt-2 block font-medium text-slate-800">Arraste sua planilha aqui</span><span className="mt-1 block text-sm text-slate-500">ou clique para selecionar · XLSX até 5 MB</span></span><input ref={input} accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" className="sr-only" onChange={event => onSelect(event.target.files?.[0])} type="file" /></button>; }
function FileCard({ file, onRemove }: { file: File; onRemove: () => void }) { return <div className="flex items-center justify-between gap-3 rounded-xl border border-slate-200 bg-white p-4"><div className="flex items-center gap-3"><Icon className="text-blue-600" name="sheet" /><div><p className="font-medium text-slate-900">{file.name}</p><p className="text-xs text-slate-500">{Math.max(1, Math.round(file.size / 1024))} KB</p></div></div><Button onClick={onRemove} size="sm" variant="ghost">Remover</Button></div>; }
function Preview({ preview, loading, onConfirm, onBack }: { preview: ScheduleImportPreview; error?: string; loading: boolean; onConfirm: () => void; onBack: () => void }) { const [filter, setFilter] = useState<"all" | "valid" | "error">("all"); const errorsByRow = new Map(preview.errors.map(item => [item.row, item])); const rows = preview.rows.map((row, index) => ({ row, number: index + 2, issue: errorsByRow.get(index + 2) })).filter(item => filter === "all" || (filter === "error" ? item.issue : !item.issue)); return <section className="grid gap-4"><div><p className="text-sm text-slate-600">{preview.fileName ? preview.fileName : "Arquivo validado"}</p><h3 className="mt-1 text-lg font-semibold text-slate-950">Pré-visualização da importação</h3></div><div className="grid grid-cols-2 gap-2 sm:grid-cols-5"><Summary label="Total" value={preview.totalRows} /><Summary label="Válidos" value={preview.validRows} tone="ok" /><Summary label="Disponibilidades" value={preview.availabilityRows ?? 0} /><Summary label="Bloqueios" value={preview.blockRows ?? 0} /><Summary label="Férias" value={preview.vacationRows ?? 0} /></div><div className="flex flex-wrap gap-2"><Button onClick={() => setFilter("all")} size="sm" variant={filter === "all" ? "outline" : "ghost"}>Todos</Button><Button onClick={() => setFilter("valid")} size="sm" variant={filter === "valid" ? "outline" : "ghost"}>Válidos</Button><Button onClick={() => setFilter("error")} size="sm" variant={filter === "error" ? "outline" : "ghost"}>Erros ({preview.errors.length})</Button></div><div className="overflow-x-auto rounded-xl border border-slate-200"><table className="min-w-[720px] table-fixed text-left text-xs"><thead className="bg-slate-50 text-slate-600"><tr>{["Status", "Profissional", "Tipo", "Dia/Data", "Início", "Fim", "Duração", "Motivo"].map(label => <th className="px-3 py-2 font-medium" key={label}>{label}</th>)}</tr></thead><tbody>{rows.slice(0, 100).map(({ row, number, issue }) => <tr className="border-t border-slate-100" key={number}><td className="w-16 px-3 py-2">{issue ? <span className="text-red-600" title={issue.message}>✕ Linha {number}</span> : <span className="text-emerald-600">✓</span>}</td><td className="w-28 break-words px-3 py-2">{row.professionalRegistration}</td><td className="w-28 break-words px-3 py-2">{typeLabel[row.recordType] ?? row.recordType}</td><td className="w-32 break-words px-3 py-2">{row.dayOfWeek ?? ([row.startDate, row.endDate].filter(Boolean).join(" a ") || "—")}</td><td className="w-20 px-3 py-2">{row.startTime ?? "—"}</td><td className="w-20 px-3 py-2">{row.endTime ?? "—"}</td><td className="w-20 px-3 py-2">{row.slotDurationMinutes ? `${row.slotDurationMinutes} min` : "—"}</td><td className="w-48 break-words px-3 py-2">{issue ? "Corrija o valor destacado" : row.reason ?? "—"}</td></tr>)}</tbody></table></div>{preview.errors.length > 0 && <div className="rounded-xl border border-red-200 bg-red-50 p-3 text-sm text-red-800"><p className="font-medium">Corrija os erros destacados antes de confirmar.</p><ul className="mt-2 grid gap-1">{preview.errors.slice(0, 8).map(item => <li className="break-words" key={`${item.row}-${item.field}`}>Linha {item.row}: {item.message}</li>)}</ul></div>}<div className="flex flex-wrap justify-between gap-2"><Button onClick={onBack} variant="ghost">Escolher outro arquivo</Button><Button disabled={preview.errors.length > 0} loading={loading} onClick={onConfirm}><Icon name="shield" />Confirmar importação</Button></div></section>; }
function Summary({ label, value, tone }: { label: string; value: number; tone?: "ok" }) { return <div className={`rounded-xl border p-3 ${tone === "ok" ? "border-emerald-100 bg-emerald-50" : "border-slate-200 bg-slate-50"}`}><p className="text-xs text-slate-600">{label}</p><p className="mt-1 text-xl font-semibold text-slate-950">{value}</p></div>; }
