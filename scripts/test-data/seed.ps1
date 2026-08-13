param([Parameter(Mandatory = $true)][ValidateSet('minimal','e2e')][string]$Profile)
$ErrorActionPreference = 'Stop'
if ($env:ASPNETCORE_ENVIRONMENT -eq 'Production') { throw 'test-data: execution is blocked in Production.' }
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$database = if ($env:DATABASE_NAME) { $env:DATABASE_NAME } elseif ($env:POSTGRES_DB) { $env:POSTGRES_DB } else { 'clinicassistant' }
if ($database -notmatch '(?i)test|e2e|dev' -and (',$($env:TEST_DATA_ALLOWED_DATABASES),' -notmatch ",$database,")) { throw "test-data: database '$database' is not explicitly allowed." }
$env:E2E_DEFAULT_PASSWORD = if ($env:E2E_DEFAULT_PASSWORD) { $env:E2E_DEFAULT_PASSWORD } else { 'ClinicAssistant-E2E-Only-2026' }
$hash = dotnet run --project "$root/backend/tools/ClinicAssistant.TestDataHash/ClinicAssistant.TestDataHash.csproj"
$args = @('-h',($env:DATABASE_HOST ?? 'localhost'),'-p',($env:DATABASE_PORT ?? '5432'),'-U',($env:DATABASE_USER ?? $env:POSTGRES_USER ?? 'clinicassistant'),'-d',$database,'-X','-v','ON_ERROR_STOP=1','-v',"password_hash=$hash",'-v',"base_date=$($env:E2E_BASE_DATE ?? '2026-08-03')",'-v',"profile=$Profile")
$env:PGPASSWORD = $env:DATABASE_PASSWORD ?? $env:POSTGRES_PASSWORD ?? 'clinicassistant'
Get-ChildItem "$root/database/seeds/common/*.sql", "$root/database/seeds/$Profile/*.sql" | Sort-Object FullName | ForEach-Object { & psql @args -f $_.FullName; if ($LASTEXITCODE) { throw "psql failed: $($_.Name)" } }
