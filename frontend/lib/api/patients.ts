import type { ApiClient } from "@/lib/api/client";
import type { Patient, PatientRequest } from "@/lib/api/types";

export const patientsApi = {
  list: (api: ApiClient) => api.request<Patient[]>("/api/patients"),
  create: (api: ApiClient, request: PatientRequest) => api.request<Patient>("/api/patients", { method: "POST", body: JSON.stringify(request) }),
  update: (api: ApiClient, id: string, request: PatientRequest) => api.request<Patient>(`/api/patients/${id}`, { method: "PUT", body: JSON.stringify(request) }),
};
