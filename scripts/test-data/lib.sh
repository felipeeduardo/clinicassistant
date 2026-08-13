#!/usr/bin/env sh
set -eu

TEST_DATA_PROJECT_ROOT="${TEST_DATA_PROJECT_ROOT:-$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)}"

# Local scripts are expected to work from a freshly cloned workspace. Load the
# project .env only when the caller has not already supplied a target database.
# Explicitly exported DATABASE_NAME remains the highest-priority override.
if [ -z "${DATABASE_NAME:-}" ] && [ -f "$TEST_DATA_PROJECT_ROOT/.env" ]; then
  set -a
  . "$TEST_DATA_PROJECT_ROOT/.env"
  set +a
fi
DATABASE_NAME="${DATABASE_NAME:-${POSTGRES_DB:-clinicassistant}}"
DATABASE_HOST="${DATABASE_HOST:-${POSTGRES_HOST:-localhost}}"
DATABASE_PORT="${DATABASE_PORT:-${POSTGRES_PORT:-5432}}"
DATABASE_USER="${DATABASE_USER:-${POSTGRES_USER:-clinicassistant}}"
DATABASE_PASSWORD="${DATABASE_PASSWORD:-${POSTGRES_PASSWORD:-clinicassistant}}"
E2E_BASE_DATE="${E2E_BASE_DATE:-2026-08-03}"
WHATSAPP_PROVIDER="${WHATSAPP_PROVIDER:-Fake}"
TWILIO_INTEGRATION_KEY="${TWILIO_INTEGRATION_KEY:-twilio-local-main}"
TWILIO_WHATSAPP_FROM="${TWILIO_WHATSAPP_FROM:-whatsapp:+5500000000000}"
TWILIO_DISPLAY_PHONE_NUMBER="${TWILIO_DISPLAY_PHONE_NUMBER:-${TWILIO_WHATSAPP_FROM#whatsapp:}}"

fail() { printf '%s\n' "test-data: $*" >&2; exit 1; }

require_psql() {
  command -v psql >/dev/null 2>&1 || fail "psql is required for local test-data scripts. macOS: brew install libpq && echo 'export PATH=\"/opt/homebrew/opt/libpq/bin:\$PATH\"' >> ~/.zshrc && source ~/.zshrc. Or run: docker compose --profile e2e run --rm test-data-seeder e2e"
}

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
  PGPASSWORD="$DATABASE_PASSWORD" psql $(psql_args) -X -v ON_ERROR_STOP=1 -v password_hash="$PASSWORD_HASH" -v base_date="$E2E_BASE_DATE" -v profile="$PROFILE" -v whatsapp_provider="$WHATSAPP_PROVIDER" -v twilio_integration_key="$TWILIO_INTEGRATION_KEY" -v twilio_whatsapp_from="$TWILIO_WHATSAPP_FROM" -v twilio_display_phone_number="$TWILIO_DISPLAY_PHONE_NUMBER" -v tenant_id="${TENANT_ID:-00000000-0000-0000-0000-000000000000}" -c "SET test_data.profile TO '$PROFILE'; SET test_data.whatsapp_provider TO '$WHATSAPP_PROVIDER';" -f "$file"
}

migrations_ready() {
  PGPASSWORD="$DATABASE_PASSWORD" psql $(psql_args) -X -v ON_ERROR_STOP=1 -qc "DO \$\$
  DECLARE history_schema text; migration_exists boolean;
  BEGIN
    SELECT table_schema INTO history_schema
    FROM information_schema.tables
    WHERE table_name = '__EFMigrationsHistory' AND table_schema IN ('public', 'clinic_assistant')
    ORDER BY CASE table_schema WHEN 'public' THEN 1 ELSE 2 END
    LIMIT 1;
    IF history_schema IS NULL THEN RAISE EXCEPTION 'EF Core migrations history was not found.'; END IF;
    EXECUTE format('SELECT EXISTS (SELECT 1 FROM %I.\"__EFMigrationsHistory\" WHERE \"MigrationId\" = %L)', history_schema, '202607300009_HumanQueue') INTO migration_exists;
    IF NOT migration_exists THEN RAISE EXCEPTION 'Required migration 202607300009_HumanQueue was not found.'; END IF;
  END \$\$;"
}

assert_migrations() {
  require_psql
  migrations_ready || fail "migration 202607300009_HumanQueue was not found in '$DATABASE_NAME' at $DATABASE_HOST:$DATABASE_PORT. Start the API (it applies migrations on startup) or apply migrations before running test-data scripts."
}

generate_password_hash() {
  : "${E2E_DEFAULT_PASSWORD:=ClinicAssistant-E2E-Only-2026}"
  export E2E_DEFAULT_PASSWORD
  dotnet run --project "$TEST_DATA_PROJECT_ROOT/backend/tools/ClinicAssistant.TestDataHash/ClinicAssistant.TestDataHash.csproj" 2>/dev/null || fail "could not generate the password hash with the application PasswordHasher."
}

confirm_reset() {
  [ "${ALLOW_TEST_DATA_RESET:-}" = "true" ] || fail "ALLOW_TEST_DATA_RESET=true is required."
  if [ "${CI:-}" = "true" ] || [ "${TEST_DATA_CONFIRM:-}" = "YES" ]; then return 0; fi
  [ -t 0 ] || fail "set TEST_DATA_CONFIRM=YES for a non-interactive reset."
  printf "Reset test data from %s? Type RESET: " "$DATABASE_NAME" >&2
  read -r confirmation
  [ "$confirmation" = "RESET" ] || fail "reset cancelled."
}
