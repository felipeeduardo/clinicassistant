#!/usr/bin/env sh
set -eu
. "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)/lib.sh"
require_local_environment
export DATABASE_NAME=clinicassistant_test TEST_DATA_ALLOWED_DATABASES=clinicassistant_test ALLOW_TEST_DATA_RESET=true TEST_DATA_CONFIRM=YES
compose --profile e2e run --rm test-data-seeder e2e
