import type { ApiClient } from "@/lib/api/client";
import type { Clinic, ClinicRequest, Professional, ProfessionalRequest, Specialty, SpecialtyRequest, Unit, UnitRequest } from "@/lib/api/types";

export const catalogApi = {
  getClinic: (api: ApiClient) => api.request<Clinic>("/api/clinics/current"),
  updateClinic: (api: ApiClient, request: ClinicRequest) => api.request<Clinic>("/api/clinics/current", { method: "PUT", body: JSON.stringify(request) }),
  listUnits: (api: ApiClient) => api.request<Unit[]>("/api/units"),
  createUnit: (api: ApiClient, request: UnitRequest) => api.request<Unit>("/api/units", { method: "POST", body: JSON.stringify(request) }),
  updateUnit: (api: ApiClient, id: string, request: UnitRequest) => api.request<Unit>(`/api/units/${id}`, { method: "PUT", body: JSON.stringify(request) }),
  listSpecialties: (api: ApiClient) => api.request<Specialty[]>("/api/specialties"),
  createSpecialty: (api: ApiClient, request: SpecialtyRequest) => api.request<Specialty>("/api/specialties", { method: "POST", body: JSON.stringify(request) }),
  updateSpecialty: (api: ApiClient, id: string, request: SpecialtyRequest) => api.request<Specialty>(`/api/specialties/${id}`, { method: "PUT", body: JSON.stringify(request) }),
  listProfessionals: (api: ApiClient) => api.request<Professional[]>("/api/professionals"),
  createProfessional: (api: ApiClient, request: ProfessionalRequest) => api.request<Professional>("/api/professionals", { method: "POST", body: JSON.stringify(request) }),
  updateProfessional: (api: ApiClient, id: string, request: ProfessionalRequest) => api.request<Professional>(`/api/professionals/${id}`, { method: "PUT", body: JSON.stringify(request) }),
};
