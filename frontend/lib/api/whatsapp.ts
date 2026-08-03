import type { ApiClient } from "@/lib/api/client";
import type { IntegrationStatus } from "@/lib/api/types";

export const whatsAppApi = {
  status: (api: ApiClient) => api.request<IntegrationStatus>("/api/whatsapp/integration/status"),
  validate: (api: ApiClient) => api.request<void>("/api/whatsapp/integration/validate", { method: "POST" }),
  enable: (api: ApiClient) => api.request<void>("/api/whatsapp/integration/enable", { method: "POST" }),
  disable: (api: ApiClient) => api.request<void>("/api/whatsapp/integration/disable", { method: "POST" }),
  testMessage: (api: ApiClient) => api.request<void>("/api/whatsapp/integration/test-message", { method: "POST", headers: { "Idempotency-Key": crypto.randomUUID() } }),
};
