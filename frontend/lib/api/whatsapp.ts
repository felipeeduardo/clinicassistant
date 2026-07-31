import type { ApiClient } from "@/lib/api/client";
import type { IntegrationStatus } from "@/lib/api/types";

export const whatsAppApi = {
  status: (api: ApiClient) => api.request<IntegrationStatus>("/api/whatsapp/integration/status"),
};
