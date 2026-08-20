import type { ApiClient } from "@/lib/api/client";
import type { DemoLeadDetail, DemoLeadPage, DemoLeadSummary, OnboardTenantRequest, OnboardTenantResponse, PlatformClinic, PlatformDashboard, PlatformOnboardingStatus, PlatformTenant, PlatformUser, PlatformWhatsAppStatus } from "@/lib/api/types";

export const platformApi = {
  tenants: (api: ApiClient) => api.request<PlatformTenant[]>("/api/platform/tenants"),
  users: (api: ApiClient) => api.request<PlatformUser[]>("/api/platform/users"),
  clinics: (api: ApiClient) => api.request<PlatformClinic[]>("/api/platform/clinics"),
  dashboard: (api: ApiClient, period = "30d") => api.request<PlatformDashboard>(`/api/platform/dashboard?period=${period}`),
  onboardingStatus: (api: ApiClient, tenantId: string) => api.request<PlatformOnboardingStatus>(`/api/platform/onboarding/${tenantId}`),
  whatsappStatus: (api: ApiClient, tenantId: string) => api.request<PlatformWhatsAppStatus>(`/api/platform/tenants/${tenantId}/whatsapp`),
  createClinicAdmin: (api: ApiClient, tenantId: string, request: { name: string; email: string; temporaryPassword: string }, idempotencyKey: string) => api.request<PlatformUser>(`/api/platform/tenants/${tenantId}/clinic-admins`, { method: "POST", headers: { "Idempotency-Key": idempotencyKey }, body: JSON.stringify(request) }),
  status: (api: ApiClient, id: string, action: "activate" | "suspend" | "disable") => api.request<void>(`/api/platform/tenants/${id}/${action}`, { method: "POST" }),
  deleteTenant: (api: ApiClient, tenantId: string, request: { clinicAdminEmail: string; confirmation: string }) => api.request<void>(`/api/platform/tenants/${tenantId}/purge`, { method: "POST", body: JSON.stringify(request) }),
  onboard: (api: ApiClient, request: OnboardTenantRequest, idempotencyKey: string) => api.request<OnboardTenantResponse>("/api/platform/onboarding", { method: "POST", headers: { "Idempotency-Key": idempotencyKey }, body: JSON.stringify(request) }),
  leads: (api: ApiClient, query = "") => api.request<DemoLeadPage>(`/api/platform/leads${query ? `?${query}` : ""}`),
  leadSummary: (api: ApiClient) => api.request<DemoLeadSummary>("/api/platform/leads/summary"),
  lead: (api: ApiClient, id: string) => api.request<DemoLeadDetail>(`/api/platform/leads/${id}`),
  leadStatus: (api: ApiClient, id: string, status: string) => api.request<void>(`/api/platform/leads/${id}/status`, { method: "POST", body: JSON.stringify({ status }) }),
  leadAssign: (api: ApiClient, id: string, userId: string | null) => api.request<void>(`/api/platform/leads/${id}/assignment`, { method: "POST", body: JSON.stringify({ userId }) }),
  leadNote: (api: ApiClient, id: string, note: string) => api.request<void>(`/api/platform/leads/${id}/notes`, { method: "POST", body: JSON.stringify({ note }) }),
};
