# Runbook de migrations em produção

## Princípios

- Usar PostgreSQL separado de Development/Test.
- Nunca usar `EnsureCreated()`.
- Fazer backup antes de migration potencialmente destrutiva.
- Executar migrations uma única vez, fora do ciclo de múltiplas réplicas da API.
- Não apontar `Database__Target=test` para Production.

## Procedimento

1. Confirmar a connection string no secret manager, sem copiá-la para terminal,
   ticket ou log.
2. Confirmar o commit/imagem que será promovido e registrar a janela de mudança.
3. Comparar migrations pendentes com `backend/src/ClinicAssistant.Infrastructure/Persistence/Migrations/`.
4. Criar e verificar o backup conforme o runbook do provedor.
5. Pausar o rollout de réplicas adicionais e executar a migration com um único job
   administrativo ou instância controlada.
6. Validar a tabela de histórico do EF Core e a integridade das tabelas afetadas.
7. Subir API e Worker, verificando `/health/live` e `/health/ready`.
8. Executar smoke autenticado sem dados reais e registrar evidências.

## Rollback

Migration aplicada não deve ser revertida automaticamente em produção. Em caso de
falha, parar o rollout, preservar logs sanitizados, restaurar backup se necessário
e seguir o plano de rollback da versão. Toda alteração de schema deve ter plano de
compatibilidade entre versão anterior e posterior.
