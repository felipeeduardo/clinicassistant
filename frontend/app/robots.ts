import type { MetadataRoute } from "next";

export default function robots(): MetadataRoute.Robots {
  const baseUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";
  return { rules: [{ userAgent: "*", allow: "/", disallow: ["/login", "/dashboard", "/appointments", "/conversations", "/audit", "/platform", "/settings", "/patients", "/professionals", "/specialties", "/units", "/integrations", "/setup"] }], sitemap: `${baseUrl}/sitemap.xml` };
}
