import type { Metadata } from "next";
import { publicMetadata } from "@/lib/seo/public";
import DemoPageClient from "./page-client";

export const metadata: Metadata = publicMetadata("Veja uma conversa com a IA Recepção", "Uma simulação de atendimento pelo WhatsApp para conhecer a experiência.", "/demo");
export default function Page() { return <DemoPageClient />; }
