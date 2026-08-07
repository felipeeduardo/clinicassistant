#!/usr/bin/env sh
set -eu
. "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
require_local_environment
wait_for_url http://localhost:8080/health/ready
wait_for_url http://localhost:3000/login
printf '%s\n' "Local validation passed. Provider defaults to Fake; no outbound message was sent."
