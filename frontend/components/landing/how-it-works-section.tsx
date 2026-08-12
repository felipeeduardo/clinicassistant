import { ReceptionMark } from "@/components/brand/reception-mark";
import { Icon, type IconName } from "@/components/ui/icon";

type FlowNodeProps = { id: string; title: string; subtitle: string; description: string; icon: IconName; central?: boolean };

const branches: FlowNodeProps[] = [
  { id: "specialties", title: "Especialidades", subtitle: "Encontra o atendimento", description: "Consulta as especialidades configuradas pela clínica.", icon: "specialties" },
  { id: "professionals", title: "Profissionais", subtitle: "Relaciona profissionais", description: "Apresenta profissionais de acordo com a especialidade.", icon: "professionals" },
  { id: "availability", title: "Disponibilidade", subtitle: "Consulta horários", description: "Consulta horários reais disponíveis.", icon: "signal" },
];

function FlowNode({ id, title, subtitle, description, icon, central = false }: FlowNodeProps) {
  return <article aria-describedby={`${id}-description`} className={`flow-node ${central ? "flow-node-central" : ""}`} tabIndex={0}>
    <div className="flex items-center gap-3">{central ? <ReceptionMark className="size-9 shrink-0" /> : <span className="grid size-9 shrink-0 place-items-center rounded-control bg-brand-50 text-brand-700"><Icon name={icon} /></span>}<div className="min-w-0"><h3 className="font-semibold text-slate-950">{title}</h3><p className="text-xs text-slate-500">{subtitle}</p></div></div>
    <p className="flow-node-description" id={`${id}-description`}>{description}</p>
  </article>;
}

function ConnectorLayer() {
  return <svg aria-hidden="true" className="flow-connectors" fill="none" preserveAspectRatio="none" viewBox="0 0 1000 700"><defs><marker id="flow-arrow" markerHeight="7" markerWidth="7" orient="auto-start-reverse" refX="6" refY="3.5"><path d="m0 0 7 3.5L0 7Z" fill="currentColor" /></marker></defs><g className="flow-connector-paths" markerEnd="url(#flow-arrow)"><path d="M500 88v42" /><path d="M500 230v50M500 280H190v38M500 280h310v38M500 280v38" /><path d="M190 414v35M500 414v35M810 414v35M190 485h620M500 485v36" /><path d="M500 580H260v34M500 580h240v34M500 580v34" /></g></svg>;
}

export function HowItWorksSection() {
  return <div className="how-it-works-flow" aria-label="Fluxo operacional da IA Recepção">
    <ConnectorLayer />
    <div className="flow-stage flow-stage-patient"><FlowNode description="Faz a solicitação." icon="patients" id="patient" subtitle="Faz a solicitação" title="Paciente" /></div>
    <div className="flow-stage flow-stage-whatsapp"><FlowNode description="Canal de atendimento da clínica." icon="messages" id="whatsapp" subtitle="Canal de atendimento" title="WhatsApp" /></div>
    <div className="flow-stage flow-stage-ai"><FlowNode central description="Conduz o fluxo, mantém o contexto e consulta a operação." icon="messages" id="ai" subtitle="Núcleo da operação · Automação" title="IA Recepção" /></div>
    <div className="flow-stage flow-stage-branches">{branches.map(node => <FlowNode key={node.id} {...node} />)}</div>
    <div className="flow-stage flow-stage-agenda"><FlowNode description="Registra e atualiza consultas." icon="calendar" id="agenda" subtitle="Atualiza a consulta" title="Agenda" /><div className="flow-actions" aria-label="Operações da agenda">{["Agendar", "Reagendar", "Cancelar", "Confirmar"].map(action => <span key={action}>{action}</span>)}</div></div>
    <div className="flow-stage flow-stage-outcomes"><FlowNode description="Consulta atualizada e operação concluída." icon="dashboard" id="completed" subtitle="Consulta atualizada" title="Fluxo concluído" /><FlowNode description="Sua equipe assume quando necessário, com o histórico da conversa." icon="messages" id="human" subtitle="Assume quando necessário" title="Recepção humana" /></div>
    <div className="flow-stage flow-stage-panel"><FlowNode description="A operação fica visível para toda a equipe." icon="dashboard" id="panel" subtitle="Agenda · Conversas · Dashboard · Auditoria" title="Painel da clínica" /></div>
  </div>;
}
