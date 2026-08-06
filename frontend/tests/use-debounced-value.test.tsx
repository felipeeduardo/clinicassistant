import { act, renderHook } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { useDebouncedValue } from "@/hooks/use-debounced-value";

afterEach(() => vi.useRealTimers());

describe("useDebouncedValue", () => {
  it("only publishes the latest value after the configured delay", () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(({ value }) => useDebouncedValue(value, 300), { initialProps: { value: "ana" } });
    rerender({ value: "ana souza" });
    expect(result.current).toBe("ana");
    act(() => vi.advanceTimersByTime(300));
    expect(result.current).toBe("ana souza");
  });
});
