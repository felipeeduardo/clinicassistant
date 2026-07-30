import { describe, expect, it } from "vitest";
import { can } from "@/lib/auth/permissions";
describe("permissions", () => { it("allows catalog management only for clinic admin", () => { expect(can({ id: "1", tenantId: "t", name: "A", email: "a@a.com", role: "ClinicAdmin" }, "manageCatalog")).toBe(true); expect(can({ id: "1", tenantId: "t", name: "A", email: "a@a.com", role: "Receptionist" }, "manageCatalog")).toBe(false); }); });
