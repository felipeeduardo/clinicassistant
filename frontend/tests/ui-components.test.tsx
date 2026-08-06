import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Button } from "@/components/ui/button";
import { FormErrorSummary, FormField, Input } from "@/components/ui/form";
import { Drawer } from "@/components/ui/drawer";
import { EmptyState, Skeleton } from "@/components/ui/states";
import { StatusBadge } from "@/components/ui/surfaces";

describe("design system foundation", () => {
  it("prevents duplicate submissions while a button is loading", () => {
    render(<Button loading>Salvar</Button>);
    expect(screen.getByRole("button", { name: "Salvar" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Salvar" })).toHaveAttribute("aria-busy", "true");
  });

  it("connects form feedback and accessible states", () => {
    render(<><FormField label="Nome" htmlFor="name" hint="Use o nome completo." required error="Informe o nome."><Input id="name" required /></FormField><FormErrorSummary errors={["Informe o nome."]} /></>);
    expect(screen.getByRole("textbox")).toBeRequired();
    expect(screen.getByText("Nome").closest("label")).toHaveAttribute("for", "name");
    expect(screen.getByRole("textbox")).toHaveAttribute("aria-describedby", "name-hint name-error");
    expect(screen.getByRole("textbox")).toHaveAttribute("aria-invalid", "true");
    expect(screen.getAllByRole("alert")).toHaveLength(2);
  });

  it("closes a drawer with Escape", () => {
    const onClose = vi.fn();
    render(<Drawer onClose={onClose} open title="Detalhe"><p>Conteúdo</p></Drawer>);
    fireEvent.keyDown(screen.getByRole("dialog"), { key: "Escape" });
    expect(onClose).toHaveBeenCalledOnce();
  });

  it("renders reusable status and empty states", () => {
    render(<><StatusBadge tone="success">Ativa</StatusBadge><EmptyState title="Sem pacientes" description="Cadastre o primeiro paciente." /><Skeleton className="h-8" /></>);
    expect(screen.getByText("Ativa")).toHaveClass("bg-emerald-100");
    expect(screen.getByText("Sem pacientes")).toBeVisible();
  });
});
