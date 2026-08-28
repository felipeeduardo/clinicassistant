# OpenTelemetry

`Observability__Enabled` controla a exportação. Com `false`, a aplicação não
exige endpoint/headers OTLP e não faz chamadas externas. `TraceSamplingRatio`
aceita de 0 a 1 (1.0 é 100%). Métricas não dependem do sampling.

Os nomes de serviço são definidos por `OTEL_SERVICE_NAME`; localmente use
`ia-recepcao-api-local` e `ia-recepcao-worker-local`, e em produção
`ia-recepcao-api`/`ia-recepcao-worker`. O namespace é `ia-recepcao`.

ASP.NET Core, HttpClient, EF Core, ActivitySources de WhatsApp/conversas e os
Meters operacionais existentes são exportados apenas quando habilitados.
Health checks são filtrados dos traces HTTP. O exporter é best-effort: falhas
OTLP não interrompem API, Worker, Outbox, agenda, WhatsApp ou e-mail.
