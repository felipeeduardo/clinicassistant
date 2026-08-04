import type { ApiClient } from "@/lib/api/client";
import type { IntegrationStatus, PagedResult, TwilioConfigurationStatus, WhatsAppTemplate, WhatsAppTemplateDetail } from "@/lib/api/types";

export const whatsAppApi = {
  status: (api: ApiClient) => api.request<IntegrationStatus>("/api/whatsapp/integration/status"),
  twilioConfiguration: (api: ApiClient) => api.request<TwilioConfigurationStatus>("/api/whatsapp/integration/twilio/configuration"),
  templates: (api: ApiClient, search = "") => api.request<PagedResult<WhatsAppTemplate>>(`/api/whatsapp/templates?page=1&pageSize=25&search=${encodeURIComponent(search)}`),
  createTemplate: (api: ApiClient, request: { contentSid: string; name: string; languageCode: string; category?: string; variables?: string[] }) => api.request<WhatsAppTemplate>("/api/whatsapp/templates", { method: "POST", body: JSON.stringify(request) }),
  template: (api: ApiClient, id: string) => api.request<WhatsAppTemplateDetail>(`/api/whatsapp/templates/${id}`),
  updateTemplate: (api: ApiClient, id: string, request: { name: string; languageCode: string; category?: string; variables?: string[] }) => api.request<WhatsAppTemplateDetail>(`/api/whatsapp/templates/${id}`, { method: "PUT", body: JSON.stringify({ name: request.name, languageCode: request.languageCode, category: request.category, variables: request.variables }) }),
  setTemplateStatus: (api: ApiClient, id: string, active: boolean) => api.request<void>(`/api/whatsapp/templates/${id}/${active ? "activate" : "deactivate"}`, { method: "POST" }),
  validate: (api: ApiClient) => api.request<void>("/api/whatsapp/integration/validate", { method: "POST" }),
  enable: (api: ApiClient) => api.request<void>("/api/whatsapp/integration/enable", { method: "POST" }),
  disable: (api: ApiClient) => api.request<void>("/api/whatsapp/integration/disable", { method: "POST" }),
  testMessage: (api: ApiClient) => api.request<void>("/api/whatsapp/integration/test-message", { method: "POST", headers: { "Idempotency-Key": crypto.randomUUID() } }),
};
