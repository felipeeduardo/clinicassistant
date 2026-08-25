import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach } from "vitest";

// Unmount every React tree before jsdom is torn down. Without this, React's
// scheduler can flush a pending update after the environment has removed
// `window`, producing false-positive unhandled errors.
afterEach(() => cleanup());
