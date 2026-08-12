# Checklist de prontidão para piloto

Status: `NOT READY — EXTERNAL AND COMMERCIAL GATES OPEN`

## Produto e operação

- [ ] clínica piloto, responsável e escopo definidos;
- [ ] ambiente Pilot/Staging separado de Development e Production;
- [ ] tenant e usuários criados com menor privilégio;
- [ ] agenda, profissionais, unidades e horários validados;
- [ ] fila humana e handoff testados;
- [x] FakeWhatsAppGateway validado antes do Twilio.

## Twilio e segurança

- [ ] HTTPS válido em `api.iarecepcao.com.br`;
- [ ] inbound webhook e StatusCallback configurados;
- [ ] assinatura Twilio validada;
- [ ] idempotência, replay, retry e correlation ID verificados;
- [ ] secrets apenas em secret store/environment;
- [ ] CORS limitado às origens necessárias.

## Dados e suporte

- [ ] backup e rollback testados;
- [x] logs sem PII desnecessária;
- [x] catálogo de eventos públicos sem PII implementado, aguardando escolha do fornecedor;
- [ ] métricas e alertas mínimos configurados no ambiente de piloto;
- [ ] canal de suporte e janela de atendimento definidos;
- [ ] plano de encerramento do piloto documentado.

## Validação visual pública

- [ ] landing revisada em 375, 390, 430, 768, 1024, 1280 e 1440px;
- [ ] login revisado em mobile e desktop;
- [ ] sidebar expandida e recolhida sem overflow;
- [ ] mark claro, escuro e monocromático revisado em 16/32/64px;
- [ ] favicon e app icon conferidos em navegador limpo.

## Aprovações

- [ ] preço, implantação, limites e consumo aprovados;
- [ ] calculadora aprovada para o nível de exposição escolhido;
- [ ] analytics aprovado ou explicitamente desabilitado;
- [ ] destino do lead flow aprovado.

## Validações concluídas no código

- [x] identidade D/variação 1 aplicada gradualmente;
- [x] assets claro, escuro e monocromático versionados;
- [x] metadata pública e favicon atualizados;
- [x] lint, typecheck, build e 34 testes do frontend aprovados;
- [x] eventos provider-neutral preparados sem rede, cookies ou PII.
- [x] Compose validado com a matriz de URLs públicas por ambiente.
