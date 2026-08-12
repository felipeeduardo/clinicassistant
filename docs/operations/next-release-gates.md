# Gates restantes para o próximo release

Este documento separa o que já está validado no código do que exige ação humana ou configuração externa.

## Bloqueios externos

- DNS/TLS e hospedagem de `iarecepcao.com.br`, `app` e `api`;
- configuração de CORS e URLs por ambiente;
- webhooks e StatusCallback do Twilio em HTTPS;
- alertas e retenção de logs no ambiente Pilot/Staging.

## Bloqueios de decisão

- fornecedor de analytics;
- destino do lead flow;
- preço, implantação, limites e consumo;
- exposição pública da calculadora;
- condições do piloto.

## Critério de avanço

Não promover para Production enquanto qualquer bloqueio externo ou decisão marcada `APPROVAL REQUIRED` permanecer sem responsável, evidência e rollback documentados.
