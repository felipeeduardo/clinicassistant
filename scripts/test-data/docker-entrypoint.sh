#!/usr/bin/env sh
set -eu
profile="${1:-e2e}"
[ "$profile" = "minimal" ] || [ "$profile" = "e2e" ] || { echo "usage: test-data-seeder [minimal|e2e]" >&2; exit 2; }
/scripts/wait-for-migrations.sh
/scripts/reset.sh "$profile"
/scripts/seed.sh "$profile"
/scripts/validate.sh "$profile"
