# Fixtures E2E estáveis

O manifesto canônico é [manifest.json](../../database/seeds/e2e/manifest.json). Testes devem consumi-lo em vez de repetir UUIDs.

| Fixture | UUID |
| --- | --- |
| Tenant principal | `00000000-0000-0000-0000-000000000101` |
| Tenant isolado | `00000000-0000-0000-0000-000000000102` |
| Admin | `00000000-0000-0000-0000-000000000201` |
| Paciente principal | `00000000-0000-0000-0000-000000000301` |
| Conversa aguardando humano | `00000000-0000-0000-0000-000000000401` |
| Consulta reagendável | `00000000-0000-0000-0000-000000000503` |

Os usuários E2E são `admin`, `manager`, `reception`, `operator`, `operator2` e `viewer`, todos no domínio `fake.local`. A senha é recebida de `E2E_DEFAULT_PASSWORD` (padrão apenas para desenvolvimento: `ClinicAssistant-E2E-Only-2026`) e nunca é persistida em texto puro.
