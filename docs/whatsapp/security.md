# Segurança e isolamento

O tenant de webhooks é determinado pela `IntegrationKey` no servidor, nunca por valor fornecido pelo cliente. Consumers validam tenant, integração, conversa e mensagem antes de persistir ou enviar. Templates e mídias são restringidos ao mesmo tenant/integration.

Logs não devem conter AuthToken, payload integral, telefone completo, conteúdo clínico ou URL de mídia. Use o status operacional autenticado, que mascara o telefone.
