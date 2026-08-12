export const publicBrand = {
  name: "IA Recepção",
  technicalName: "Clinic Assistant",
  domain: process.env.NEXT_PUBLIC_BRAND_DOMAIN ?? "iarecepcao.com.br",
  tagline: "Recepção inteligente para clínicas.",
  supportEmail: process.env.NEXT_PUBLIC_SUPPORT_EMAIL ?? "demo@clinicassistant.local",
  appUrl: process.env.NEXT_PUBLIC_APP_URL ?? "http://localhost:3000",
} as const;
