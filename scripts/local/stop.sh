#!/usr/bin/env sh
set -eu
. "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
require_local_environment
compose down
if [ "${STOP_TWILIO_SMOKE:-false}" = "true" ]; then compose -f "$ROOT_DIR/docker-compose.yml" -f "$ROOT_DIR/docker-compose.twilio-smoke.yml" --profile twilio-smoke down; fi
