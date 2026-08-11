# Scheduling frontend

The `/appointments` route is the operational entry point for scheduling. It owns view state and query orchestration, while `CalendarShell` and its child views are presentational. Appointment creation, availability, confirmation, cancellation and rescheduling continue to use the existing API services and backend authorization/version checks.

## Query contract

The calendar requests only the selected period (`from`/`to`) and active professional, specialty, unit, status and source filters. Patient search is applied to the resolved display name until a server-side patient-search parameter is available. Names are resolved from catalog/patient queries and IDs never appear in event cards.

When a professional filter is active, the existing schedule endpoint is queried for the same period. Blocks and vacations are rendered as non-appointment events with explicit labels and cannot trigger appointment mutations.

## Delivery boundary

This delivery implements the visual foundation and views. Drawer operations remain available; quick-create progression, conflict-specific messaging, blocks/vacations, incremental SignalR updates and drag-and-drop are subsequent work and must not move business rules into React.
