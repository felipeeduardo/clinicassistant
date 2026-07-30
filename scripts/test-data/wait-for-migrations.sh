#!/usr/bin/env sh
set -eu
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/lib.sh"
attempt=0
until migrations_ready; do
  attempt=$((attempt + 1))
  [ "$attempt" -lt 30 ] || fail "timed out waiting for EF Core migrations."
  sleep 2
done
