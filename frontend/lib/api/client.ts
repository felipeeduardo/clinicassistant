export class ApiError extends Error {
  constructor(public readonly status: number, message: string, public readonly code?: string, public readonly traceId?: string) {
    super(message);
    this.name = "ApiError";
  }

  get isConflict() {
    return this.status === 409;
  }
}

export class ApiClient {
  constructor(
    private readonly token: () => string | null,
    private readonly unauthorized: () => void,
    private readonly baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080",
  ) {}

  async request<T>(path: string, init: RequestInit = {}): Promise<T> {
    const accessToken = this.token();
    const response = await fetch(`${this.baseUrl}${path}`, {
      ...init,
      credentials: "include",
      headers: {
        Accept: "application/json",
        ...(init.body ? { "Content-Type": "application/json" } : {}),
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        ...init.headers,
      },
    });

    if (response.status === 401) this.unauthorized();

    if (!response.ok) {
      const body = await response.json().catch(() => ({}));
      throw new ApiError(response.status, readErrorMessage(body), readProblemField(body, "code"), readProblemField(body, "traceId"));
    }

    return response.status === 204 ? (undefined as T) : (response.json() as Promise<T>);
  }
}

function readProblemField(body: unknown, key: string) {
  if (body && typeof body === "object") { const value = (body as Record<string, unknown>)[key]; return typeof value === "string" ? value : undefined; }
  return undefined;
}

function readErrorMessage(body: unknown) {
  if (body && typeof body === "object") {
    const code = readProblemField(body, "code");
    const safeMessages: Record<string, string> = {
      scheduling_conflict: "O horário não está mais disponível. Atualize a agenda e escolha outro slot.",
      appointment_conflict: "Existe conflito de horário. Atualize a agenda e escolha outro slot.",
      slot_unavailable: "Este horário deixou de estar disponível. Consulte novos horários.",
      professional_unavailable: "O profissional não está disponível neste período.",
      unit_closed: "A unidade está fechada neste período.",
      version_conflict: "A consulta foi alterada por outro usuário. Atualize os dados antes de tentar novamente.",
      appointment_invalid_status: "A consulta não permite esta operação no status atual.",
      clinic_not_ready: "A clínica ainda possui etapas obrigatórias pendentes. Revise o checklist antes de ativar.",
      unauthorized: "Seu usuário não tem permissão para realizar esta operação.",
      resource_not_found: "O recurso solicitado não foi encontrado.",
      invalid_operation: "A operação não pode ser concluída com o estado atual.",
      unexpected_error: "Não foi possível concluir a solicitação.",
    };
    if (code && safeMessages[code]) return safeMessages[code];
    const title = (body as { title?: unknown }).title;
    if (typeof title === "string" && title.trim()) return title;
  }

  return "Não foi possível concluir a solicitação.";
}
