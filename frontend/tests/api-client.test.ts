import { describe, expect, it, vi } from "vitest";
import { ApiClient, ApiError } from "@/lib/api/client";

describe("ApiClient", () => {
  it("sends the access token and calls logout on 401", async () => {
    const unauthorized = vi.fn();
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify({ title: "Unauthorized" }), { status: 401, headers: { "Content-Type": "application/json" } })));
    await expect(new ApiClient(() => "token", unauthorized, "http://api.test").request("/protected")).rejects.toBeInstanceOf(ApiError);
    expect(unauthorized).toHaveBeenCalledOnce();
    expect(fetch).toHaveBeenCalledWith("http://api.test/protected", expect.objectContaining({ headers: expect.objectContaining({ Authorization: "Bearer token" }) }));
  });

  it("does not expose a server error detail", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify({ title: "Request failed", detail: "stack trace must not reach the UI" }), { status: 400, headers: { "Content-Type": "application/json" } })));
    await expect(new ApiClient(() => null, () => undefined, "http://api.test").request("/bad")).rejects.toMatchObject({ message: "Request failed" });
  });
});
