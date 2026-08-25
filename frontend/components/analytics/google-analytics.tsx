"use client";

import Script from "next/script";
import { useEffect } from "react";

declare global { interface Window { dataLayer: unknown[]; gtag?: (...args: unknown[]) => void; } }
const measurementId = process.env.NEXT_PUBLIC_GA_MEASUREMENT_ID;

export function GoogleAnalytics() {
  useEffect(() => {
    if (!measurementId) return;
    window.gtag?.("event", "page_view", { page_location: window.location.href, page_path: window.location.pathname });
    const handler = (event: Event) => {
      const detail = (event as CustomEvent<{ event?: string; properties?: Record<string, string | number | boolean> }>).detail;
      if (detail?.event) window.gtag?.("event", detail.event, detail.properties ?? {});
    };
    window.addEventListener("clinicassistant:public-event", handler);
    return () => window.removeEventListener("clinicassistant:public-event", handler);
  }, []);
  if (!measurementId) return null;
  return <><Script src={`https://www.googletagmanager.com/gtag/js?id=${measurementId}`} strategy="afterInteractive" /><Script id="google-analytics" strategy="afterInteractive">{`window.dataLayer=window.dataLayer||[];function gtag(){dataLayer.push(arguments);}gtag('js',new Date());gtag('config','${measurementId}',{send_page_view:false});`}</Script></>;
}
