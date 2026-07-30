# Webhooks Twilio

Mensagens recebidas usam `POST /api/webhooks/whatsapp/twilio/{integrationKey}`. Atualizações usam `POST /api/webhooks/whatsapp/twilio/status/{integrationKey}`. Ambos aceitam somente formulário e validam `X-Twilio-Signature` antes de persistir dados.

Para desenvolvimento local, exponha a API com ngrok ou Cloudflare Tunnel e use a origem HTTPS pública nas variáveis `TWILIO__*_BASE_URL`; não coloque URLs temporárias no código.
