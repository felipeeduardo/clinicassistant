import { describe, expect, it, vi } from "vitest";
import { publicEvents, trackPublicEvent } from "@/lib/analytics/public-events";

describe("public analytics events", () => {
  it("exposes only the documented PII-free event names", () => {
    expect(publicEvents).toContain("product_tab_changed");
    expect(publicEvents).not.toContain("patient_viewed" as never);
  });

  it("dispatches a provider-neutral browser event", () => {
    const listener = vi.fn();
    window.addEventListener("clinicassistant:public-event", listener);
    trackPublicEvent("product_tab_changed", { tab: "Agenda" });
    expect(listener).toHaveBeenCalledOnce();
    expect(listener.mock.calls[0]?.[0]).toMatchObject({ detail: { event: "product_tab_changed", properties: { tab: "Agenda" } } });
    window.removeEventListener("clinicassistant:public-event", listener);
  });
});
