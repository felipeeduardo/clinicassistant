import type { ApiClient } from "@/lib/api/client";
import type { Appointment, AppointmentDetail, AppointmentPage, AppointmentRequest, AvailableSlot } from "@/lib/api/types";

export const appointmentsApi = {
  list: (api: ApiClient, startsAt: string, endsAt: string) => api.request<Appointment[]>(`/api/appointments?startsAt=${encodeURIComponent(startsAt)}&endsAt=${encodeURIComponent(endsAt)}`),
  search: (api: ApiClient, query: { page?: number; pageSize?: number; from?: string; to?: string; professionalId?: string; specialtyId?: string; unitId?: string; patientId?: string; status?: string; source?: string }) => { const params = new URLSearchParams(); Object.entries(query).forEach(([key, value]) => { if (value) params.set(key, String(value)); }); return api.request<AppointmentPage>(`/api/appointments/search?${params}`); },
  detail: (api: ApiClient, id: string) => api.request<AppointmentDetail>(`/api/appointments/${id}`),
  reschedule: (api: ApiClient, id: string, request: { startsAt: string; endsAt: string; expectedVersion: number; notes?: string | null }, idempotencyKey: string) => api.request(`/api/appointments/${id}/reschedule`, { method: "POST", headers: { "Idempotency-Key": idempotencyKey }, body: JSON.stringify(request) }),
  availability: (api: ApiClient, professionalId: string, date: string) => api.request<AvailableSlot[]>(`/api/professionals/${professionalId}/availability?date=${date}`),
  create: (api: ApiClient, request: AppointmentRequest, idempotencyKey: string) => api.request<Appointment>("/api/appointments", { method: "POST", headers: { "Idempotency-Key": idempotencyKey }, body: JSON.stringify(request) }),
  confirm: (api: ApiClient, id: string, expectedVersion: number, idempotencyKey: string) => api.request<Appointment>(`/api/appointments/${id}/confirm`, { method: "POST", headers: { "Idempotency-Key": idempotencyKey }, body: JSON.stringify({ expectedVersion }) }),
  cancel: (api: ApiClient, id: string, reason: string, expectedVersion: number, idempotencyKey: string) => api.request<Appointment>(`/api/appointments/${id}/cancel`, { method: "POST", headers: { "Idempotency-Key": idempotencyKey }, body: JSON.stringify({ reason, expectedVersion }) }),
};
