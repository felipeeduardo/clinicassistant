# ngrok

O profile `twilio-smoke` expõe o inspector em `http://localhost:4040`. O script consulta `/api/tunnels`, extrai apenas a URL HTTPS e grava em `.tmp/ngrok-url` (ignorado pelo Git).
