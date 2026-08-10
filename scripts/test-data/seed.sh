#!/usr/bin/env sh
set -eu
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/lib.sh"
PROFILE="${1:-}"; [ "$PROFILE" = "minimal" ] || [ "$PROFILE" = "e2e" ] || fail "usage: seed.sh <minimal|e2e>"
assert_safe_environment
assert_migrations
WHATSAPP_PROVIDER="${WHATSAPP_PROVIDER:-Fake}"
case "$WHATSAPP_PROVIDER" in
  Fake) TWILIO_INTEGRATION_KEY="${TWILIO_INTEGRATION_KEY:-twilio-local-main}"; TWILIO_WHATSAPP_FROM="${TWILIO_WHATSAPP_FROM:-whatsapp:+5500000000000}" ;;
  Twilio)
    : "${TWILIO_WHATSAPP_FROM:?TWILIO_WHATSAPP_FROM is required when WHATSAPP_PROVIDER=Twilio.}"
    TWILIO_INTEGRATION_KEY="${TWILIO_INTEGRATION_KEY:-twilio-local-main}"
    case "$TWILIO_WHATSAPP_FROM" in whatsapp:*) ;; +*) TWILIO_WHATSAPP_FROM="whatsapp:$TWILIO_WHATSAPP_FROM" ;; *) fail "TWILIO_WHATSAPP_FROM must be an E.164 number or start with whatsapp:." ;; esac
    ;;
  *) fail "WHATSAPP_PROVIDER must be Fake or Twilio." ;;
esac
TWILIO_DISPLAY_PHONE_NUMBER="${TWILIO_DISPLAY_PHONE_NUMBER:-${TWILIO_WHATSAPP_FROM#whatsapp:}}"
export WHATSAPP_PROVIDER TWILIO_INTEGRATION_KEY TWILIO_WHATSAPP_FROM TWILIO_DISPLAY_PHONE_NUMBER
PASSWORD_HASH="$(generate_password_hash)"
for file in "$TEST_DATA_PROJECT_ROOT/database/seeds/common"/*.sql "$TEST_DATA_PROJECT_ROOT/database/seeds/$PROFILE"/*.sql; do run_sql "$file"; done
printf '%s\n' "test-data: '$PROFILE' seed completed."
