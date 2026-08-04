import { cleanup, fireEvent, render, screen, within } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { AppShell } from "@/components/app-shell";

vi.mock("next/navigation", () => ({ usePathname: () => "/dashboard" }));

const user = { id: "user-1", tenantId: "tenant-1", name: "Ana Administradora", email: "ana@example.test", role: "ClinicAdmin" };
afterEach(cleanup);

describe("AppShell", () => {
  it("shows only permitted navigation and opens the mobile menu", () => {
    render(<AppShell onLogout={vi.fn()} realtimeStatus="online" user={user}><p>Conteúdo</p></AppShell>);
    expect(screen.getAllByText("Dashboard").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Auditoria").length).toBeGreaterThan(0);
    fireEvent.click(screen.getByRole("button", { name: "Abrir menu" }));
    const drawer = screen.getByRole("dialog", { name: "Menu de navegação" });
    expect(drawer).toBeVisible();
    fireEvent.click(within(drawer).getByRole("button", { name: "Fechar menu" }));
    expect(screen.queryByRole("dialog", { name: "Menu de navegação" })).not.toBeInTheDocument();
  });

  it("does not expose administrative items to a receptionist", () => {
    render(<AppShell onLogout={vi.fn()} realtimeStatus="offline" user={{ ...user, role: "Receptionist" }}><p>Conteúdo</p></AppShell>);
    expect(screen.queryByText("Auditoria")).not.toBeInTheDocument();
    expect(screen.queryByText("Clínica")).not.toBeInTheDocument();
  });
});
