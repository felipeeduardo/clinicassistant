# Prontidão Twilio para produção

O envio real só pode ser validado pelo workflow manual **Manual Twilio smoke**. Ele não integra o pipeline de pull request e envia exatamente uma mensagem ao destinatário de teste configurado exclusivamente no ambiente da API.

## Configuração do ambiente protegido

Crie o environment GitHub `twilio-production-smoke` e exija aprovação. Cadastre nele, como secrets:

- `TWILIO_SMOKE_API_URL`: URL HTTPS da API já implantada;
- `TWILIO_SMOKE_ADMIN_EMAIL`: administrador técnico do tenant de smoke;
- `TWILIO_SMOKE_ADMIN_PASSWORD`: senha desse administrador.

Na API desse ambiente, mantenha `WHATSAPP_TEST_RECIPIENT` configurado com um único número E.164 autorizado. Esse valor não é versionado, não é informado como input do workflow e não pode ser um contato de paciente real.

## Checklist antes do smoke

- [ ] Credenciais Twilio foram rotacionadas e estão fora do repositório.
- [ ] Sender WhatsApp está autorizado pela Twilio.
- [ ] API está publicada em HTTPS.
- [ ] Webhook inbound e StatusCallback apontam para a URL HTTPS pública correta.
- [ ] Validação de `X-Twilio-Signature` está ativa em produção.
- [ ] Templates necessários foram aprovados e sincronizados.
- [ ] Destinatário de smoke está na allowlist via `WHATSAPP_TEST_RECIPIENT`.
- [ ] Nenhum dado real de paciente será usado.
- [ ] Logs, traces e alertas estão sanitizados e ativos.
- [ ] Há plano de rollback para desabilitar a integração.

## Execução controlada

1. Abra **Actions → Manual Twilio smoke → Run workflow**.
2. Informe a referência da mudança ou validação operacional.
3. Digite exatamente `SEND_ONE_TWILIO_SMOKE` como confirmação.
4. Aprove o environment protegido quando solicitado.
5. Confirme no resumo do job que uma única solicitação foi aceita.
6. Confirme a entrega final pelo StatusCallback, logs sanitizados e métricas do ambiente.

O workflow valida que a integração conectada é Twilio, solicita a validação local da configuração e chama uma única vez `POST /api/whatsapp/integration/test-message`. Ele não imprime token, senha, número de telefone ou Auth Token. A entrega não é síncrona: o resultado final depende do callback da Twilio.

## Após a execução

- [ ] Consultar `whatsapp.message.status.changed` e a auditoria sanitizada.
- [ ] Verificar `twilio_configuration_validations_total` e falhas de envio.
- [ ] Registrar resultado e referência da mudança.
- [ ] Em caso de falha, desabilitar a integração, investigar logs e executar rollback documentado.
