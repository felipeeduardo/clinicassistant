import type { Metadata } from "next";
import { publicMetadata } from "@/lib/seo/public";
import { TrackedLink } from "@/components/analytics/tracked-link";

export const metadata: Metadata = publicMetadata("Clínicas pioneiras", "Participe do programa de adoção da IA Recepção e ajude a construir uma recepção mais simples.", "/clinicas-pioneiras");

export default function Page() {
  const href = "/demonstracao?utm_source=clinics_pioneers&utm_medium=landing&utm_campaign=clinics_pioneers";
  return <main className="min-h-screen bg-brand-950 px-5 py-20 text-white"><div className="mx-auto max-w-3xl"><p className="text-sm font-semibold uppercase tracking-[.18em] text-brand-300">Programa de adoção</p><h1 className="mt-4 text-5xl font-semibold tracking-tight">Clínicas pioneiras constroem a próxima recepção.</h1><p className="mt-6 text-xl leading-8 text-slate-300">Um grupo pequeno de clínicas participa de um piloto acompanhado, valida os fluxos da operação e ajuda a priorizar a evolução da IA Recepção.</p><ul className="mt-10 space-y-4 text-lg text-slate-200"><li>✓ Descoberta guiada da rotina e demonstração com seus cenários</li><li>✓ Configuração inicial de agenda, WhatsApp e atendimento</li><li>✓ Canal direto para feedback e acompanhamento do piloto</li><li>✓ Apoio próximo nos primeiros passos da operação</li></ul><TrackedLink href={href} event="pioneers_page_view" properties={{ cta: "pioneer_application" }} className="mt-10 inline-block rounded-control bg-brand-500 px-6 py-3 font-semibold">Quero participar do piloto</TrackedLink></div></main>;
}
