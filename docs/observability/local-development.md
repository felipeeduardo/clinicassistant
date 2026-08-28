# Validação local

No `.env` (não versionado), preencha o endpoint/header reais e use:

```ini
OBSERVABILITY_ENABLED=true
OBSERVABILITY_TRACE_SAMPLING_RATIO=1.0
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_SERVICE_NAME=ia-recepcao-api-local
OTEL_SERVICE_NAMESPACE=ia-recepcao
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=development
```

Suba API e Worker, faça login, uma consulta de saúde e uma operação de banco,
aguarde a exportação e confirme os dois serviços no Grafana. Nunca imprima o
header. Para rollback local, defina `OBSERVABILITY_ENABLED=false`.
