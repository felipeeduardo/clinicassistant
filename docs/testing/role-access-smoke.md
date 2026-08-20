# Smoke manual de perfis e permissões

Use este roteiro após qualquer alteração no onboarding, no menu lateral ou nas policies da API. O objetivo é confirmar que o `PlatformAdmin` administra tenants sem ser tratado como usuário clínico e que o `ClinicAdmin` conduz a configuração operacional do próprio tenant.

## Preparação

1. Suba a API, o frontend e o banco do ambiente que será validado.
2. Garanta um usuário ativo de cada perfil: `PlatformAdmin`, `ClinicAdmin`, `Receptionist`, `Professional` e `Viewer` (quando usado pelo ambiente).
3. Abra uma janela anônima por perfil ou limpe a sessão entre as trocas.
4. Registre o status HTTP observado no DevTools para cada tentativa bloqueada.

## Matriz esperada

| Perfil | Deve acessar | Deve administrar | Não deve acessar |
|---|---|---|---|
| `PlatformAdmin` | Dashboard global, Plataforma e Leads | Provisionar tenant, consultar status e ciclo de vida | Conversas clínicas, pacientes, agenda, catálogo e WhatsApp operacional |
| `ClinicAdmin` | Todas as áreas do próprio tenant e `/setup` | Catálogo, agenda, conversas humanas, WhatsApp e auditoria | `/platform` e dados de outro tenant |
| `Receptionist` | Conversas, pacientes e agenda conforme menu | Operação permitida pela policy de atendimento | Catálogo administrativo, WhatsApp e `/platform` |
| `Professional` | Pacientes e agenda conforme menu | Somente ações explicitamente permitidas pela operação | Catálogo administrativo, WhatsApp e `/platform` |
| `Viewer` | Apenas leituras previstas pela policy | Nenhuma mutação administrativa | Todas as mutações e `/platform` |

## Roteiro por perfil

### PlatformAdmin

- [ ] `/platform` carrega tenants, usuários globais e clínicas globais.
- [ ] O botão de novo onboarding abre o formulário de clínica, unidade e ClinicAdmin.
- [ ] `/api/platform/onboarding` aceita a criação com `Idempotency-Key`.
- [ ] O tenant recém-criado aparece como **Em configuração**.
- [ ] Ações de ativar, suspender e desativar respeitam o estado do tenant.
- [ ] Acessar diretamente `/conversations`, `/patients`, `/appointments`, `/specialties` ou `/integrations/whatsapp` não libera operação clínica.
- [ ] Tentar `GET`/`POST` de catálogo com o token retorna `403` quando a rota exige policy clínica.

### ClinicAdmin

- [ ] `/setup` mostra a checklist do tenant autenticado, sem seleção manual de tenant.
- [ ] Os links de especialidades, profissionais, unidades e WhatsApp abrem o contexto correto.
- [ ] É possível cadastrar e alterar itens do próprio tenant.
- [ ] `/platform` retorna `403` e não aparece no menu.
- [ ] Um identificador de outro tenant não permite leitura ou alteração.

### Receptionist, Professional e Viewer

- [ ] O menu mostra somente as áreas previstas para o perfil.
- [ ] Rotas administrativas ocultadas no menu continuam protegidas quando acessadas diretamente.
- [ ] A API retorna `403` para mutações não autorizadas.
- [ ] Não há vazamento de dados de outro tenant nas respostas de leitura.

## Critério de aprovação

A validação passa quando todos os itens esperados forem confirmados, nenhum perfil clínico acessar `/platform`, e o `PlatformAdmin` não receber dados ou ações operacionais de clínica. Registre evidências (captura ou export do DevTools) para qualquer `403` esperado e abra uma pendência somente para respostas diferentes da matriz.
