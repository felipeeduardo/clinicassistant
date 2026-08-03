import type { ApiClient } from "@/lib/api/client";
import type { Patient, PatientDetail, PatientPage, PatientRequest } from "@/lib/api/types";

export const patientsApi = {
  list: (api: ApiClient) => api.request<Patient[]>("/api/patients"),
  search: (api: ApiClient, query: { page?: number; pageSize?: number; search?: string; consentStatus?: string }) => {
    const params = new URLSearchParams();
    Object.entries(query).forEach(([key, value]) => { if (value !== undefined && value !== "") params.set(key, String(value)); });
    return api.request<PatientPage>(`/api/patients/search?${params}`);
  },
  detail: (api: ApiClient, id: string) => api.request<PatientDetail>(`/api/patients/${id}`),
  create: (api: ApiClient, request: PatientRequest) => api.request<Patient>("/api/patients", { method: "POST", body: JSON.stringify(request) }),
  update: (api: ApiClient, id: string, request: PatientRequest) => api.request<Patient>(`/api/patients/${id}`, { method: "PUT", body: JSON.stringify(request) }),
};
