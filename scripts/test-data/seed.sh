#!/usr/bin/env sh
set -eu
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/lib.sh"
PROFILE="${1:-}"; [ "$PROFILE" = "minimal" ] || [ "$PROFILE" = "e2e" ] || fail "usage: seed.sh <minimal|e2e>"
assert_safe_environment
assert_migrations
PASSWORD_HASH="$(generate_password_hash)"
for file in "$TEST_DATA_PROJECT_ROOT/database/seeds/common"/*.sql "$TEST_DATA_PROJECT_ROOT/database/seeds/$PROFILE"/*.sql; do run_sql "$file"; done
printf '%s\n' "test-data: '$PROFILE' seed completed."
