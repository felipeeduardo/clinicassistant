"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { calculateOperationalImpact, type ValueCalculatorInputs } from "@/lib/commercial/value-calculator";
import { commercialConfig } from "@/lib/commercial/config";

const currency = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
const number = new Intl.NumberFormat("pt-BR", { maximumFractionDigits: 1 });
const pricing = commercialConfig.pricing;

const fields: ReadonlyArray<[keyof ValueCalculatorInputs, string, number, number]> = [
  ["attendants", "Atendentes", 1, 20],
  ["monthlyTeamCost", "Custo mensal da equipe (R$)", 0, 100000],
  ["requestsPerDay", "Solicitações repetitivas/dia", 0, 1000],
  ["minutesPerRequest", "Minutos por solicitação", 0, 120],
  ["workDays", "Dias úteis/mês", 1, 31],
  ["automatablePercent", "Percentual potencialmente automatizável (%)", 0, 100],
];

export function ValueCalculator() {
  const [inputs, setInputs] = useState<ValueCalculatorInputs>({ attendants: 2, monthlyTeamCost: 6000, requestsPerDay: 30, minutesPerRequest: 4, workDays: 22, automatablePercent: 40, monthlyInvestment: pricing.monthlyPrice });
  const result = useMemo(() => calculateOperationalImpact({ ...inputs, monthlyInvestment: pricing.monthlyPrice }), [inputs]);
  const update = (key: keyof ValueCalculatorInputs) => (event: React.ChangeEvent<HTMLInputElement>) => setInputs(current => ({ ...current, [key]: Number(event.target.value) }));
  const positiveBalance = result.timeValue > pricing.monthlyPrice;

  return <section className="bg-white text-slate-950" id="impacto">
    <div className="mx-auto max-w-content px-5 py-20 lg:px-8">
      <div className="max-w-3xl">
        <p className="text-sm font-semibold uppercase tracking-[.18em] text-brand-600">Investimento e retorno operacional</p>
        <h2 className="mt-3 text-3xl font-semibold tracking-tight sm:text-4xl">Quanto tempo sua recepção poderia recuperar todos os meses?</h2>
        <p className="mt-4 text-lg leading-8 text-slate-600">Simule o impacto das tarefas repetitivas na rotina da sua clínica e compare esse tempo com o investimento no IA Recepção.</p>
      </div>

      <div className="mt-10 grid gap-6 rounded-panel border border-slate-200 bg-slate-50 p-6 shadow-sm lg:grid-cols-[.8fr_1.2fr] lg:p-8">
        <div className="rounded-panel border border-slate-200 bg-white p-6">
          <p className="text-sm font-semibold uppercase tracking-[.16em] text-brand-600">Plano principal</p>
          <div className="mt-3 flex flex-wrap items-baseline gap-x-3 gap-y-1"><h3 className="text-xl font-semibold">{pricing.planName}</h3><span className="text-sm text-slate-500">{pricing.planLabel}</span></div>
          <p className="mt-5 text-3xl font-semibold tracking-tight text-slate-950">{currency.format(pricing.monthlyPrice)}<span className="ml-1 text-base font-medium text-slate-500">/mês</span></p>
          <p className="mt-2 text-sm text-slate-600">+ {currency.format(pricing.setupPrice)} de implantação, pagamento inicial</p>
          <ul className="mt-6 grid gap-2 text-sm text-slate-700 sm:grid-cols-2">{pricing.benefits.map(benefit => <li className="flex gap-2" key={benefit}><span className="font-semibold text-emerald-600" aria-hidden="true">✓</span><span>{benefit}</span></li>)}</ul>
          <Link className="mt-7 inline-flex rounded-control bg-brand-600 px-5 py-3 font-semibold text-white transition hover:-translate-y-0.5 hover:bg-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2" href="/demonstracao">Solicitar demonstração</Link>
        </div>

        <div>
          <p className="text-sm font-semibold uppercase tracking-[.16em] text-brand-600">Simule sua operação</p>
          <div className="mt-4 grid gap-4 sm:grid-cols-2">{fields.map(([key, label, min, max]) => <label className="grid gap-1 text-sm font-medium text-slate-700" key={key}>{label}<input aria-label={label} className="h-11 rounded-control border border-slate-300 bg-white px-3 text-slate-950 transition focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-200" min={min} max={max} onChange={update(key)} step="any" type="number" value={inputs[key]} /></label>)}</div>
        </div>
      </div>

      <div className="mt-6 rounded-panel border border-brand-dark-border bg-brand-dark p-6 text-white lg:p-8">
        <p className="text-sm uppercase tracking-[.18em] text-brand-300">Impacto operacional estimado</p>
        <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-4" aria-live="polite">
          <Metric label="Horas potencialmente liberadas/mês" value={`${number.format(result.releasedHours)} h`} />
          <Metric label="Valor equivalente do tempo" value={currency.format(result.timeValue)} />
          <Metric label="Investimento IA Recepção" value={`${currency.format(pricing.monthlyPrice)}/mês`} />
          <Metric label="Saldo operacional estimado" value={currency.format(result.estimatedImpact)} />
        </div>
        <p className="mt-6 rounded-control border border-brand-dark-border bg-brand-dark-surface p-4 text-sm leading-6 text-slate-200">{positiveBalance ? "Com essas premissas, o valor estimado do tempo potencialmente liberado supera o investimento mensal no IA Recepção." : "Nesta simulação, o principal ganho está na organização e disponibilidade da operação. Ajuste as premissas para refletir a realidade da sua clínica."}</p>
        <p className="mt-4 text-xs leading-5 text-slate-400">Esta simulação é uma estimativa baseada nas informações fornecidas e não representa garantia de economia ou retorno financeiro. Os resultados reais dependem do volume de atendimentos, adesão, configuração e rotina operacional de cada clínica.</p>
      </div>
    </div>
  </section>;
}

function Metric({ label, value }: { label: string; value: string }) { return <div className="rounded-control border border-brand-dark-border bg-brand-dark-surface p-4 transition hover:border-brand-400"><span className="block text-sm text-slate-300">{label}</span><strong className="mt-2 block text-xl text-white sm:text-2xl">{value}</strong></div>; }
