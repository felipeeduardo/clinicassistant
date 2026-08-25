import type { Metadata } from "next";

export const siteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";
export function publicMetadata(title: string, description: string, path = "/"): Metadata {
  const url = new URL(path, siteUrl).toString();
  return { metadataBase: new URL(siteUrl), title, description, alternates: { canonical: url }, openGraph: { title, description, url, type: "website", locale: "pt_BR" }, twitter: { card: "summary", title, description }, robots: { index: true, follow: true } };
}
