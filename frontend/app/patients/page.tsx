"use client";
import { useQuery } from "@tanstack/react-query";
import { ProtectedShell } from "@/components/protected-shell";
import { useApi } from "@/providers/providers";
import type { Patient } from "@/lib/api/types";
export default function PatientsPage() { const api = useApi(); const patients = useQuery({ queryKey: ["patients"], queryFn: () => api.request<Patient[]>("/api/patients") }); return <ProtectedShell><h1 className="text-2xl font-bold">Pacientes</h1><div className="mt-6 overflow-x-auto rounded bg-white shadow"><table className="w-full text-left"><thead><tr className="border-b"><th className="p-3">Nome</th><th className="p-3">Telefone</th><th className="p-3">Consentimento</th></tr></thead><tbody>{patients.data?.map(patient => <tr className="border-b" key={patient.id}><td className="p-3">{patient.name}</td><td className="p-3">{patient.phone}</td><td className="p-3">{patient.consentStatus}</td></tr>)}</tbody></table>{patients.isLoading && <p className="p-3">Carregando...</p>}{patients.isError && <p className="p-3" role="alert">Não foi possível carregar pacientes.</p>}</div></ProtectedShell>; }
