import type { ApiClient } from "@/lib/api/client";
import type { ConversationDetail, ConversationItem, ConversationMessage, PagedResult } from "@/lib/api/types";

export const conversationsApi = {
  list: (api: ApiClient, page: number, search: string) => api.request<PagedResult<ConversationItem>>(`/api/conversations?page=${page}&pageSize=25${search ? `&search=${encodeURIComponent(search)}` : ""}`),
  detail: (api: ApiClient, id: string) => api.request<ConversationDetail>(`/api/conversations/${id}`),
  messages: (api: ApiClient, id: string) => api.request<PagedResult<ConversationMessage>>(`/api/conversations/${id}/messages?page=1&pageSize=100`),
  markRead: (api: ApiClient, id: string, messageId: string, expectedVersion: number) => api.request<void>(`/api/conversations/${id}/messages/${messageId}/read`, { method: "POST", body: JSON.stringify({ expectedVersion }) }),
  assign: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "assign", expectedVersion),
  release: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "release", expectedVersion),
  pause: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "automation/pause", expectedVersion),
  resume: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "automation/resume", expectedVersion),
};

function operation(api: ApiClient, id: string, action: string, expectedVersion: number) {
  return api.request<void>(`/api/conversations/${id}/${action}`, { method: "POST", body: JSON.stringify({ expectedVersion }) });
}
