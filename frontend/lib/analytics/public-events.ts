export const publicEvents = [
  "page_view",
  "cta_demo_click",
  "lead_form_view",
  "lead_form_start",
  "lead_form_submit",
  "demo_page_view",
  "pricing_view",
  "pioneers_page_view",
  "landing_view",
  "hero_demo_started",
  "hero_demo_completed",
  "product_tab_changed",
  "pricing_viewed",
  "pricing_cta_clicked",
  "roi_calculator_started",
  "roi_calculator_completed",
  "demo_cta_clicked",
  "pilot_cta_clicked",
  "lead_started",
  "lead_submitted",
  "lead_failed",
  "demo_cta_click",
] as const;

export type PublicEvent = (typeof publicEvents)[number];
export type PublicEventProperties = Record<string, string | number | boolean>;

/** Provider-neutral and PII-free until an analytics vendor is approved. */
export function trackPublicEvent(event: PublicEvent, properties: PublicEventProperties = {}) {
  if (typeof window === "undefined") return;
  window.dispatchEvent(new CustomEvent("clinicassistant:public-event", { detail: { event, properties } }));
}
