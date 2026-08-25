"use client";

import { useMemo, useState } from "react";
import { ProtectedShell } from "@/components/protected-shell";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatusBadge } from "@/components/ui/surfaces";
import { Icon } from "@/components/ui/icon";
import { useAuth } from "@/providers/providers";
import { findWikiDocument, wikiCategories, wikiDocuments, type WikiCategory, type WikiDocument } from "@/lib/platform/wiki";

const statusLabels = { Current: "Atual", NeedsReview: "Revisar", Deprecated: "Obsoleto" } as const;

export default function PlatformWikiPage() {
  const { user } = useAuth();
  const [category, setCategory] = useState<WikiCategory | "Todos">("Todos");
  const [search, setSearch] = useState("");
  const [selectedSlug, setSelectedSlug] = useState("visao-produto");
  const documents = useMemo(() => wikiDocuments.filter(document => {
    const matchesCategory = category === "Todos" || document.category === category;
    const haystack = `${document.title} ${document.description} ${document.tags.join(" ")} ${document.content}`.toLocaleLowerCase("pt-BR");
    return matchesCategory && haystack.includes(search.trim().toLocaleLowerCase("pt-BR"));
  }), [category, search]);
  const selected = findWikiDocument(selectedSlug) ?? documents[0];

  if (user?.role !== "PlatformAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/platform/wiki" title="Acesso negado" description="A Wiki interna é exclusiva da administração da plataforma." /></PageContainer></ProtectedShell>;
  return <ProtectedShell><PageContainer>
    <PageHeader pathname="/platform/wiki" title="Wiki da plataforma" description="Conhecimento técnico, comercial e de implantação revisado para operar a IA Recepção." />
    <div className="mt-6 grid gap-5 lg:grid-cols-[16rem_minmax(0,1fr)]">
      <Card className="h-fit p-4 lg:sticky lg:top-24"><label className="grid gap-2 text-sm font-medium text-slate-700" htmlFor="wiki-search">Buscar na Wiki<input id="wiki-search" aria-label="Buscar na Wiki" className="h-10 rounded-control border border-border-system px-3 text-sm" placeholder="Título, tag ou conteúdo" value={search} onChange={event => setSearch(event.target.value)} /></label><div className="mt-5 grid gap-1" role="listbox" aria-label="Categorias da Wiki"><button className={`rounded-control px-3 py-2 text-left text-sm ${category === "Todos" ? "bg-brand-50 font-semibold text-brand-900" : "text-slate-600 hover:bg-slate-50"}`} onClick={() => setCategory("Todos")} type="button">Todos os documentos</button>{wikiCategories.map(item => <button className={`rounded-control px-3 py-2 text-left text-sm ${category === item ? "bg-brand-50 font-semibold text-brand-900" : "text-slate-600 hover:bg-slate-50"}`} key={item} onClick={() => setCategory(item)} type="button">{item}</button>)}</div><p className="mt-5 border-t border-border-system pt-4 text-xs leading-5 text-slate-500">Documentos continuam versionados no Git. Esta central apresenta apenas conteúdo revisado e seguro para consulta interna.</p></Card>
      <div className="grid min-w-0 gap-5 xl:grid-cols-[minmax(14rem,.7fr)_minmax(0,1.3fr)]"><Card className="h-fit p-2"><div className="px-3 py-3 text-xs font-semibold uppercase tracking-wider text-slate-500">{documents.length} documento(s)</div><div className="grid gap-1">{documents.map(document => <button className={`rounded-control p-3 text-left ${selected?.slug === document.slug ? "bg-brand-50" : "hover:bg-slate-50"}`} key={document.slug} onClick={() => setSelectedSlug(document.slug)} type="button"><span className="flex items-start justify-between gap-2"><strong className="text-sm text-slate-900">{document.title}</strong><StatusBadge tone={document.status === "Current" ? "success" : document.status === "NeedsReview" ? "warning" : "neutral"}>{statusLabels[document.status]}</StatusBadge></span><span className="mt-1 block text-xs text-slate-500">{document.category} · {document.description}</span></button>)}{!documents.length && <p className="p-4 text-sm text-slate-500">Nenhum documento corresponde à busca.</p>}</div></Card>{selected ? <WikiDocumentView document={selected} onSelect={setSelectedSlug} /> : <Card><p className="text-sm text-slate-500">Selecione um documento.</p></Card>}</div>
    </div>
  </PageContainer></ProtectedShell>;
}

function WikiDocumentView({ document, onSelect }: { document: WikiDocument; onSelect: (slug: string) => void }) {
  return <Card className="min-w-0"><div className="flex flex-wrap items-start justify-between gap-3 border-b border-border-system pb-4"><div><p className="text-xs font-semibold uppercase tracking-wider text-brand-600">Wiki · {document.category}</p><h2 className="mt-1 text-2xl font-semibold text-slate-950">{document.title}</h2><p className="mt-1 text-sm text-slate-600">{document.description}</p></div><StatusBadge tone={document.status === "Current" ? "success" : document.status === "NeedsReview" ? "warning" : "neutral"}>{statusLabels[document.status]}</StatusBadge></div><div className="mt-4 flex flex-wrap gap-2 text-xs text-slate-500"><span>Atualizado em {new Date(`${document.updatedAt}T12:00:00`).toLocaleDateString("pt-BR")}</span><span>·</span><span>{document.sourcePath}</span>{document.tags.map(tag => <span className="rounded-full bg-slate-100 px-2 py-1" key={tag}>#{tag}</span>)}</div><article className="prose prose-slate mt-7 max-w-none text-sm leading-6">{renderMarkdown(document.content)}</article><div className="mt-8 border-t border-border-system pt-4"><p className="text-xs font-semibold uppercase tracking-wider text-slate-500">Documentos relacionados</p><div className="mt-2 flex flex-wrap gap-2">{document.related.map(slug => <button className="inline-flex items-center gap-1 rounded-control border border-border-system px-3 py-2 text-xs font-medium text-brand-800 hover:bg-brand-50" key={slug} onClick={() => onSelect(slug)} type="button"><Icon name="arrowRight" />{findWikiDocument(slug)?.title ?? slug}</button>)}</div></div></Card>;
}

function renderMarkdown(markdown: string) {
  return markdown.split("\n").map((line, index) => {
    if (line.startsWith("# ")) return <h3 className="mt-0 text-xl font-semibold text-slate-950" key={index}>{line.slice(2)}</h3>;
    if (line.startsWith("## ")) return <h4 className="mt-6 text-base font-semibold text-slate-950" key={index}>{line.slice(3)}</h4>;
    if (line.startsWith("- ")) return <li className="ml-5 list-disc" key={index}>{line.slice(2)}</li>;
    if (!line.trim()) return <div className="h-2" key={index} />;
    return <p key={index}>{line}</p>;
  });
}
