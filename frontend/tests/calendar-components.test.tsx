import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { CalendarEmptyState, RealtimeIndicator } from "@/components/calendar/calendar";

describe("calendar accessibility states", () => {
  it.each([["online", "Connected"], ["connecting", "Reconnecting"], ["offline", "Offline"]] as const)("announces realtime state %s", (status, label) => {
    render(<RealtimeIndicator status={status} />);
    expect(screen.getByLabelText(`Realtime ${label}`)).toBeVisible();
  });

  it("offers a keyboard-accessible create action in the empty state", () => {
    render(<CalendarEmptyState onCreate={() => undefined} />);
    expect(screen.getByRole("button", { name: "Nova consulta" })).toBeVisible();
    expect(screen.getByText("Nenhuma consulta neste período.")).toBeVisible();
  });
});
