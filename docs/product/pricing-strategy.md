# Estratégia de pricing — hipótese validável

Não existe decisão comercial aprovada. O Clinic Assistant opera no modo `demo`; nenhum preço é publicado como definitivo.

A configuração fica centralizada em `frontend/lib/commercial/config.ts`. `NEXT_PUBLIC_COMMERCIAL_MODE` aceita `demo`, `pilot` ou `publicPricing`; o último só mostra preço quando `NEXT_PUBLIC_PUBLIC_PRICING_APPROVED=true` e um valor aprovado for fornecido no código de configuração. A Landing não possui checkout nem altera cobrança real.

| Modelo | Vantagens | Riscos | Previsibilidade | Adequação ao MVP |
|---|---|---|---|---|
| Mensalidade fixa por clínica | Simples de explicar e vender | Pode subestimar consumo WhatsApp | Alta para cliente, média para operação | Boa para piloto controlado |
| Faixa por profissionais | Cresce com porte da clínica | Mais complexidade e degraus artificiais | Alta | Adequada após medir uso |
| Base + consumo | Protege margem variável | Difícil comunicação inicial | Média | Evitar antes de conhecer custos |
| Piloto para clínicas parceiras | Aprende operação real antes de precificar | Não representa preço final | Controlada por contrato | Recomendado agora |

## Hipótese recomendada

Usar piloto comercial com proposta individual, sem preço público, limites e condições definidos por aprovação humana. Custos Twilio/WhatsApp, infraestrutura, e-mail, observabilidade, storage e IA futura permanecem `TO VALIDATE`.

Decisões **APPROVAL REQUIRED**: preço, implantação, limites, consumo, piloto grátis/pago e publicação de calculadora.
