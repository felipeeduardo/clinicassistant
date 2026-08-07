# Mensagem de teste WhatsApp

O endpoint atual é `POST /api/whatsapp/integration/test-message`, protegido por `ClinicAdmin` e com `Idempotency-Key`. Ele cria a mensagem e a Outbox de forma transacional e retorna `202 Accepted`; o Worker resolve Fake ou Twilio depois. O provider padrão em desenvolvimento e CI é Fake.
