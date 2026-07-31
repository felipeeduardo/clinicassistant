import { describe, expect, it } from "vitest";
import { patientSchema } from "@/features/patients/patient-form";

describe("patientSchema", () => {
  it("requires the administrative data accepted by the API", () => {
    expect(patientSchema.safeParse({ name: "Ana", phone: "+550000000000", email: "ana@example.test", birthDate: "1990-01-01", consentStatus: "Granted" }).success).toBe(true);
    expect(patientSchema.safeParse({ name: "", phone: "", email: "invalido", birthDate: "", consentStatus: "" }).success).toBe(false);
  });
});
