export type CommercialMode = "demo" | "pilot" | "publicPricing";

export type CommercialConfig = {
  mode: CommercialMode;
  showPricing: boolean;
  showCalculator: boolean;
  ctaLabel: string;
  badge?: string;
  pricing: CommercialPricing;
};

export type CommercialPricing = {
  planName: string;
  planLabel: string;
  monthlyPrice: number;
  setupPrice: number;
  benefits: readonly string[];
};

export const formatCommercialCurrency = (value: number) => new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);

/** Single source of truth for the public commercial presentation. */
const clinicPricing: CommercialPricing = {
  planName: "IA Recepção",
  planLabel: "Plano Clínica",
  monthlyPrice: 497,
  setupPrice: 990,
  benefits: [
    "Atendimento pelo WhatsApp",
    "Consulta de disponibilidade",
    "Agendamento, reagendamento e cancelamento",
    "Agenda integrada",
    "Fila e atendimento humano",
    "Painel operacional",
    "Histórico das conversas",
    "Até 10 profissionais",
    "1 unidade",
    "1 número de WhatsApp",
  ],
};

const mode = (process.env.NEXT_PUBLIC_COMMERCIAL_MODE ?? "demo") as CommercialMode;

export const commercialConfig: CommercialConfig = mode === "pilot"
  ? { mode, showPricing: false, showCalculator: true, ctaLabel: "Participar do piloto", badge: "Piloto para clínicas parceiras", pricing: clinicPricing }
  : mode === "publicPricing"
    ? { mode, showPricing: Boolean(process.env.NEXT_PUBLIC_PUBLIC_PRICING_APPROVED === "true"), showCalculator: true, ctaLabel: "Solicitar demonstração", pricing: clinicPricing }
    : { mode: "demo", showPricing: false, showCalculator: true, ctaLabel: "Solicitar demonstração", pricing: clinicPricing };
