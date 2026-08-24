import type { ApiClient } from "./client";
import type { NotificationSummary, OperationalNotification, PagedResult } from "./types";
export const notificationKeys = { all: ["notifications"] as const, summary: ["notifications", "summary"] as const };
export const notificationsApi = { list: (api: ApiClient, page = 1, pageSize = 20) => api.request<PagedResult<OperationalNotification>>(`/api/notifications?page=${page}&pageSize=${pageSize}`), summary: (api: ApiClient) => api.request<NotificationSummary>("/api/notifications/summary"), read: (api: ApiClient, id: string) => api.request<void>(`/api/notifications/${id}/read`, { method: "POST" }), readAll: (api: ApiClient) => api.request<void>("/api/notifications/read-all", { method: "POST" }) };
