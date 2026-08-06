import type { NextConfig } from "next";
import { createSecurityHeaders } from "./lib/security-headers";

const nextConfig: NextConfig = { reactStrictMode: true, poweredByHeader: false, async headers() { return [{ source: "/:path*", headers: createSecurityHeaders(process.env.NEXT_PUBLIC_API_URL) }]; } };
export default nextConfig;
