# Dashboards

Crie no Grafana, após observar o baseline, o dashboard **IA Recepção —
Production Overview** com request rate/P95/5xx, Worker processados/falhas,
Outbox pendente/falha, WhatsApp, SendGrid, agenda e lembretes.

Dashboard **WhatsApp Operations**: inbound, outbound, falhas, latência e fila.
Dashboard **Scheduling**: criados, reagendados, cancelados, falhas e lembretes.
Não há JSON versionado porque os nomes finais e queries dependem do backend do
Grafana; utilize os Meters documentados em `docs/operations/operational-metrics.md`.
