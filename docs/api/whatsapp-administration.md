# Administração da integração WhatsApp

As operações exigem `ClinicAdmin` e não retornam credenciais, tokens ou referências sensíveis.

- `POST /api/whatsapp/integration/validate` valida a configuração local.
- `POST /api/whatsapp/integration/enable` e `/disable` alteram o estado operacional.
- `POST /api/whatsapp/integration/test-message` exige `Idempotency-Key`, usa exclusivamente `WhatsApp:TestRecipient` do ambiente e cria a mensagem e Outbox para processamento assíncrono.

## Templates

Os endpoints abaixo também exigem `ClinicAdmin` e sempre aplicam o tenant do token autenticado:

- `GET /api/whatsapp/templates?page=1&pageSize=25&search=&status=&languageCode=&category=&provider=` lista templates com paginação e filtros opcionais.
- `GET /api/whatsapp/templates/{templateId}` retorna o detalhe de um template do tenant atual.

O `ContentSid` é mascarado nas respostas e a lista de variáveis é derivada do schema persistido. As operações de criação, edição, ativação, desativação e sincronização serão disponibilizadas nos próximos incrementos administrativos; nenhum endpoint de consulta expõe tokens ou resposta bruta do provider.

Configure o destinatário de teste somente no ambiente: `WHATSAPP_TEST_RECIPIENT=+<numero-em-formato-e164>`. Não o registre em código, seeds ou documentação pública.
