# Runbook operacional

1. Validar local com `Observability__Enabled=true` e sampling 1.0.
2. Confirmar API e Worker no Grafana, traces HTTP/DB e correlação Outbox/RabbitMQ.
3. Configurar secrets OTLP separadamente na API e Worker do Railway.
4. Fazer redeploy, gerar uma operação controlada e observar 30–60 minutos.
5. Em falha, consultar Railway para container/deploy e Grafana para traces,
   métricas e correlação. Desative `Observability__Enabled` para rollback;
   fluxos de negócio devem continuar normalmente.
