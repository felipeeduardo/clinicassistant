#!/usr/bin/env sh
set -eu
. "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
require_local_environment
export DATABASE_TARGET=test DATABASE_NAME=clinicassistant_test TEST_DATA_ALLOWED_DATABASES=clinicassistant_test WHATSAPP_PROVIDER=Fake
export E2E_DEFAULT_PASSWORD="${E2E_DEFAULT_PASSWORD:-ClinicAssistant-E2E-Only-2026}"
compose --profile e2e up -d --build postgres rabbitmq redis api worker frontend
wait_for_url http://localhost:8080/health/ready
wait_for_url http://localhost:3000/login
compose --profile e2e run --rm test-data-seeder e2e
printf '%s\n' "E2E environment ready (provider: Fake; external calls: disabled)."
show_urls
