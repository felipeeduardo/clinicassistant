import type { ApiClient } from "@/lib/api/client";
import type { AssignableUser, ConversationAppointment, ConversationDetail, ConversationItem, ConversationMessage, HumanQueueItem, PagedResult } from "@/lib/api/types";

export const conversationsApi = {
  list: (api: ApiClient, page: number, search: string) => api.request<PagedResult<ConversationItem>>(`/api/conversations?page=${page}&pageSize=25${search ? `&search=${encodeURIComponent(search)}` : ""}`),
  detail: (api: ApiClient, id: string) => api.request<ConversationDetail>(`/api/conversations/${id}`),
  messages: (api: ApiClient, id: string, page = 1) => api.request<PagedResult<ConversationMessage>>(`/api/conversations/${id}/messages?page=${page}&pageSize=100`),
  markRead: (api: ApiClient, id: string, messageId: string, expectedVersion: number) => api.request<void>(`/api/conversations/${id}/messages/${messageId}/read`, { method: "POST", body: JSON.stringify({ expectedVersion }) }),
  assign: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "assign", expectedVersion),
  release: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "release", expectedVersion),
  pause: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "automation/pause", expectedVersion),
  resume: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "automation/resume", expectedVersion),
  close: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "close", expectedVersion),
  reopen: (api: ApiClient, id: string, expectedVersion: number) => operation(api, id, "reopen", expectedVersion),
  setPriority: (api: ApiClient, id: string, expectedVersion: number, priority: string) => api.request<void>(`/api/conversations/${id}/priority`, { method: "PATCH", body: JSON.stringify({ expectedVersion, priority }) }),
  sendManualMessage: (api: ApiClient, id: string, expectedVersion: number, content: string) => api.request<void>(`/api/conversations/${id}/messages`, { method: "POST", headers: { "Idempotency-Key": crypto.randomUUID() }, body: JSON.stringify({ expectedVersion, content }) }),
  queue: (api: ApiClient) => api.request<PagedResult<HumanQueueItem>>("/api/conversation-queue?page=1&pageSize=25&status=Waiting"),
  transfer: (api: ApiClient, id: string, expectedVersion: number, targetUserId: string, reason?: string) => api.request<void>(`/api/conversations/${id}/transfer`, { method: "POST", body: JSON.stringify({ expectedVersion, targetUserId, reason }) }),
  appointments: (api: ApiClient, id: string) => api.request<ConversationAppointment[]>(`/api/conversations/${id}/appointments`),
  operators: (api: ApiClient) => api.request<AssignableUser[]>("/api/conversations/operators"),
};

function operation(api: ApiClient, id: string, action: string, expectedVersion: number) {
  return api.request<void>(`/api/conversations/${id}/${action}`, { method: "POST", body: JSON.stringify({ expectedVersion }) });
}
