#!/usr/bin/env sh
set -eu
. "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
require_local_environment
: "${TWILIO_ACCOUNT_SID:?Set TWILIO_ACCOUNT_SID in the environment.}"
: "${TWILIO_AUTH_TOKEN:?Set TWILIO_AUTH_TOKEN in the environment.}"
: "${TWILIO_WHATSAPP_FROM:?Set TWILIO_WHATSAPP_FROM in the environment.}"
export DATABASE_TARGET=primary WHATSAPP_PROVIDER=Twilio
compose -f "$ROOT_DIR/docker-compose.yml" -f "$ROOT_DIR/docker-compose.twilio-smoke.yml" --profile twilio-smoke up -d --build postgres rabbitmq redis api worker frontend ngrok
wait_for_url http://localhost:8080/health/ready
wait_for_url http://localhost:4040/api/tunnels
mkdir -p "$ROOT_DIR/.tmp"
curl -fsS http://localhost:4040/api/tunnels | sed -n 's/.*"public_url":"\(https:\/\/[^" ]*\)".*/\1/p' | head -n 1 > "$ROOT_DIR/.tmp/ngrok-url"
printf '%s\n' "Twilio smoke environment ready (manual only; no message was sent automatically)."
printf 'Inbound webhook: https://%s/api/webhooks/twilio/whatsapp/inbound\n' "$(cat "$ROOT_DIR/.tmp/ngrok-url")"
printf 'Status callback: https://%s/api/webhooks/twilio/whatsapp/status\n' "$(cat "$ROOT_DIR/.tmp/ngrok-url")"
