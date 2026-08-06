# Realtime operacional

O Hub autenticado é `GET /hubs/operations`. O servidor associa cada conexão ao grupo do tenant extraído do JWT; clientes não fornecem nem escolhem tenant.

Os eventos usam envelope sanitizado com `eventId`, tipo, tenant, data de ocorrência, correlação e versão quando aplicável. O frontend deduplica por `eventId` e invalida queries HTTP específicas; ele não usa o payload como fonte de dados.

Eventos de conversa, agenda, WhatsApp, templates, fila, auditoria e dashboard são publicados somente após o commit. O mapeamento completo de eventos e cache está em [Tempo real operacional](../frontend/realtime.md).
