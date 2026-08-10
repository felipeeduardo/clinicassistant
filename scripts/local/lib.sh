#!/usr/bin/env sh
set -eu

ROOT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
COMPOSE="docker compose"

# Load the ignored local environment for every local helper. Explicit exports
# made after this file is sourced still override these defaults.
if [ -f "$ROOT_DIR/.env" ]; then
  set -a
  . "$ROOT_DIR/.env"
  set +a
fi

fail() { printf 'local: %s\n' "$*" >&2; exit 1; }
require_command() { command -v "$1" >/dev/null 2>&1 || fail "required command not found: $1"; }
require_local_environment() {
  [ "${ASPNETCORE_ENVIRONMENT:-Development}" != "Production" ] || fail "local scripts are blocked in Production.";
  require_command docker
  docker info >/dev/null 2>&1 || fail "Docker Desktop is not running or is inaccessible."
}
compose() { $COMPOSE -f "$ROOT_DIR/docker-compose.yml" "$@"; }
wait_for_url() {
  url="$1"; attempts="${2:-30}"
  i=0
  while [ "$i" -lt "$attempts" ]; do
    if curl -fsS --max-time 3 "$url" >/dev/null 2>&1; then return 0; fi
    i=$((i + 1)); sleep 2
  done
  fail "service did not become ready: $url"
}
show_urls() {
  printf '%s\n' "API:      http://localhost:8080" "Frontend: http://localhost:3000" "Swagger:  http://localhost:8080/swagger"
}
