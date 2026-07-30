#!/usr/bin/env sh
set -eu
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/lib.sh"
PROFILE="${1:-}"; [ "$PROFILE" = "minimal" ] || [ "$PROFILE" = "e2e" ] || fail "usage: validate.sh <minimal|e2e>"
assert_safe_environment
assert_migrations
PASSWORD_HASH=""
for file in "$TEST_DATA_PROJECT_ROOT/database/validation"/*.sql; do run_sql "$file"; done
printf '%s\n' "test-data: '$PROFILE' validation completed."
