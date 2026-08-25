"use client";

import Link from "next/link";
import { ProtectedShell } from "@/components/protected-shell";
import { PageContainer, PageHeader } from "@/components/ui/page";
import { Card, StatCard, StatusBadge } from "@/components/ui/surfaces";
import { Button } from "@/components/ui/button";
import { Icon } from "@/components/ui/icon";
import { useAuth } from "@/providers/providers";
import { commercialConfig, formatCommercialCurrency } from "@/lib/commercial/config";

export default function PlatformMonetizationPage() {
  const { user } = useAuth();
  if (user?.role !== "PlatformAdmin") return <ProtectedShell><PageContainer><PageHeader pathname="/platform/monetization" title="Acesso negado" description="Monetização é exclusiva da administração da plataforma." /></PageContainer></ProtectedShell>;
  const pricing = commercialConfig.pricing;
  return <ProtectedShell><PageContainer><PageHeader pathname="/platform/monetization" title="Monetização" description="Referência comercial da oferta atual, sem simular billing ou dados financeiros inexistentes." actions={<Link href="/platform/wiki"><Button variant="outline"><Icon name="history" />Consultar Wiki</Button></Link>} />
    <section className="mt-6 grid gap-4 md:grid-cols-3"><StatCard label="Plano atual" value={pricing.planLabel} description={pricing.planName}><Icon className="text-brand-600" name="building" /></StatCard><StatCard label="Mensalidade" value={formatCommercialCurrency(pricing.monthlyPrice)} description="Configuração comercial compartilhada"><Icon className="text-brand-600" name="history" /></StatCard><StatCard label="Implantação" value={formatCommercialCurrency(pricing.setupPrice)} description="Pagamento inicial"><Icon className="text-brand-600" name="arrowRight" /></StatCard></section>
    <section className="mt-6 grid gap-6 lg:grid-cols-2"><Card><div className="flex items-start justify-between gap-3"><div><h2 className="font-semibold text-slate-950">Oferta apresentada</h2><p className="mt-1 text-sm text-slate-600">Os limites abaixo são posicionamento comercial, não enforcement de billing.</p></div><StatusBadge tone="info">Referência</StatusBadge></div><ul className="mt-5 grid gap-2 text-sm text-slate-700 sm:grid-cols-2">{pricing.benefits.map(item => <li className="flex gap-2" key={item}><span className="text-emerald-600">✓</span>{item}</li>)}</ul></Card><Card><h2 className="font-semibold text-slate-950">Indicadores financeiros</h2><p className="mt-1 text-sm text-slate-600">Nenhum billing automático está conectado a esta versão.</p><div className="mt-5 grid gap-3 sm:grid-cols-2"><Metric label="MRR" value="Não disponível" /><Metric label="Ticket médio" value="Aguardando dados reais" /><Metric label="Margem estimada" value="Aguardando custos reais" /><Metric label="Clientes pagantes" value="Não disponível" /></div><p className="mt-5 text-xs leading-5 text-slate-500">Não confundir o preço configurado ou a calculadora de impacto da landing com receita reconhecida, contrato ou garantia de retorno.</p></Card></section>
    <Card className="mt-6 border-brand-200 bg-brand-50"><h2 className="font-semibold text-brand-950">Próxima decisão</h2><p className="mt-1 text-sm text-brand-900">Quando houver fonte financeira aprovada, integrar billing e derivar MRR, margem e status de cobrança. Até lá, mantenha os valores como referência manual.</p><Link className="mt-4 inline-flex text-sm font-semibold text-brand-800 hover:underline" href="/platform/leads">Acompanhar oportunidades comerciais →</Link></Card>
  </PageContainer></ProtectedShell>;
}

function Metric({ label, value }: { label: string; value: string }) { return <div className="rounded-control bg-slate-50 p-3"><p className="text-xs text-slate-500">{label}</p><p className="mt-1 text-sm font-semibold text-slate-900">{value}</p></div>; }
