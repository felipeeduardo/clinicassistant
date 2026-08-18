import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { FaqSection, FinalCtaSection, LandingFooter, SecuritySection } from "@/components/landing/closing-sections";

describe("landing closing sections", () => {
  it("communicates governance without absolute security claims", () => {
    render(<SecuritySection />);
    expect(screen.getByRole("heading", { name: "Sua equipe continua no controle." })).toBeVisible();
    for (const title of ["Controle de acesso", "Isolamento por clínica", "Atendimento humano", "Histórico operacional"]) expect(screen.getByRole("heading", { name: title })).toBeVisible();
    expect(screen.queryByText(/100% seguro|segurança garantida|LGPD compliant/i)).not.toBeInTheDocument();
  });

  it("opens one FAQ answer with accessible state", () => {
    render(<FaqSection />);
    const trigger = screen.getByRole("button", { name: "O paciente precisa instalar aplicativo?" });
    expect(trigger).toHaveAttribute("aria-expanded", "false");
    fireEvent.click(trigger);
    expect(trigger).toHaveAttribute("aria-expanded", "true");
    expect(screen.getByText("Não. A experiência pode acontecer pelo WhatsApp configurado pela clínica.")).toBeVisible();
  });

  it("offers a clear final conversion and footer navigation", () => {
    render(<><FinalCtaSection /><LandingFooter /></>);
    expect(screen.getByRole("heading", { name: "Veja a IA Recepção funcionando na rotina da sua clínica." })).toBeVisible();
    // The primary conversion goes through the validated demo-request form.
    expect(screen.getAllByRole("link", { name: /Solicitar demonstração/ })[0]).toHaveAttribute("href", "/demonstracao");
    expect(screen.getByRole("link", { name: "Entrar na operação" })).toHaveAttribute("href", "/login");
  });
});
