#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
collection="$root/docs/postman/clinic-assistant.postman_collection.json"
environments=("$root"/docs/postman/environments/*.json)
data_files=("$root"/docs/postman/data/*.json)

jq empty "$collection" "${environments[@]}" "${data_files[@]}"

used_variables="$(mktemp)"
defined_variables="$(mktemp)"
trap 'rm -f "$used_variables" "$defined_variables"' EXIT

jq -r '.. | strings | scan("\\{\\{([A-Za-z][A-Za-z0-9_]*)\\}\\}")[]' "$collection" | sort -u > "$used_variables"
jq -r '.variable[]?.key' "$collection" | sort -u > "$defined_variables"

if missing="$(comm -23 "$used_variables" "$defined_variables")" && [[ -n "$missing" ]]; then
  printf 'Variáveis Postman usadas sem declaração:\n%s\n' "$missing" >&2
  exit 1
fi

if jq -e '.. | objects | select(has("request")) | .request.url | if type == "string" then . else (.raw // "") end | select(test("^https?://localhost"; "i"))' "$collection" >/dev/null; then
  echo "A collection não pode conter URLs localhost fixas; use {{baseUrl}}." >&2
  exit 1
fi

if jq -e '.. | objects | select(.key? == "twilioAuthToken")' "$collection" "${environments[@]}" >/dev/null; then
  echo "twilioAuthToken não pode ser versionado em Postman." >&2
  exit 1
fi

if jq -e '.values[]? | select((.key | test("(password|token|secret|api.?key)"; "i")) and (.value | type == "string" and length > 0))' "${environments[@]}" >/dev/null; then
  echo "Environments Postman versionados não podem conter segredo preenchido." >&2
  exit 1
fi

echo "Collection e environments Postman: OK."
