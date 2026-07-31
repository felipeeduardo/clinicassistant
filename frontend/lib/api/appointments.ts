import type { ApiClient } from "@/lib/api/client";
import type { Appointment, AppointmentRequest, AvailableSlot } from "@/lib/api/types";

export const appointmentsApi = {
  list: (api: ApiClient, startsAt: string, endsAt: string) => api.request<Appointment[]>(`/api/appointments?startsAt=${encodeURIComponent(startsAt)}&endsAt=${encodeURIComponent(endsAt)}`),
  availability: (api: ApiClient, professionalId: string, date: string) => api.request<AvailableSlot[]>(`/api/professionals/${professionalId}/availability?date=${date}`),
  create: (api: ApiClient, request: AppointmentRequest) => api.request<Appointment>("/api/appointments", { method: "POST", body: JSON.stringify(request) }),
  confirm: (api: ApiClient, id: string) => api.request<Appointment>(`/api/appointments/${id}/confirm`, { method: "POST" }),
  cancel: (api: ApiClient, id: string, reason: string) => api.request<Appointment>(`/api/appointments/${id}/cancel`, { method: "POST", body: JSON.stringify({ reason }) }),
};
