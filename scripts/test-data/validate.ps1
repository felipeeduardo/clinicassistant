param([Parameter(Mandatory = $true)][ValidateSet('minimal','e2e')][string]$Profile)
$ErrorActionPreference = 'Stop'
if ($env:ASPNETCORE_ENVIRONMENT -eq 'Production') { throw 'test-data: execution is blocked in Production.' }
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot); $database = $env:DATABASE_NAME ?? $env:POSTGRES_DB ?? 'clinicassistant'
$env:PGPASSWORD = $env:DATABASE_PASSWORD ?? $env:POSTGRES_PASSWORD ?? 'clinicassistant'
$args = @('-h',($env:DATABASE_HOST ?? 'localhost'),'-p',($env:DATABASE_PORT ?? '5432'),'-U',($env:DATABASE_USER ?? $env:POSTGRES_USER ?? 'clinicassistant'),'-d',$database,'-X','-v','ON_ERROR_STOP=1','-v',"profile=$Profile")
Get-ChildItem "$root/database/validation/*.sql" | Sort-Object FullName | ForEach-Object { & psql @args -c "SET test_data.profile TO '$Profile';" -f $_.FullName; if ($LASTEXITCODE) { throw "psql failed: $($_.Name)" } }
