"use client";
import { useQuery } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { useApi } from "@/providers/providers";
import type { Professional, Specialty, Unit } from "@/lib/api/types";
export default function AppointmentsPage() { const api = useApi(); const data = useQuery({ queryKey: ["appointment-form-data"], queryFn: async () => Promise.all([api.request<Unit[]>("/api/units"), api.request<Specialty[]>("/api/specialties"), api.request<Professional[]>("/api/professionals")]) }); return <ProtectedShell><h1 className="text-2xl font-bold">Agenda</h1><section className="mt-6 rounded bg-white p-5 shadow"><h2 className="font-semibold">Agendamento manual</h2><p className="mt-2 text-slate-600">A API já permite criar, confirmar e cancelar consultas. A interface de seleção e confirmação será adicionada na próxima entrega.</p>{data.isLoading && <p className="mt-3">Carregando dados de agendamento...</p>}{data.data && <p className="mt-3 text-sm">{data.data[0].length} unidades · {data.data[1].length} especialidades · {data.data[2].length} profissionais disponíveis.</p>}</section></ProtectedShell>; }
