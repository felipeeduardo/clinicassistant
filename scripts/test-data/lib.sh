#!/usr/bin/env sh
set -eu

TEST_DATA_PROJECT_ROOT="${TEST_DATA_PROJECT_ROOT:-$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)}"
DATABASE_NAME="${DATABASE_NAME:-${POSTGRES_DB:-clinicassistant}}"
DATABASE_HOST="${DATABASE_HOST:-${POSTGRES_HOST:-localhost}}"
DATABASE_PORT="${DATABASE_PORT:-${POSTGRES_PORT:-5432}}"
DATABASE_USER="${DATABASE_USER:-${POSTGRES_USER:-clinicassistant}}"
DATABASE_PASSWORD="${DATABASE_PASSWORD:-${POSTGRES_PASSWORD:-clinicassistant}}"
E2E_BASE_DATE="${E2E_BASE_DATE:-2026-08-03}"

fail() { printf '%s\n' "test-data: $*" >&2; exit 1; }

assert_safe_environment() {
  [ "${ASPNETCORE_ENVIRONMENT:-Development}" != "Production" ] || fail "execution is blocked in Production."
  case "$DATABASE_NAME" in
    *test*|*Test*|*e2e*|*E2E*|*dev*|*Dev*) return 0 ;;
  esac
  case ",${TEST_DATA_ALLOWED_DATABASES:-}," in
    *",$DATABASE_NAME,"*) return 0 ;;
  esac
  fail "database '$DATABASE_NAME' is not an explicit test/development database. Set TEST_DATA_ALLOWED_DATABASES to allow it."
}

psql_args() {
  printf '%s\n' "-h" "$DATABASE_HOST" "-p" "$DATABASE_PORT" "-U" "$DATABASE_USER" "-d" "$DATABASE_NAME"
}

run_sql() {
  file="$1"
  PGPASSWORD="$DATABASE_PASSWORD" psql $(psql_args) -X -v ON_ERROR_STOP=1 -v password_hash="$PASSWORD_HASH" -v base_date="$E2E_BASE_DATE" -v profile="$PROFILE" -v tenant_id="${TENANT_ID:-00000000-0000-0000-0000-000000000000}" -c "SET test_data.profile TO '$PROFILE';" -f "$file"
}

migrations_ready() {
  PGPASSWORD="$DATABASE_PASSWORD" psql $(psql_args) -X -v ON_ERROR_STOP=1 -Atqc "SELECT CASE WHEN to_regclass('clinic_assistant.__EFMigrationsHistory') IS NOT NULL AND EXISTS (SELECT 1 FROM clinic_assistant.\"__EFMigrationsHistory\" WHERE \"MigrationId\" = '202607300009_HumanQueue') THEN 'ok' ELSE 'missing' END" 2>/dev/null | grep -qx ok
}

assert_migrations() { migrations_ready || fail "the expected EF Core migrations have not been applied."; }

generate_password_hash() {
  : "${E2E_DEFAULT_PASSWORD:=ClinicAssistant-E2E-Only-2026}"
  export E2E_DEFAULT_PASSWORD
  dotnet run --project "$TEST_DATA_PROJECT_ROOT/tools/ClinicAssistant.TestDataHash/ClinicAssistant.TestDataHash.csproj" 2>/dev/null || fail "could not generate the password hash with the application PasswordHasher."
}

confirm_reset() {
  [ "${ALLOW_TEST_DATA_RESET:-}" = "true" ] || fail "ALLOW_TEST_DATA_RESET=true is required."
  if [ "${CI:-}" = "true" ] || [ "${TEST_DATA_CONFIRM:-}" = "YES" ]; then return 0; fi
  [ -t 0 ] || fail "set TEST_DATA_CONFIRM=YES for a non-interactive reset."
  printf "Reset test data from %s? Type RESET: " "$DATABASE_NAME" >&2
  read -r confirmation
  [ "$confirmation" = "RESET" ] || fail "reset cancelled."
}
