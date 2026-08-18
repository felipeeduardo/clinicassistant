"use client";

import { useEffect } from "react";

/** Adds a single, accessible reveal to major landing sections without moving copy into the client bundle. */
export function LandingMotion() {
  useEffect(() => {
    document.documentElement.classList.add("landing-motion-ready");
    const elements = Array.from(document.querySelectorAll<HTMLElement>(".landing-reveal"));
    if (elements.length === 0) return;

    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (reducedMotion || !("IntersectionObserver" in window)) {
      elements.forEach((element) => element.classList.add("is-visible"));
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add("is-visible");
          observer.unobserve(entry.target);
        }
      }),
      { threshold: 0.12, rootMargin: "0px 0px -8%" },
    );

    elements.forEach((element) => observer.observe(element));
    return () => observer.disconnect();
  }, []);

  return null;
}
