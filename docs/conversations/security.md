# Segurança

O consumer recebe apenas o contrato interno produzido pela Etapa 6. Antes de processar, valida tenant, integração, conversa, paciente e mensagem de entrada. Consultas não utilizam somente identificadores isolados.

Logs contêm apenas identificadores técnicos necessários para diagnóstico; não registram corpo da mensagem, telefone, mídia ou dados clínicos. Termos clínicos e pedidos de diagnóstico provocam handoff e interrompem a automação.
