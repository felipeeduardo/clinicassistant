# Setup operacional do ClinicAdmin

Após o provisionamento, o ClinicAdmin entra usando o e-mail criado pela plataforma. O tenant é resolvido automaticamente pelas claims da sessão; não existe seleção manual de clínica.

Acesse `/setup` para acompanhar o progresso derivado dos dados persistidos:

1. revisar dados básicos;
2. cadastrar unidades adicionais;
3. criar especialidades;
4. cadastrar profissionais;
5. configurar disponibilidade, bloqueios e férias;
6. conectar WhatsApp e seus templates.

Cada cartão leva diretamente ao módulo correspondente. O fluxo não é um wizard bloqueante: módulos podem ser concluídos em qualquer momento, respeitando apenas dependências reais, como profissional depender de especialidade e disponibilidade depender de profissional.

Todas as operações usam o tenant presente na identidade autenticada. Tentativas de enviar ou substituir outro tenant são rejeitadas pelo backend.
