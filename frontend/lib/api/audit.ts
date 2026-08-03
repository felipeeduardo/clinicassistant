import type { ApiClient } from "@/lib/api/client";
import type { AuditItem, PagedResult } from "@/lib/api/types";

export const auditApi = {
  search: (api: ApiClient, query: { page?: number; pageSize?: number; action?: string; resourceType?: string; result?: string }) => {
    const params = new URLSearchParams(); Object.entries(query).forEach(([key, value]) => { if (value) params.set(key, String(value)); });
    return api.request<PagedResult<AuditItem>>(`/api/audit?${params}`);
  },
};
