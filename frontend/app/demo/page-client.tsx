"use client";

import Link from "next/link";
import { useEffect } from "react";
import { trackPublicEvent } from "@/lib/analytics/public-events";

export default function DemoPageClient() {
  useEffect(() => { trackPublicEvent("demo_page_view"); }, []);
  return <main className="min-h-screen bg-slate-100 px-5 py-16 text-slate-950"><div className="mx-auto max-w-2xl"><p className="text-sm font-semibold uppercase tracking-[.18em] text-brand-600">Demonstração simulada</p><h1 className="mt-3 text-4xl font-semibold">Uma conversa curta, uma operação mais organizada.</h1><div className="mt-10 space-y-4 rounded-panel bg-slate-900 p-5 text-white shadow-floating"><div className="max-w-[85%] rounded-panel bg-white/10 p-4">Olá! Como posso ajudar?</div><div className="ml-auto max-w-[75%] rounded-panel bg-brand-600 p-4">Quero agendar uma consulta.</div><div className="max-w-[85%] rounded-panel bg-white/10 p-4">Encontrei horários disponíveis. Qual dia fica melhor para você?</div><div className="ml-auto max-w-[45%] rounded-panel bg-brand-600 p-4">Amanhã</div><div className="max-w-[85%] rounded-panel bg-emerald-400/20 p-4 text-emerald-100">Consulta confirmada ✓</div></div><p className="mt-6 text-slate-600">Esta é uma simulação, sem conexão com dados ou infraestrutura da sua clínica.</p><Link href="/demonstracao?utm_source=demo&utm_medium=landing&utm_campaign=demo" onClick={() => trackPublicEvent("demo_cta_click", { cta: "real_demo" })} className="mt-6 inline-block rounded-control bg-brand-600 px-5 py-3 font-semibold text-white">Solicitar uma demonstração real</Link></div></main>;
}
