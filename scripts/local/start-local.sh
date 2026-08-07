#!/usr/bin/env sh
set -eu
. "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
require_local_environment
export WHATSAPP_PROVIDER=Fake
compose up -d --build postgres rabbitmq redis api worker frontend
wait_for_url http://localhost:8080/health/ready
wait_for_url http://localhost:3000/login
printf '%s\n' "Local environment ready (provider: Fake; Twilio/ngrok: disabled)."
show_urls
