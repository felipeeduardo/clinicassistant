import "./globals.css";
import { Providers } from "@/providers/providers";
import { publicBrand } from "@/lib/brand/public-brand";

export const metadata = { title: publicBrand.name, description: publicBrand.tagline };
export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) { return <html lang="pt-BR"><body><Providers>{children}</Providers></body></html>; }
