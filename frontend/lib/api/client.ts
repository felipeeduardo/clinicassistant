export class ApiError extends Error { constructor(public readonly status: number, message: string) { super(message); } }
export class ApiClient {
  constructor(private readonly token: () => string | null, private readonly unauthorized: () => void, private readonly baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080") {}
  async request<T>(path: string, init: RequestInit = {}): Promise<T> {
    const accessToken = this.token(); const response = await fetch(`${this.baseUrl}${path}`, { ...init, headers: { "Content-Type": "application/json", ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}), ...init.headers } });
    if (response.status === 401) this.unauthorized();
    if (!response.ok) { const body = await response.json().catch(() => ({})); throw new ApiError(response.status, body.title ?? "Não foi possível concluir a solicitação."); }
    return response.status === 204 ? (undefined as T) : response.json() as Promise<T>;
  }
}
