# Abstração de provider WhatsApp

O produto depende de `IWhatsAppGateway`, definido na camada de aplicação. `FakeWhatsAppGateway` e `TwilioWhatsAppGateway` são implementações de infraestrutura; controllers, conversas e Outbox não conhecem o cliente Twilio.

## Seleção

- Development/Test: `WhatsApp:Provider=Fake` é permitido.
- Production: somente `WhatsApp:Provider=Twilio` é aceito.
- A seleção ocorre uma única vez no registro de dependências.
- Não existe fallback de Twilio para Fake.

O estado de uso da clínica (`Pending`, `Connected`, `Disabled`) é independente do provider global. O ClinicAdmin controla apenas **usar WhatsApp nesta clínica**.

