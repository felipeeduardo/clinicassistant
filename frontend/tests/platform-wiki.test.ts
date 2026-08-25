import { describe, expect, it } from "vitest";
import { findWikiDocument, wikiCategories, wikiDocuments } from "@/lib/platform/wiki";

describe("PlatformAdmin wiki registry", () => {
  it("contains reviewed technical, business and implementation documents", () => {
    expect(wikiCategories).toEqual(expect.arrayContaining(["Técnica", "Negócio", "Implantação"]));
    expect(wikiDocuments.some(document => document.category === "Técnica")).toBe(true);
    expect(wikiDocuments.some(document => document.category === "Negócio")).toBe(true);
    expect(wikiDocuments.some(document => document.category === "Implantação")).toBe(true);
  });

  it("keeps source metadata and does not include credential-shaped content", () => {
    expect(wikiDocuments.every(document => document.sourcePath.startsWith("docs/"))).toBe(true);
    expect(wikiDocuments.every(document => !/auth.?token\s*[:=]|password\s*[:=]|connectionstring\s*[:=]/i.test(document.content))).toBe(true);
    expect(findWikiDocument("onboarding-clinica")?.status).toBe("Current");
  });
});
