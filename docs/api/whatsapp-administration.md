# Administração da integração WhatsApp

As operações exigem `ClinicAdmin` e não retornam credenciais, tokens ou referências sensíveis.

- `POST /api/whatsapp/integration/validate` valida a configuração local.
- `POST /api/whatsapp/integration/enable` e `/disable` alteram o estado operacional.
- `POST /api/whatsapp/integration/test-message` exige `Idempotency-Key`, usa exclusivamente `WhatsApp:TestRecipient` do ambiente e cria a mensagem e Outbox para processamento assíncrono.

Configure o destinatário de teste somente no ambiente: `WHATSAPP_TEST_RECIPIENT=+<numero-em-formato-e164>`. Não o registre em código, seeds ou documentação pública.
