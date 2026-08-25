import "./globals.css";
import { Providers } from "@/providers/providers";
import { publicBrand } from "@/lib/brand/public-brand";
import { GoogleAnalytics } from "@/components/analytics/google-analytics";

export const metadata = { metadataBase: new URL(process.env.NEXT_PUBLIC_SITE_URL ?? publicBrand.appUrl), title: { default: publicBrand.name, template: `%s | ${publicBrand.name}` }, description: publicBrand.tagline, openGraph: { siteName: publicBrand.name, locale: "pt_BR", type: "website" }, twitter: { card: "summary" }, icons: { icon: "/favicon.ico" } };
export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) { return <html lang="pt-BR"><body><GoogleAnalytics /><Providers>{children}</Providers></body></html>; }
