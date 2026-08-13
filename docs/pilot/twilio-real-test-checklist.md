# Checklist de teste Twilio real

## Pré-condições

- [ ] Gate D aprovado.
- [ ] Sender WhatsApp aprovado e templates necessários publicados.
- [ ] Credenciais somente no secret manager do backend/Worker.
- [ ] Inbound e StatusCallback usam HTTPS estável.
- [ ] Assinatura `X-Twilio-Signature` validada.
- [ ] Destinatário único, allowlisted e pertencente à equipe de QA.
- [ ] Conteúdo e tenant do teste registrados.

## Execução controlada

1. Solicitar Gate E com responsável e referência de mudança.
2. Validar configuração da integração.
3. Enviar uma única mensagem pelo workflow manual protegido.
4. Acompanhar Outbox e callback até o estado final.
5. Confirmar auditoria, métricas e ausência de duplicidade.
6. Desabilitar a integração ou encerrar a janela de teste, conforme plano.

O Sandbox continua sendo o caminho de desenvolvimento. Este documento não contém
Account SID, Auth Token, telefone ou conteúdo de produção.
