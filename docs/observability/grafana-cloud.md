# Grafana Cloud

No Grafana Cloud, crie a stack, abra **Connections → OpenTelemetry**, escolha
`HTTP/protobuf`, copie o endpoint OTLP e gere uma credencial com a menor
permissão necessária. Configure o header retornado em
`OTEL_EXPORTER_OTLP_HEADERS`; ele é segredo e não deve ser versionado.

Não configure `Grafana__Token` ou `Grafana__Url`: o sistema fala somente o
protocolo OpenTelemetry. Valide primeiro localmente buscando
`service.name=ia-recepcao-api-local` e depois o Worker.
