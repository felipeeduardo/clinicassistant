import type { NextConfig } from "next";
const apiOrigin = new URL(process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080").origin;
const wsOrigin = apiOrigin.replace(/^http/, "ws");
const securityHeaders = [
  { key: "X-Content-Type-Options", value: "nosniff" },
  { key: "X-Frame-Options", value: "DENY" },
  { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
  { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=()" },
  { key: "Content-Security-Policy", value: `default-src 'self'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; connect-src 'self' ${apiOrigin} ${wsOrigin}` },
];
const nextConfig: NextConfig = { reactStrictMode: true, poweredByHeader: false, async headers() { return [{ source: "/:path*", headers: securityHeaders }]; } };
export default nextConfig;
