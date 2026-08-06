# Fluxos manuais E2E no Postman

Use o environment **E2E** e o perfil determinístico antes de executar qualquer fluxo:

```bash
docker compose --profile e2e run --rm test-data-seeder e2e
```

Preencha `loginEmail` e `loginPassword` somente na sessão do Postman. Não exporte a senha. Os requests abaixo estão nas pastas da collection **Etapa 9.3** e **Etapa 9.4**.

O arquivo [e2e-test-data.json](data/e2e-test-data.json) pode preencher os IDs das fixtures no Collection Runner. Para uso manual, copie somente os valores necessários para as variáveis da collection.

## 1. Onboarding de clínica

Permissão: `PlatformAdmin`.

1. Execute **Health - Ready**.
2. Execute **Login** com o administrador da plataforma.
3. Execute **Listar tenants (plataforma)** para registrar o estado inicial.
4. Execute **02 — Tenants / Onboarding de tenant** com `Idempotency-Key` e dados fictícios únicos.
5. Salve `tenantId`, `clinicId`, `unitId`, `userId` e `integrationId` da resposta.
6. Execute **Listar tenants (plataforma)**, **Listar clínicas da plataforma** e **Listar usuários da plataforma** para conferir o resultado.

O onboarding cria tenant, clínica, unidade inicial, administrador e integração Fake desabilitada em uma transação. Repetir a mesma chave deve retornar o resultado original.

## 2. Cadastro e atualização de paciente

Permissão: `Patients.Manage`.

1. Execute **Login** como `ClinicAdmin` ou `Receptionist`.
2. Execute **Buscar pacientes** para verificar duplicidade por dado de contato.
3. Execute **Criar paciente**; o script salva `patientId`.
4. Execute **Detalhar paciente**.
5. Execute `PUT /api/patients/{{patientId}}` com a alteração administrativa.
6. Consulte o detalhe novamente e confirme que a resposta pertence ao tenant da sessão.

Não inclua diagnóstico, sintomas, prontuário ou prescrição no payload.

## 3. Agendamento completo

Permissão: `ClinicStaff`.

1. Garanta `unitId`, `specialtyId`, `professionalId` e `patientId` válidos.
2. Execute **Disponibilidade por data** e selecione um slot livre.
3. Execute **Criar agendamento**; o script salva `appointmentId`.
4. Execute **Detalhar agendamento** e copie o `version` retornado para a próxima operação.
5. Execute **Confirmar agendamento**, **Reagendar agendamento** ou **Cancelar agendamento** com `expectedVersion` atualizado e uma nova `idempotencyKey` por operação.
6. Consulte o detalhe novamente após cada alteração.

Conflitos de agenda retornam `409`. Não reutilize `expectedVersion` após uma alteração bem-sucedida.

## 4. Conversa e atendimento humano

Permissão: `ClinicAdmin` para ações humanas.

1. Execute **Listar conversas**, selecione um item e preencha `conversationId`.
2. Execute **Detalhar conversa** e atualize `expectedVersion` no corpo dos requests seguintes.
3. Execute **Fila humana** e, quando aplicável, **Assumir conversa**.
4. Execute **Pausar automação**, **Enviar mensagem humana**, **Transferir conversa**, **Liberar conversa**, **Encerrar conversa** e **Reabrir conversa** conforme o cenário.
5. Consulte **Listar mensagens da conversa** e **Auditoria**.

Cada mudança de estado exige a versão mais recente. Mensagem manual também exige `Idempotency-Key`.

## 5. WhatsApp

Permissão: `ClinicAdmin`.

1. Execute **Integração WhatsApp - status** e confirme o provider esperado.
2. Para Fake, execute **Integração WhatsApp - validar** e **Integração WhatsApp - mensagem de teste**.
3. Consulte **Listar conversas** e as mensagens para verificar o processamento assíncrono.
4. Para templates, execute **Criar template WhatsApp**, **Detalhar template WhatsApp**, **Ativar template WhatsApp** e **Sincronizar templates WhatsApp**.
5. Em Twilio Sandbox, use somente a configuração prévia do servidor; nunca informe Auth Token no Postman.

## 6. Dashboard e auditoria

1. Execute **Dashboard** após um fluxo de agenda, conversa ou WhatsApp.
2. Execute **Auditoria** e filtre pelo período/ação relevante.
3. Confirme que os dados pertencem ao tenant autenticado e que a resposta não contém segredos ou conteúdo sensível de webhook.
