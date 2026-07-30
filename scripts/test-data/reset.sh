#!/usr/bin/env sh
set -eu
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/lib.sh"
PROFILE="${1:-}"; [ "$PROFILE" = "minimal" ] || [ "$PROFILE" = "e2e" ] || [ "$PROFILE" = "tenant" ] || fail "usage: reset.sh <minimal|e2e|tenant <tenant-id>>"
TENANT_ID="${2:-}"
[ "$PROFILE" != "tenant" ] || [ -n "$TENANT_ID" ] || fail "tenant id is required."
assert_safe_environment
confirm_reset
assert_migrations
PASSWORD_HASH=""
run_sql "$TEST_DATA_PROJECT_ROOT/database/reset/reset_test_data.sql"
printf '%s\n' "test-data: '$PROFILE' reset completed."
