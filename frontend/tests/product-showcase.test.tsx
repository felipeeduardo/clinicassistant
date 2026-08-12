import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ProductShowcase } from "@/components/landing/product-showcase";

describe("landing product showcase", () => {
  it("changes surface through accessible tabs", () => {
    render(<ProductShowcase />);
    const agendaTab = screen.getByRole("tab", { name: "Agenda" });
    expect(screen.getByRole("tabpanel")).toHaveAttribute("aria-live", "polite");
    fireEvent.click(agendaTab);
    expect(screen.getByRole("tabpanel")).toHaveTextContent("app.clinicassistant.local / agenda");
    expect(agendaTab).toHaveAttribute("aria-selected", "true");
  });
});
