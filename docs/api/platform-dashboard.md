# API — Dashboard PlatformAdmin

`GET /api/platform/dashboard?period=30d`

Autenticação: Bearer token com role `PlatformAdmin`. Os valores aceitos para `period` são `7d`, `30d` e `90d`; o padrão é `30d`. O endpoint também aceita `from` e `to` em ISO-8601 para uma janela customizada de até 90 dias.

O retorno contém `summary`, `commercial`, `growth`, `clinics`, `health`, `attention` e `recentActivity`. Os dados são agregados no servidor e não incluem segredos, credenciais, pacientes ou conteúdo de conversas.

Perfis ClinicAdmin, Receptionist e usuários não autenticados recebem `403`/`401` conforme a camada de autenticação.

