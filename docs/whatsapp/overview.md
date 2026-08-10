# Visão geral do WhatsApp

O fluxo usa webhook assinado, Inbox/Outbox transacional, RabbitMQ e consumers separados. A API recebe rapidamente; o Worker cria paciente, conversa e mensagem, e processa envios de forma assíncrona. `IWhatsAppGateway` mantém Twilio isolado em Infrastructure e permite o provider `Fake` no desenvolvimento.

Para o roteiro completo de interação, consulte o [playbook de conversação](conversation-playbook.md), que documenta perguntas, respostas, comandos globais e diagramas dos fluxos.

Os endpoints públicos são os webhooks Twilio. O status operacional autenticado é `GET /api/whatsapp/integration/status` e nunca retorna credenciais ou telefone completo.
