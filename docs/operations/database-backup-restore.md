# Backup e restauração do PostgreSQL

## Requisitos antes do Production

- [ ] Backup automático habilitado no provedor.
- [ ] Retenção, criptografia e acesso restrito documentados.
- [ ] RPO/RTO aprovados pelo responsável operacional.
- [ ] Restauração testada em banco separado.
- [ ] Resultado do teste registrado sem dados pessoais em logs ou tickets.

## Procedimento de restauração

1. Abrir incidente e interromper mudanças no API/Worker.
2. Identificar o backup e validar sua integridade pelo provedor.
3. Restaurar em instância isolada primeiro.
4. Validar migrations, integridade referencial e health checks.
5. Solicitar aprovação para promover a instância restaurada.
6. Atualizar a conexão pelo secret manager e reiniciar API/Worker de forma gradual.
7. Executar o smoke operacional e registrar o resultado.

Nunca incluir connection strings, senhas ou dumps no repositório.
