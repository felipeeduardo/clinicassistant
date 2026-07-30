## Etapa 9 — Frontend Operacional e Validação E2E

Implementar o frontend operacional do sistema conforme a especificação detalhada disponível em:

`docs/prompts/etapa-09-frontend-operacional-e2e.md`

A Etapa 9 deverá priorizar a validação dos fluxos reais do produto antes da implementação da Etapa 8, referente à Inteligência Artificial, RAG e Tool Calling.

O frontend deverá ser desenvolvido com:

* Next.js;
* TypeScript;
* App Router;
* Tailwind CSS;
* TanStack Query;
* React Hook Form;
* Zod;
* SignalR;
* Vitest;
* Testing Library;
* Playwright.

A implementação deverá contemplar:

### Fundação do frontend

* estrutura modular por features;
* API Client centralizado;
* Query Client;
* tratamento padronizado de erros;
* providers globais;
* variáveis de ambiente;
* Docker;
* integração com CI;
* testes unitários, de integração e E2E.

### Autenticação

* login;
* logout;
* recuperação da sessão;
* expiração da sessão;
* proteção de rotas;
* redirecionamento;
* tratamento de acesso não autorizado;
* armazenamento seguro de credenciais e tokens.

### Autorização

* controle por permissões;
* ocultação de ações não permitidas;
* proteção de páginas;
* proteção de componentes;
* validação de permissões pelo backend;
* suporte aos perfis administrativos e operacionais.

### Multi-tenancy

* identificação do tenant atual;
* exibição do tenant no layout;
* troca de tenant quando autorizada;
* limpeza de caches após troca;
* reinicialização das conexões em tempo real;
* garantia de que dados de tenants diferentes não sejam reutilizados;
* testes de isolamento.

### Layout e navegação

* sidebar;
* header;
* navegação responsiva;
* breadcrumbs;
* usuário atual;
* tenant atual;
* status de conexão;
* status da integração WhatsApp;
* notificações;
* estados de loading, erro, vazio e acesso negado.

### Dashboard

* conversas abertas;
* conversas aguardando paciente;
* conversas aguardando atendimento humano;
* conversas em atendimento;
* tamanho da fila humana;
* mensagens recebidas;
* mensagens enviadas;
* falhas de envio;
* consultas agendadas;
* consultas reagendadas;
* consultas canceladas;
* tempo médio de espera;
* status da integração WhatsApp;
* filtros por período, unidade, fila e atendente.

### Conversas

* lista de conversas;
* filtros;
* paginação server-side;
* busca;
* ordenação;
* conversa selecionada;
* histórico de mensagens;
* direção da mensagem;
* tipo da mensagem;
* status de envio;
* mensagens humanas e automatizadas;
* intenção atual;
* etapa atual;
* prioridade;
* responsável;
* estado da automação;
* indicadores de novas mensagens e falhas.

### Atendimento humano

* fila de atendimento;
* assumir conversa;
* liberar conversa;
* transferir conversa;
* pausar automação;
* retomar automação;
* solicitar handoff;
* encerrar conversa;
* reabrir conversa;
* alterar prioridade;
* enviar mensagem manual;
* tratar conflitos de concorrência;
* registrar ações no histórico e auditoria.

### Atualização em tempo real

* integração com SignalR;
* atualização de conversas;
* atualização de mensagens;
* atualização de status;
* atualização da fila;
* atualização de agendamentos;
* atualização da integração;
* reconexão com backoff;
* indicador de conexão;
* prevenção de eventos duplicados;
* sincronização após reconexão;
* polling controlado como fallback temporário, quando necessário.

### Agenda

* visualização diária;
* visualização semanal;
* visualização em lista;
* filtros por profissional;
* filtros por especialidade;
* filtros por unidade;
* filtros por status;
* timezone da clínica;
* detalhes do agendamento;
* conflitos;
* origem do agendamento;
* confirmação.

### Agendamento manual

* seleção de paciente;
* seleção de especialidade;
* seleção de profissional;
* seleção de unidade;
* consulta de disponibilidade;
* seleção de horário;
* confirmação;
* revalidação do slot;
* idempotência;
* tratamento de conflitos;
* atualização da agenda;
* atualização da conversa quando aplicável.

### Reagendamento

* seleção da consulta;
* seleção de novo horário;
* comparação entre horário atual e novo;
* confirmação explícita;
* tratamento de slot indisponível;
* preservação da consulta original em caso de falha;
* atualização da agenda;
* atualização da conversa.

### Cancelamento

* seleção da consulta;
* exibição da política aplicável;
* motivo opcional;
* confirmação explícita;
* tratamento de consulta já cancelada;
* atualização da agenda;
* atualização da conversa;
* feedback de sucesso ou falha.

### Profissionais

* listagem;
* busca;
* filtros;
* detalhes;
* especialidades;
* unidades;
* agenda;
* disponibilidade;
* criação;
* edição;
* ativação;
* desativação;
* vínculo com especialidades;
* vínculo com unidades.

### Especialidades

* listagem;
* criação;
* edição;
* ativação;
* desativação;
* vínculo com profissionais;
* vínculo com unidades;
* preservação de histórico;
* bloqueio de exclusões destrutivas quando houver dependências.

### Pacientes

* listagem;
* busca;
* detalhes administrativos;
* telefone mascarado;
* histórico de conversas;
* consultas futuras;
* histórico operacional;
* consentimentos quando disponíveis;
* nenhuma exibição de prontuário clínico nesta etapa.

### Integração WhatsApp

* provider;
* número mascarado;
* status;
* último webhook;
* último envio;
* última falha;
* templates;
* ambiente;
* envio de mensagem de teste;
* validação da integração;
* sincronização de templates quando suportada;
* atualização do status de mensagens;
* nenhuma exposição de segredos.

### Configurações

* dados gerais da clínica;
* unidades;
* filas de atendimento;
* horários de funcionamento;
* políticas operacionais;
* templates internos;
* permissões;
* parâmetros da integração;
* configurações visíveis somente para usuários autorizados.

### Auditoria

* ações administrativas;
* mudanças de estado;
* atribuições;
* mensagens manuais;
* agendamentos;
* reagendamentos;
* cancelamentos;
* correlation ID;
* resultado;
* dados sanitizados;
* nenhuma exibição de payload sensível.

### Segurança

* proteção contra XSS;
* ausência de segredos em variáveis públicas;
* tratamento seguro de tokens;
* validação de permissões;
* isolamento multi-tenant;
* mascaramento de telefone;
* proteção de rotas;
* headers de segurança;
* CSP quando possível;
* ausência de logs sensíveis;
* tratamento das mensagens do paciente como entrada não confiável.

### Acessibilidade e responsividade

* navegação por teclado;
* foco visível;
* labels;
* aria;
* contraste adequado;
* suporte a desktop e tablet;
* suporte básico a mobile;
* dialogs acessíveis;
* estados que não dependam apenas de cor;
* aderência inicial à WCAG 2.2 nível AA.

### Testes

Criar:

* testes unitários;
* testes de integração com API mockada;
* testes de autenticação;
* testes de autorização;
* testes de multi-tenancy;
* testes de componentes;
* testes de formulários;
* testes de conflitos;
* testes de tempo real;
* testes E2E com Playwright;
* ambiente E2E sem dependência da internet;
* FakeWhatsAppGateway;
* dados isolados por execução.

Os testes E2E deverão validar, no mínimo:

1. login;
2. carregamento do dashboard;
3. mensagem recebida;
4. conversa exibida;
5. atendimento assumido;
6. automação pausada;
7. mensagem manual enviada;
8. atualização de status;
9. retorno à automação;
10. encerramento da conversa;
11. criação de agendamento;
12. reagendamento;
13. cancelamento;
14. conflito de slot;
15. conflito de atribuição;
16. integração WhatsApp;
17. falha simulada;
18. isolamento multi-tenant.

### Critérios de aceite

A Etapa 9 somente será considerada concluída quando:

* o frontend compilar;
* autenticação funcionar;
* autorização funcionar;
* multi-tenancy estiver validado;
* dashboard estiver funcional;
* Inbox de conversas estiver funcional;
* histórico de mensagens estiver correto;
* fila humana estiver funcional;
* envio manual utilizar backend e Outbox;
* automação puder ser pausada e retomada;
* tempo real estiver funcional;
* pacientes puderem ser consultados;
* profissionais e especialidades puderem ser gerenciados;
* agenda estiver funcional;
* agendamento manual funcionar;
* reagendamento funcionar;
* cancelamento funcionar;
* integração WhatsApp estiver visível e testável;
* erros e conflitos forem tratados;
* acessibilidade básica estiver validada;
* testes unitários passarem;
* testes de integração passarem;
* testes E2E passarem;
* Docker Compose continuar funcional;
* CI passar;
* documentação estiver atualizada;
* nenhum segredo estiver exposto.

### Ordem inicial de implementação

Antes de alterar o código:

1. analisar o frontend atual;
2. analisar os endpoints do backend;
3. identificar contratos existentes;
4. identificar endpoints ausentes;
5. listar páginas;
6. listar componentes;
7. listar hooks;
8. listar services;
9. listar schemas;
10. listar providers;
11. descrever autenticação;
12. descrever autorização;
13. descrever multi-tenancy;
14. descrever integração em tempo real;
15. descrever estratégia E2E;
16. listar riscos;
17. apresentar arquivos a criar e alterar.

Implementar inicialmente apenas:

* fundação do frontend;
* API Client;
* Query Client;
* tratamento de erros;
* autenticação;
* autorização;
* contexto multi-tenant;
* layout;
* navegação;
* dashboard inicial;
* estrutura inicial da Inbox;
* testes da fundação;
* configuração Playwright;
* Docker;
* documentação inicial.

Ao concluir, executar:

```bash
npm ci
npm run lint
npm run typecheck
npm run test
npm run build
```

Caso o projeto utilize `pnpm` ou `yarn`, manter o gerenciador já adotado.

Corrigir todos os erros antes de avançar.

Não avançar para SignalR, envio manual, fila humana completa, agenda, agendamento, reagendamento, cancelamento ou testes E2E completos enquanto:

* a aplicação não compilar;
* autenticação não estiver funcional;
* autorização não estiver validada;
* isolamento multi-tenant não estiver testado;
* os testes da fundação não passarem;
* nenhum segredo estiver exposto.

A Etapa 8 deverá permanecer adiada até que os principais fluxos da Etapa 9 estejam validados ponta a ponta.
