# Provisionamento inicial de clínica

PlatformAdmin executa somente o provisionamento global. `POST /api/platform/onboarding` recebe um único comando transacional com clínica, unidade inicial e ClinicAdmin. O `Idempotency-Key` torna a operação segura para retry; em caso de falha, nenhum dos três recursos é persistido.

O tenant começa em `Provisioning` (exibido como **Em configuração**). O PlatformAdmin pode apenas visualizar o estado e executar ações globais de ciclo de vida. Especialidades, profissionais, disponibilidade, usuários internos, agenda e WhatsApp pertencem ao ClinicAdmin.

Após o primeiro login, o ClinicAdmin acessa `/setup`. O endpoint `GET /api/clinics/current/setup` resolve o tenant pelas claims e calcula o progresso a partir dos dados reais, sem seleção manual de tenant.
### Cadastro inicial simplificado

O endpoint de provisioning cria transacionalmente o tenant, a clínica, a unidade inicial e o ClinicAdmin. O tenant permanece com status `Provisioning` (exibido como **Em configuração**) e o frontend retorna para `/platform`. Especialidades, profissionais, disponibilidade, usuários operacionais, agenda e WhatsApp são configurados posteriormente pelo ClinicAdmin dentro do próprio tenant.

### WhatsApp no hub da plataforma

O hub consulta `GET /api/platform/tenants/{tenantId}/whatsapp` com autorização `PlatformAdmin`. A resposta informa apenas se há integração configurada, provedor, status operacional, telefone mascarado e os últimos eventos de webhook/envio; credenciais, tokens e a chave de integração nunca são retornados. A configuração ou habilitação do canal continua sendo executada no contexto da clínica por um `ClinicAdmin`, preservando o isolamento e o princípio de menor privilégio.

Após o primeiro login, o ClinicAdmin acessa `/setup` para acompanhar uma checklist modular. O endpoint `GET /api/clinics/current/setup` resolve o tenant diretamente pelas claims e deriva o progresso dos dados persistidos; não há seleção manual de tenant nem etapa obrigatória artificial.

Payload de provisionamento:

```json
{
  "tenantName": "Clínica Vida",
  "tenantSlug": "clinica-vida",
  "clinicLegalName": "Clínica Vida Serviços de Saúde LTDA",
  "clinicTradeName": "Clínica Vida",
  "clinicDocument": "00.000.000/0000-00",
  "clinicEmail": "contato@clinicavida.com.br",
  "clinicPhone": "+5581999999999",
  "timeZone": "America/Recife",
  "unitName": "Unidade principal",
  "unitAddress": "Av. Exemplo, 100",
  "unitPhone": "+5581999999999",
  "adminName": "Maria da Silva",
  "adminEmail": "admin@clinicavida.com.br",
  "temporaryPassword": "TroqueEstaSenha!2026"
}
```

A senha temporária é armazenada somente como hash e deve ser entregue por canal seguro. Nunca deve ser versionada em collection, environment ou documentação real.
