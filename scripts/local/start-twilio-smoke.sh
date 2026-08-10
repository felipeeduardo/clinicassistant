#!/usr/bin/env sh
set -eu
. "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
require_local_environment
: "${TWILIO_ACCOUNT_SID:?Set TWILIO_ACCOUNT_SID in the environment.}"
: "${TWILIO_AUTH_TOKEN:?Set TWILIO_AUTH_TOKEN in the environment.}"
: "${TWILIO_WHATSAPP_FROM:?Set TWILIO_WHATSAPP_FROM in the environment.}"
: "${TWILIO_INTEGRATION_KEY:?Set TWILIO_INTEGRATION_KEY for the connected Twilio integration.}"
[ "${DATABASE_TARGET:-}" = "primary" ] || fail "Twilio Sandbox smoke must use DATABASE_TARGET=primary."
[ "${WHATSAPP_PROVIDER:-}" = "Twilio" ] || fail "Twilio Sandbox smoke requires WHATSAPP_PROVIDER=Twilio."
compose_twilio() { docker compose -f "$ROOT_DIR/docker-compose.yml" -f "$ROOT_DIR/docker-compose.twilio-smoke.yml" --profile twilio-smoke "$@"; }
# A previous local container can keep the account's temporary endpoint online.
# Remove only the local ngrok container before starting a new smoke session.
compose_twilio rm -sf ngrok >/dev/null 2>&1 || true
compose_twilio up -d --build postgres rabbitmq redis api worker frontend ngrok
wait_for_url http://localhost:8080/health/ready
wait_for_url http://localhost:4040/api/tunnels
mkdir -p "$ROOT_DIR/.tmp"
ngrok_url=""
for attempt in $(seq 1 30); do
  ngrok_url="$(curl -fsS http://localhost:4040/api/tunnels | sed -n 's/.*"public_url":"\(https:\/\/[^" ]*\)".*/\1/p' | head -n 1)"
  [ -n "$ngrok_url" ] && break
  sleep 1
done
[ -n "$ngrok_url" ] || fail "ngrok inspector is available, but no active HTTPS tunnel was created. Stop any existing endpoint in the ngrok dashboard and retry."
printf '%s\n' "$ngrok_url" > "$ROOT_DIR/.tmp/ngrok-url"
# Twilio signs the complete public URL. Recreate the API/Worker with the
# discovered ngrok base URLs so validation never falls back to the Docker host.
export TWILIO_INCOMING_WEBHOOK_BASE_URL="$ngrok_url"
export TWILIO_STATUS_CALLBACK_BASE_URL="$ngrok_url"
export TWILIO_STATUS_CALLBACK_URL="$ngrok_url/api/webhooks/whatsapp/twilio/status/$TWILIO_INTEGRATION_KEY"
compose_twilio up -d --force-recreate --no-deps api worker frontend
wait_for_url http://localhost:8080/health/ready
printf '%s\n' "Twilio smoke environment ready (manual only; no message was sent automatically)."
printf 'Inbound webhook: %s/api/webhooks/whatsapp/twilio/%s\n' "$(cat "$ROOT_DIR/.tmp/ngrok-url")" "$TWILIO_INTEGRATION_KEY"
printf 'Status callback: %s/api/webhooks/whatsapp/twilio/status/%s\n' "$(cat "$ROOT_DIR/.tmp/ngrok-url")" "$TWILIO_INTEGRATION_KEY"
