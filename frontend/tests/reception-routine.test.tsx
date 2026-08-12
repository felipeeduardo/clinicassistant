import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ReceptionRoutineSection } from "@/components/landing/reception-routine-section";

describe("reception routine section", () => {
  it("communicates the manual routine and organized handoff", () => {
    render(<ReceptionRoutineSection />);
    expect(screen.getByRole("heading", { name: "Solicitações chegando uma após a outra." })).toBeVisible();
    expect(screen.getByRole("heading", { name: "Um fluxo organizado, com sua equipe no controle." })).toBeVisible();
    for (const request of ["Tem horário amanhã?", "Qual médico atende cardiologia?", "Quero remarcar minha consulta.", "Pode cancelar?"]) expect(screen.getByText(request)).toBeVisible();
    expect(screen.getByText("A IA Recepção organiza o trabalho repetitivo. Sua equipe continua no controle.")).toBeVisible();
    expect(screen.getByText("Handoff humano")).toBeVisible();
  });
});
