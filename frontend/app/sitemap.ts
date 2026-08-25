import type { MetadataRoute } from "next";

export default function sitemap(): MetadataRoute.Sitemap {
  const baseUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";
  const paths = ["/", "/demonstracao", "/demo", "/recepcionista-ia-para-clinicas", "/agendamento-whatsapp-para-clinicas", "/automacao-whatsapp-clinica", "/clinicas-pioneiras", "/conteudos", "/conteudos/como-reduzir-faltas-com-confirmacao-automatica", "/conteudos/whatsapp-na-recepcao-de-clinicas", "/conteudos/agenda-online-e-atendimento", "/conteudos/ia-na-recepcao-com-controle-humano", "/conteudos/como-escolher-uma-ia-para-clinicas"]; return paths.map(path => ({ url: new URL(path, baseUrl).toString(), changeFrequency: "monthly" as const, priority: path === "/" ? 1 : 0.7 }));
}
