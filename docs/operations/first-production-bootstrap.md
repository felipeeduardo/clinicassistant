# Primeiro bootstrap de produção

1. Confirme que o banco de produção e as migrations estão prontos.
2. No secret store da Railway, configure `PlatformBootstrap__Enabled=true` e os
   e-mails/senhas `PlatformBootstrap__Admins__0/1__*` (sem colocar valores em
   arquivos ou tickets).
3. Faça um deploy/restart controlado da API.
4. Verifique os logs de início/conclusão e confirme que não há senha ou hash.
5. Faça login com cada PlatformAdmin e valide a área de plataforma.
6. Execute o onboarding da primeira clínica.
7. Rotacione as credenciais, defina `PlatformBootstrap__Enabled=false`, remova as
   senhas do secret store e reinicie.
8. Confirme auditoria e readiness da clínica.

O bootstrap não deve permanecer habilitado depois da criação inicial.
