import { Icon, type IconName } from "@/components/ui/icon";

type Benefit = { id: string; number: string; title: string; description: string; icon: IconName; highlights: string[]; visual: "conversation" | "calendar" | "handoff" | "dashboard" };

const benefits: Benefit[] = [
  { id: "organized", number: "01", title: "Atendimento organizado", description: "Conversas seguem um fluxo com contexto e histórico para que a equipe saiba o que está acontecendo.", icon: "messages", highlights: ["Contexto preservado", "Histórico acessível", "Menos troca de telas"], visual: "conversation" },
  { id: "calendar", number: "02", title: "Agenda centralizada", description: "Disponibilidade, agendamentos e alterações ficam conectados à mesma operação.", icon: "calendar", highlights: ["Horários organizados", "Atualizações na agenda", "Menos consultas manuais"], visual: "calendar" },
  { id: "team", number: "03", title: "IA + Equipe", description: "A IA Recepção conduz os fluxos suportados e sua equipe assume quando o atendimento humano é necessário.", icon: "professionals", highlights: ["Handoff humano", "Contexto preservado", "Equipe no controle"], visual: "handoff" },
  { id: "visibility", number: "04", title: "Visibilidade operacional", description: "Agenda, conversas, fila e indicadores tornam mais simples acompanhar o que precisa de atenção.", icon: "dashboard", highlights: ["Dashboard", "Fila humana", "Histórico operacional"], visual: "dashboard" },
];

function MiniVisual({ variant }: { variant: Benefit["visual"] }) {
  if (variant === "conversation") return <div className="benefit-visual benefit-visual-conversation" aria-label="Exemplo de conversa organizada"><span>Paciente · Tem horário amanhã?</span><b>IA Recepção · Encontrei opções</b></div>;
  if (variant === "calendar") return <div className="benefit-visual benefit-visual-calendar" aria-label="Exemplo de agenda centralizada"><span>Hoje</span><b>09:00 · Confirmada</b><i>10:30 · Disponível</i><em>14:00 · Pendente</em></div>;
  if (variant === "handoff") return <div className="benefit-visual benefit-visual-handoff" aria-label="Exemplo de handoff humano"><span>IA Recepção conduz</span><b>↓ precisa de humano</b><i>Equipe assume · contexto preservado</i></div>;
  return <div className="benefit-visual benefit-visual-dashboard" aria-label="Exemplo de visibilidade operacional"><span>Atendimentos</span><div aria-hidden="true"><i /><i /><i /><i /><i /></div><b>✓ acompanhamento da equipe</b></div>;
}

function OperationalBenefitCard({ benefit }: { benefit: Benefit }) {
  return <article className={`operational-benefit operational-benefit-${benefit.id}`} aria-labelledby={`benefit-${benefit.id}`}>
    <div className="flex items-start justify-between gap-3"><span className="grid size-10 place-items-center rounded-control bg-brand-50 text-brand-700"><Icon name={benefit.icon} /></span><span className="operational-benefit-number" aria-hidden="true">{benefit.number}</span></div>
    <h3 className="mt-5 text-xl font-semibold text-slate-950" id={`benefit-${benefit.id}`}>{benefit.title}</h3>
    <p className="mt-2 max-w-xl text-sm leading-6 text-slate-600">{benefit.description}</p>
    <ul className="mt-5 grid gap-2">{benefit.highlights.map(item => <li className="flex items-center gap-2 text-xs font-medium text-slate-700" key={item}><span className="text-emerald-600" aria-hidden="true">✓</span>{item}</li>)}</ul>
    <MiniVisual variant={benefit.visual} />
  </article>;
}

export function OperationalResultsSection() {
  return <div className="operational-results"><div className="operational-results-grid">{benefits.map(benefit => <OperationalBenefitCard benefit={benefit} key={benefit.id} />)}</div><p className="operational-results-closing">A IA Recepção organiza o fluxo. Sua equipe continua no controle.</p></div>;
}
