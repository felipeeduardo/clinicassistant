import "./globals.css";
import { Providers } from "@/providers/providers";
export const metadata = { title: "Clinic Assistant", description: "Operação clínica" };
export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) { return <html lang="pt-BR"><body><Providers>{children}</Providers></body></html>; }
