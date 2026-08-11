# Roadmap de métricas avançadas do Dashboard

O Dashboard atual exibe apenas agregados disponíveis no domínio. As métricas abaixo ficam documentadas para uma evolução posterior, sem estimativas no frontend.

## Contrato proposto

```json
{
  "appointmentsTrend": [{ "date": "2026-08-11", "scheduled": 0, "confirmed": 0, "completed": 0, "cancelled": 0 }],
  "professionalOccupancy": [{ "professionalId": "uuid", "professionalName": "Nome", "appointments": 0, "occupiedMinutes": 0, "occupancyRate": 0 }],
  "peakHours": [{ "hour": "08:00", "appointments": 0 }],
  "funnel": { "conversationsStarted": 0, "availabilityConsulted": 0, "bookingStarted": 0, "appointmentsBooked": 0, "appointmentsConfirmed": 0 },
  "serviceLevels": { "averageFirstResponseSeconds": 0, "conversationsOverSla": 0 }
}
```

## Dados necessários

- Eventos de transição da conversa para medir disponibilidade consultada e agendamento iniciado.
- Histórico de mensagens inbound/outbound com timestamps para SLA e tempo de primeira resposta.
- Regras de disponibilidade por profissional para calcular capacidade e taxa de ocupação, não apenas quantidade de consultas.
- Normalização de fuso horário do tenant para agrupar horários de maior procura.

## Critérios de implementação futura

1. Criar consultas agregadas no backend com filtro de tenant e período.
2. Expor o contrato somente quando houver dados suficientes para cada métrica.
3. Mostrar estado `indisponível` quando a métrica ainda não possuir eventos.
4. Adicionar testes de autorização, período, fuso horário e divisão por zero.
5. Adicionar drill-down para Agenda, Conversas e Auditoria.
