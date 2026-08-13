import type { Config } from "tailwindcss";

export default {
  content: ["./app/**/*.{ts,tsx}", "./components/**/*.{ts,tsx}", "./features/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        brand: { 50: "#eff6ff", 100: "#dbeafe", 500: "#2563eb", 600: "#1d4ed8", 700: "#1e40af", 900: "#172554" },
        surface: { DEFAULT: "#ffffff", muted: "#f8fafc", subtle: "#f1f5f9" },
        canvas: "var(--color-background)",
        foreground: { DEFAULT: "var(--color-foreground)", muted: "var(--color-foreground-muted)", dark: "var(--color-foreground-on-dark)" },
        "brand-dark": { DEFAULT: "var(--color-brand-dark)", surface: "var(--color-brand-dark-surface)", border: "var(--color-brand-dark-border)" },
        "brand-primary": { DEFAULT: "var(--color-brand-primary)", hover: "var(--color-brand-primary-hover)", subtle: "var(--color-brand-primary-subtle)" },
        "border-system": { DEFAULT: "var(--color-border)", strong: "var(--color-border-strong)", dark: "var(--color-border-on-dark)" },
      },
      borderRadius: { control: "0.625rem", panel: "0.875rem" },
      boxShadow: { panel: "0 1px 3px rgb(15 23 42 / 0.08), 0 1px 2px rgb(15 23 42 / 0.04)", floating: "0 12px 32px rgb(15 23 42 / 0.14)" },
      maxWidth: { content: "90rem" },
      transitionDuration: { ui: "160ms" },
      zIndex: { navigation: "40", overlay: "50", toast: "60" },
    },
  },
  plugins: [],
} satisfies Config;
