import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { OperationalResultsSection } from "@/components/landing/operational-results-section";

describe("operational results section", () => {
  it("presents four distinct operational benefits", () => {
    render(<OperationalResultsSection />);
    for (const title of ["Atendimento organizado", "Agenda centralizada", "IA + Equipe", "Visibilidade operacional"]) expect(screen.getByRole("heading", { name: title })).toBeVisible();
    expect(screen.getByText("A IA Recepção organiza o fluxo. Sua equipe continua no controle.")).toBeVisible();
    expect(screen.getByLabelText("Exemplo de agenda centralizada")).toBeVisible();
  });
});
