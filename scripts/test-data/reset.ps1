param([Parameter(Mandatory = $true)][ValidateSet('minimal','e2e','tenant')][string]$Profile,[string]$TenantId)
$ErrorActionPreference = 'Stop'
if ($env:ASPNETCORE_ENVIRONMENT -eq 'Production') { throw 'test-data: execution is blocked in Production.' }
if ($env:ALLOW_TEST_DATA_RESET -ne 'true') { throw 'test-data: ALLOW_TEST_DATA_RESET=true is required.' }
if ($Profile -eq 'tenant' -and -not $TenantId) { throw 'test-data: tenant id is required.' }
if ($env:CI -ne 'true' -and $env:TEST_DATA_CONFIRM -ne 'YES') { $answer = Read-Host 'Type RESET to confirm'; if ($answer -ne 'RESET') { throw 'test-data: reset cancelled.' } }
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot); $database = $env:DATABASE_NAME ?? $env:POSTGRES_DB ?? 'clinicassistant'
$env:PGPASSWORD = $env:DATABASE_PASSWORD ?? $env:POSTGRES_PASSWORD ?? 'clinicassistant'
& psql -h ($env:DATABASE_HOST ?? 'localhost') -p ($env:DATABASE_PORT ?? '5432') -U ($env:DATABASE_USER ?? $env:POSTGRES_USER ?? 'clinicassistant') -d $database -X -v ON_ERROR_STOP=1 -v "profile=$Profile" -v "tenant_id=$TenantId" -f "$root/database/reset/reset_test_data.sql"
if ($LASTEXITCODE) { throw 'test-data: reset failed.' }
