#!/usr/bin/env sh
set -eu
. "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
require_local_environment
compose logs --tail="${LOG_TAIL:-200}" "${1:-api}" 
