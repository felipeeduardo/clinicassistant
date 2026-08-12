"use client";

import { useState } from "react";
import Link from "next/link";
import { Icon, type IconName } from "@/components/ui/icon";

type TrustCard = { title: string; description: string; icon: IconName };
const trustCards: TrustCard[] = [
  { title: "Controle de acesso", description: "Perfis e permissões apoiam a operação da clínica.", icon: "shield" },
  { title: "Isolamento por clínica", description: "Os dados da operação permanecem separados por tenant.", icon: "building" },
  { title: "Atendimento humano", description: "Sua equipe assume a conversa quando necessário.", icon: "userCheck" },
  { title: "Histórico operacional", description: "Ações e contexto ficam disponíveis para acompanhamento.", icon: "history" },
];

export function SecuritySection() {
  return <section className="landing-security bg-slate-900 text-white" id="seguranca"><div className="mx-auto max-w-content px-5 py-20 lg:px-8"><div className="max-w-3xl"><p className="landing-eyebrow text-brand-300">Controle e segurança</p><h2 className="mt-3 text-3xl font-semibold tracking-tight sm:text-4xl">Sua equipe continua no controle.</h2><p className="mt-4 max-w-2xl text-lg leading-8 text-slate-300">A IA Recepção organiza o atendimento sem tirar a operação das mãos da sua equipe. Acesso, histórico, auditoria e atendimento humano continuam fazendo parte do fluxo.</p></div><div className="landing-trust-grid mt-10">{trustCards.map(card => <article className="landing-trust-card" key={card.title}><span className="landing-trust-icon"><Icon name={card.icon} /></span><h3>{card.title}</h3><p>{card.description}</p></article>)}</div></div></section>;
}

const questions = [
  ["O paciente precisa instalar aplicativo?", "Não. A experiência pode acontecer pelo WhatsApp configurado pela clínica."],
  ["O sistema funciona pelo WhatsApp?", "Sim. O fluxo conecta mensagens, disponibilidade, agendamento e acompanhamento da recepção."],
  ["Posso assumir uma conversa manualmente?", "Sim. A fila humana permite que a recepção assuma e acompanhe conversas."],
  ["Posso usar vários profissionais?", "Sim. A agenda organiza profissionais, especialidades, disponibilidade, bloqueios e férias."],
  ["É possível controlar horários e bloqueios?", "Sim. A operação mantém regras de disponibilidade, bloqueios e férias por profissional."],
  ["Como funciona a implantação?", "A configuração começa pela clínica, unidades, profissionais, agenda e integração WhatsApp, com validação operacional."],
] as const;

export function FaqSection() {
  const [openQuestion, setOpenQuestion] = useState<number | null>(null);
  return <section className="bg-white text-slate-950" id="faq"><div className="mx-auto max-w-content px-5 py-20 lg:px-8"><div className="landing-faq-layout"><div className="max-w-sm"><p className="landing-eyebrow text-brand-600">Perguntas frequentes</p><h2 className="mt-3 text-3xl font-semibold tracking-tight sm:text-4xl">O que você precisa saber</h2><p className="mt-4 text-lg leading-8 text-slate-600">Respostas rápidas sobre funcionamento, implantação e rotina da clínica.</p></div><div className="landing-faq-list" aria-label="Perguntas frequentes">{questions.map(([question, answer], index) => { const isOpen = openQuestion === index; const answerId = `faq-answer-${index}`; return <div className={`landing-faq-item ${isOpen ? "is-open" : ""}`} key={question}><button aria-controls={answerId} aria-expanded={isOpen} className="landing-faq-trigger" onClick={() => setOpenQuestion(isOpen ? null : index)} type="button"><span>{question}</span><Icon name={isOpen ? "chevronUp" : "chevronDown"} /></button><div className="landing-faq-answer" hidden={!isOpen} id={answerId}><p>{answer}</p></div></div>; })}</div></div></div></section>;
}

export function FinalCtaSection() {
  return <section className="landing-cta-wrap bg-slate-50"><div className="mx-auto max-w-content px-5 py-16 lg:px-8"><div className="landing-cta-card"><div className="relative z-10 max-w-2xl"><p className="landing-eyebrow text-brand-300">Próximo passo</p><h2 className="mt-3 text-3xl font-semibold tracking-tight text-white sm:text-4xl">Veja a IA Recepção funcionando na rotina da sua clínica.</h2><p className="mt-4 text-lg leading-8 text-slate-300">Conheça na prática como atendimento, agenda e equipe trabalham dentro do mesmo fluxo.</p><Link className="landing-cta-button mt-7" href="mailto:demo@clinicassistant.local?subject=Solicitar%20demonstração">Solicitar demonstração <Icon name="arrowRight" /></Link><span className="mt-3 block text-sm text-slate-400">Uma conversa rápida para entender a rotina da sua clínica.</span></div><div className="landing-cta-flow" aria-hidden="true"><span>WhatsApp</span><i>→</i><span>IA Recepção</span><i>→</i><span>Agenda</span></div></div></div></section>;
}

export function LandingFooter() {
  return <footer className="bg-slate-950 text-slate-400"><div className="mx-auto max-w-content px-5 py-12 lg:px-8"><div className="grid gap-10 sm:grid-cols-2 lg:grid-cols-[1.5fr_1fr_1fr_1fr]"><div><p className="text-lg font-semibold text-white">IA Recepção</p><p className="mt-3 max-w-xs text-sm leading-6">Sua recepção mais inteligente, do WhatsApp à agenda.</p></div><div><h2 className="text-sm font-semibold text-white">Produto</h2><nav className="mt-3 grid gap-2 text-sm" aria-label="Links do produto"><Link className="hover:text-white" href="#como-funciona">Como funciona</Link><Link className="hover:text-white" href="#showcase">Recursos</Link><Link className="hover:text-white" href="#faq">FAQ</Link></nav></div><div><h2 className="text-sm font-semibold text-white">Acesso</h2><nav className="mt-3 grid gap-2 text-sm" aria-label="Links de acesso"><Link className="hover:text-white" href="/login">Entrar na operação</Link></nav></div><div><h2 className="text-sm font-semibold text-white">Contato</h2><nav className="mt-3 grid gap-2 text-sm" aria-label="Links de contato"><Link className="hover:text-white" href="mailto:demo@clinicassistant.local?subject=Solicitar%20demonstração">Solicitar demonstração</Link></nav></div></div><div className="mt-10 border-t border-white/10 pt-5 text-xs">© {new Date().getFullYear()} IA Recepção</div></div></footer>;
}
