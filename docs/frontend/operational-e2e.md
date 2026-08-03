# Frontend operacional E2E — dependências

Atualizado na Etapa 9.2. Esta entrega usa somente as APIs existentes. Onde o backend não fornece uma operação, a interface não simula resultado e o fluxo fica explicitamente bloqueado.

## Matriz tela x endpoint

| Tela | Endpoint | Estado |
| --- | --- | --- |
| `/clinics` | `GET/PUT /api/clinics/current` | Disponível para a clínica da sessão; não há lista global de clínicas. |
| `/units` | `GET/POST/PUT /api/units`, detalhe e status operacional | Disponível; detalhe apresenta fuso, horários e profissionais vinculados. A exclusão é bloqueada quando houver profissionais. |
| `/patients` | `GET/POST/PUT /api/patients`, `GET /api/patients/search`, `GET /api/patients/{id}` | Disponível; listagem paginada com busca e filtro de consentimento, detalhe com origem, próximos agendamentos, conversas e resumo de auditoria. Telefone e e-mail são mascarados na listagem. |
| `/professionals` | `GET/POST/PUT /api/professionals`, agenda dedicada | Disponível; a tela consulta agenda dos próximos sete dias. APIs de disponibilidade, bloqueios e férias estão disponíveis para gestão administrativa. |
| `/specialties` | `GET/POST/PUT /api/specialties`, dependências e status | Disponível; a tela expõe dependências e só permite o backend efetivar desativação quando for segura. |
| `/appointments` | `GET /api/appointments?startsAt&endsAt`, `GET /api/professionals/{id}/availability`, `POST /api/appointments`, confirmar e cancelar | Disponível para operação diária. Reagendamento e versões esperadas continuam indisponíveis. |
| `/conversations` | Listagem, detalhe, mensagens, marcar leitura, atribuir, liberar, pausar e retomar | Disponível. As quatro últimas ações enviam `expectedVersion` e o backend pode responder `409`. |
| `/integrations/whatsapp` | `GET /api/whatsapp/integration/status` | Disponível em modo somente leitura, com número mascarado e diagnóstico sanitizado. |
| `/tenants`, `/users`, onboarding | — | Bloqueado: não existem endpoints administrativos. |
| `/audit` | — | Bloqueado: não existe endpoint de auditoria. |

## Matriz permissão x ação

| Ação | Política atual do backend | Interface |
| --- | --- | --- |
| Ler cadastros | `ClinicStaff` | Permitida aos usuários autenticados do tenant. |
| Criar/editar clínica, unidades, pacientes, profissionais e especialidades | `ClinicStaff` | A interface mostra mutations somente para `ClinicAdmin`; o backend ainda deve restringir estas operações se essa for a regra definitiva. |
| Administrar conversas | `ClinicAdmin` para atribuir, liberar e automação | Fora do escopo desta entrega. |
| Administração de plataforma | `PlatformAdmin` | Bloqueada, sem endpoint. |

## Tempo real e E2E

O Hub SignalR autenticado está disponível em `/hubs/operations`, isolado pelo `tenant_id` da sessão. A implementação, eventos publicados e mapeamento de cache estão documentados em [realtime.md](realtime.md). As mutations HTTP continuam invalidando somente sua query de recurso (`patients`, `units`, `specialties`, `professionals` ou `clinic`).

Os seeds E2E fornecem usuários e cadastros de tenant, mas o manifesto ainda não é exposto como contrato do frontend. Testes E2E de onboarding, troca de tenant, auditoria e realtime continuam bloqueados pelas dependências descritas acima.

Os testes Playwright consomem o manifesto diretamente para usuários e IDs determinísticos. Veja [e2e-playwright.md](e2e-playwright.md) para execução e limites atuais.

## Limitações registradas

- Os contratos atuais não expõem `expectedVersion` nos cadastros; não é possível enviar controle de concorrência sem alterar o backend.
- Os endpoints de cadastro não foram definidos como idempotentes; a interface não adiciona um comportamento que o servidor não garante.
- A criação de consulta verifica disponibilidade no servidor e responde `409` quando o horário já não está disponível. O frontend preserva a seleção para nova tentativa.
- A Inbox não oferece transferência, envio manual, encerramento ou reabertura, pois não há endpoints administrativos para essas ações.
- A administração de WhatsApp não expõe credenciais, templates, sincronização ou alteração de provider; por isso a tela é estritamente informativa.
- Não são exibidos segredos de integração, payloads de webhook ou conteúdo clínico.
