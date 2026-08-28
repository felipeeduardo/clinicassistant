# Configuração Railway

Somente após a validação local, cadastre **separadamente** na API e no Worker:

```ini
Observability__Enabled=true
Observability__TraceSamplingRatio=1.0
OTEL_EXPORTER_OTLP_ENDPOINT=<endpoint-otlp-https>
OTEL_EXPORTER_OTLP_HEADERS=<secret>
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_SERVICE_NAMESPACE=ia-recepcao
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=production
```

Use `OTEL_SERVICE_NAME=ia-recepcao-api` na API e
`OTEL_SERVICE_NAME=ia-recepcao-worker` no Worker. O header pertence ao secret
store; não coloque em Dockerfile, GitHub, Postman ou frontend. Redeploy e
verifique os serviços no Grafana. Para rollback, altere apenas
`Observability__Enabled=false`.
