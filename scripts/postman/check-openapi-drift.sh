#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
collection="$root/docs/postman/clinic-assistant.postman_collection.json"
openapi_url="${1:-http://127.0.0.1:8080/swagger/v1/swagger.json}"

openapi_paths="$(mktemp)"
collection_paths="$(mktemp)"
trap 'rm -f "$openapi_paths" "$collection_paths"' EXIT

curl --fail --silent --show-error "$openapi_url" |
  jq -r '.paths | keys[]' |
  sed -E 's/\{[^}]+\}/\{\}/g' |
  sort -u > "$openapi_paths"

jq -r '.. | objects | select(has("request")) | .request.url | if type == "string" then . else .raw end' "$collection" |
  sed -E 's#^\{\{baseUrl\}\}##; s/[?].*$//; s#/(status)/[^/]+#\/\1/{}#; s/\{\{[^}]+\}\}/\{\}/g; s/\{[^}]+\}/\{\}/g' |
  grep -Ev '^/(health|swagger)' |
  sort -u > "$collection_paths"

missing=0
while IFS= read -r path; do
  if ! grep -Fqx "$path" "$openapi_paths"; then
    echo "Request Postman sem rota correspondente no OpenAPI: $path" >&2
    missing=1
  fi
done < "$collection_paths"

if [[ "$missing" -eq 0 ]]; then
  echo "OpenAPI e collection Postman: rotas alinhadas."
fi

exit "$missing"
