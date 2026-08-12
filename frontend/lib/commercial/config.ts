export type CommercialMode = "demo" | "pilot" | "publicPricing";

export type CommercialConfig = {
  mode: CommercialMode;
  showPricing: boolean;
  showCalculator: boolean;
  ctaLabel: string;
  badge?: string;
  monthlyInvestment?: number;
};

const mode = (process.env.NEXT_PUBLIC_COMMERCIAL_MODE ?? "demo") as CommercialMode;

export const commercialConfig: CommercialConfig = mode === "pilot"
  ? { mode, showPricing: false, showCalculator: true, ctaLabel: "Participar do piloto", badge: "Piloto para clínicas parceiras" }
  : mode === "publicPricing"
    ? { mode, showPricing: Boolean(process.env.NEXT_PUBLIC_PUBLIC_PRICING_APPROVED === "true"), showCalculator: true, ctaLabel: "Solicitar demonstração" }
    : { mode: "demo", showPricing: false, showCalculator: true, ctaLabel: "Solicitar demonstração" };
