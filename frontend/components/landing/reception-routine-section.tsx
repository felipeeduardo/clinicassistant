import { Icon, type IconName } from "@/components/ui/icon";

const requests = ["Tem horário amanhã?", "Qual médico atende cardiologia?", "Quero remarcar minha consulta.", "Pode cancelar?"];
const timeline = [
  ["Solicitação recebida", "A conversa chega pelo WhatsApp.", "messages"],
  ["Informações consultadas", "Agenda, profissionais e disponibilidade.", "calendar"],
  ["Paciente orientado", "A próxima etapa fica clara para o paciente.", "signal"],
  ["Equipe acompanha", "A recepção assume quando necessário.", "audit"],
] as const satisfies ReadonlyArray<readonly [string, string, IconName]>;

function RoutineBenefit({ icon, children }: { icon: IconName; children: string }) {
  return <li className="flex items-center gap-2 text-sm text-slate-700"><span className="grid size-6 place-items-center rounded-full bg-emerald-100 text-emerald-700"><Icon name={icon} /></span>{children}</li>;
}

export function ReceptionRoutineSection() {
  return <div className="reception-routine">
    <div className="reception-routine-grid">
      <article className="routine-card routine-before" aria-labelledby="routine-before-title">
        <div><p className="routine-eyebrow text-brand-300">Antes</p><h3 className="mt-2 text-xl font-semibold text-white" id="routine-before-title">Solicitações chegando uma após a outra.</h3></div>
        <div className="routine-messages">{requests.map((request, index) => <div className="routine-message" key={request}><span className="grid size-7 shrink-0 place-items-center rounded-full bg-white/10 text-slate-300"><Icon name="messages" /></span><span>{request}</span><small>{index % 2 === 0 ? "agora" : "há pouco"}</small></div>)}</div>
        <p className="mt-auto border-t border-white/10 pt-4 text-sm text-slate-300">Cada solicitação exige consulta e resposta manual.</p>
      </article>
      <div className="routine-connector" aria-hidden="true"><span>organiza o fluxo</span><b>→</b></div>
      <article className="routine-card routine-after" aria-labelledby="routine-after-title">
        <div><p className="routine-eyebrow text-brand-700">Com IA Recepção</p><h3 className="mt-2 text-xl font-semibold text-slate-950" id="routine-after-title">Um fluxo organizado, com sua equipe no controle.</h3><p className="mt-3 text-sm leading-6 text-slate-600">As solicitações mais comuns seguem etapas claras, enquanto a recepção acompanha e assume quando necessário.</p></div>
        <ol className="routine-timeline" aria-label="Etapas do fluxo organizado">{timeline.map(([title, description, icon]) => <li key={title}><span className="routine-timeline-icon"><Icon name={icon} /></span><div><strong>{title}</strong><p>{description}</p></div></li>)}</ol>
        <ul className="mt-auto grid gap-2 border-t border-brand-100 pt-4"><RoutineBenefit icon="signal">Paciente recebe orientação</RoutineBenefit><RoutineBenefit icon="calendar">Agenda atualizada quando uma operação é concluída</RoutineBenefit><RoutineBenefit icon="audit">Recepção mantém o contexto</RoutineBenefit></ul>
      </article>
    </div>
    <div className="mt-8 flex flex-wrap items-center justify-center gap-3 text-center"><p className="text-base font-medium text-slate-700">A IA Recepção organiza o trabalho repetitivo. Sua equipe continua no controle.</p><div className="flex flex-wrap justify-center gap-2" aria-label="Capacidades da operação"><span className="routine-trust-chip">Agenda integrada</span><span className="routine-trust-chip">Handoff humano</span><span className="routine-trust-chip">Histórico operacional</span></div></div>
  </div>;
}
