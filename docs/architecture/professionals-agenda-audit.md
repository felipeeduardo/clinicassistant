# Auditoria — Profissionais e Agenda

## Diagnóstico inicial

O domínio já possuía a maior parte da base necessária para a evolução, portanto não foi criada uma fonte paralela de disponibilidade:

- `Professional`, `ProfessionalSpecialty`, `Specialty`, `ClinicUnit` e `Clinic` estão em `backend/src/ClinicAssistant.Domain/Clinics/CatalogEntities.cs`.
- `AvailabilityRule`, `ScheduleBlock`, `ProfessionalVacation` e `Appointment` estão em `backend/src/ClinicAssistant.Domain/Scheduling/SchedulingEntities.cs`.
- O `SchedulingService` gera os horários a partir de `AvailabilityRule` e remove consultas, bloqueios e férias conflitantes. Esse serviço continua sendo a fonte de verdade usada pela API e pelo WhatsApp.
- A API já expõe cadastro de profissionais, regras, bloqueios, férias e agenda individual em `Program.cs`.
- O frontend já possui um calendário compartilhado em `components/calendar/calendar.tsx`; a ação Agenda agora abre a rota dedicada `/professionals/{id}/schedule`, que combina consultas, bloqueios e férias no mesmo calendário.
- Multi-tenancy e RBAC são aplicados pelo `TenantAccessGuard` e pelas políticas `ProfessionalsView`/`ProfessionalsManage`.
- As migrations existentes já suportam as entidades atuais; esta evolução não exige migration para regras recorrentes múltiplas.

## Ajustes desta rodada

- Regras recorrentes agora aceitam múltiplos períodos no mesmo dia, desde que não se sobreponham.
- Inclusão e substituição de regras rejeitam sobreposições explicitamente.
- A página de profissionais ganhou busca por nome/registro/e-mail e filtros por especialidade, unidade e status.
- A tabela passou a exibir especialidades e unidade com nomes legíveis e ações com ícones/labels acessíveis.
- Foi adicionada a agenda individual responsiva em rota dedicada, com navegação dia/semana/mês, fuso da clínica e legenda/status do calendário compartilhado.
- Foram preservados o slot engine, os fluxos de agendamento/reagendamento/cancelamento, Outbox, Worker e autorização existentes.

## Pendências planejadas

A importação XLSX com preview/commit/histórico, exceções de disponibilidade, auditoria específica de importação e testes de integração/E2E ainda requerem implementação. O contrato inicial foi definido em `docs/architecture/professionals-agenda-import-contract.md`; falta selecionar/incluir o parser XLSX aprovado e implementar os endpoints transacionais.
