import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ProductShowcase } from "@/components/landing/product-showcase";

describe("landing product showcase", () => {
  it("changes surface through accessible tabs", () => {
    render(<ProductShowcase />);
    const agendaTab = screen.getByRole("tab", { name: "Agenda" });
    expect(screen.getByRole("tabpanel")).toHaveAttribute("aria-live", "polite");
    fireEvent.click(agendaTab);
    expect(screen.getByRole("tabpanel")).toHaveTextContent("app.iarecepcao.com.br / agenda");
    expect(agendaTab).toHaveAttribute("aria-selected", "true");
  });

  it("does not expose commercial investment in the public showcase", () => {
    render(<ProductShowcase />);
    expect(screen.queryByRole("heading", { name: "Uma proposta comercial transparente." })).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Solicitar proposta" })).not.toBeInTheDocument();
  });

});
