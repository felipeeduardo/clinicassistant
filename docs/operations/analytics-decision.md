# Decisão de analytics

Status: `APPROVAL REQUIRED`

| Opção | Privacidade/LGPD | Custo | Manutenção | Observação |
|---|---|---|---|---|
| Nenhum fornecedor, eventos internos | Alta | Baixo | Baixa | Recomendado para o MVP inicial |
| Analytics self-hosted | Alta, depende da configuração | Infra própria | Média | Requer operação e retenção definidas |
| Fornecedor SaaS | Depende do contrato/cookies | Variável | Baixa | Requer DPA, consentimento e revisão jurídica |

Eventos previstos, sempre sem PII:

`landing_view`, `hero_demo_started`, `hero_demo_completed`, `product_tab_changed`, `pricing_viewed`, `pricing_cta_clicked`, `roi_calculator_started`, `roi_calculator_completed`, `demo_cta_clicked`, `pilot_cta_clicked`, `lead_started`, `lead_submitted`, `lead_failed`.

Não enviar nome, e-mail, telefone, clínica, conteúdo de mensagens ou identificadores de pacientes.

## Implementação neutra disponível

Enquanto o fornecedor não é aprovado, o frontend possui `trackPublicEvent` em `frontend/lib/analytics/public-events.ts`. Ele apenas dispara um evento local no browser e não faz rede, cookies ou persistência. A troca por um fornecedor aprovado deve preservar a lista de eventos e a regra de não enviar PII.
