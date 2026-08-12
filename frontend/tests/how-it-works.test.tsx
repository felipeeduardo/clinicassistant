import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { HowItWorksSection } from "@/components/landing/how-it-works-section";

describe("how it works flow", () => {
  it("exposes the complete operational flow in linear reading order", () => {
    render(<HowItWorksSection />);
    for (const label of ["Paciente", "WhatsApp", "IA Recepção", "Especialidades", "Profissionais", "Disponibilidade", "Agenda", "Fluxo concluído", "Recepção humana", "Painel da clínica"]) {
      expect(screen.getByRole("heading", { name: label })).toBeVisible();
    }
    expect(screen.getByLabelText("Operações da agenda")).toHaveTextContent("Agendar");
  });
});
