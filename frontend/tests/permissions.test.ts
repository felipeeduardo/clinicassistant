import { describe, expect, it } from "vitest";
import { can } from "@/lib/auth/permissions";

const user = (role: string) => ({ id: "1", tenantId: "t", name: "A", email: "a@a.com", role });

describe("permissions", () => {
  it("allows catalog management only for clinic admin", () => {
    expect(can(user("ClinicAdmin"), "manageCatalog")).toBe(true);
    expect(can(user("PlatformAdmin"), "manageCatalog")).toBe(false);
    expect(can(user("Receptionist"), "manageCatalog")).toBe(false);
    expect(can(user("Professional"), "manageCatalog")).toBe(false);
    expect(can(user("Viewer"), "manageCatalog")).toBe(false);
  });

  it("keeps operational access restricted to clinic staff", () => {
    expect(can(user("ClinicAdmin"), "manageOperations")).toBe(true);
    expect(can(user("Receptionist"), "manageOperations")).toBe(true);
    expect(can(user("Professional"), "manageOperations")).toBe(false);
    expect(can(user("PlatformAdmin"), "manageOperations")).toBe(false);
    expect(can(user("Viewer"), "manageOperations")).toBe(false);
  });
});
