"use client";

import Link from "next/link";
import type { ComponentProps } from "react";
import type { ReactNode } from "react";
import { trackPublicEvent, type PublicEvent, type PublicEventProperties } from "@/lib/analytics/public-events";

export function TrackedLink({ event, properties, children, ...props }: ComponentProps<typeof Link> & { event: PublicEvent; properties?: PublicEventProperties; children: ReactNode }) {
  return <Link {...props} onClick={() => trackPublicEvent(event, properties)}>{children}</Link>;
}
