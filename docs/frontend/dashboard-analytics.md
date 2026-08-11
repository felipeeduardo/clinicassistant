# Dashboard executivo

## Contrato atual

`GET /api/dashboard?from={ISO}&to={ISO}` agrega no backend consultas, pendências, distribuição por status, fila humana, conversas ativas, falhas de Outbox, status da integração e métricas reais de WhatsApp (mensagens recebidas, enviadas, falhas e conversas abertas). O período padrão continua sendo o dia atual quando os parâmetros não são enviados.

O frontend oferece os atalhos Hoje, 7 dias, 30 dias e Este mês, além de Personalizado. Neste último caso, as datas são persistidas na URL (`/dashboard?period=custom&from=YYYY-MM-DD&to=YYYY-MM-DD`) e convertidas em um intervalo inclusivo antes da chamada à API.

## Limitações honestas

O domínio ainda não possui eventos agregados suficientes para evolução temporal, ocupação por profissional, horários mais procurados, no-show, SLA médio ou funil conversa → confirmação. O frontend exibe essa lacuna em vez de estimar números. A evolução deve começar por contratos de eventos e consultas agregadas, não por carregar registros brutos no navegador.

## Ações

Cards de consultas, pendências, fila e falhas levam diretamente à Agenda, Conversas ou Operação WhatsApp. O estado de atenção é calculado apenas a partir dos contadores reais retornados pela API.

O widget de próximas consultas é agregado pelo backend, limitado a cinco itens nos próximos sete dias, e retorna nomes de paciente, profissional e unidade. O clique continua levando à Agenda filtrada no dia da consulta.

O contrato futuro das métricas avançadas está detalhado em [dashboard-metrics-roadmap.md](./dashboard-metrics-roadmap.md).
