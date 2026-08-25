import { describe, expect, it } from "vitest";
import { publicArticles } from "@/lib/content/public-content";

describe("public SEO content", () => {
  it("keeps the five strategic articles addressable", () => {
    expect(publicArticles).toHaveLength(5);
    expect(new Set(publicArticles.map(article => article.slug)).size).toBe(5);
    expect(publicArticles.every(article => article.title && article.description && article.sections.length > 0)).toBe(true);
  });
});
