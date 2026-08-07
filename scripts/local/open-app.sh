#!/usr/bin/env sh
set -eu
if command -v open >/dev/null 2>&1; then open http://localhost:3000; else printf '%s\n' "Open http://localhost:3000"; fi
