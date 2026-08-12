# Calculadora de impacto operacional

## Premissas

A calculadora é uma estimativa client-side. O usuário informa atendentes, custo mensal médio da equipe, solicitações repetitivas por dia, minutos por solicitação, dias úteis, percentual potencialmente automatizável e investimento mensal.

## Fórmulas

```text
minutos_repetitivos_mes = solicitações_dia × minutos_por_solicitação × dias_úteis
minutos_potencialmente_automatizáveis = minutos_repetitivos_mes × percentual / 100
horas_potencialmente_liberadas = minutos_potencialmente_automatizáveis / 60
custo_hora_estimado = custo_mensal_equipe / (atendentes × 160)
valor_equivalente_tempo = horas_potencialmente_liberadas × custo_hora_estimado
impacto_estimado = valor_equivalente_tempo − investimento_mensal
```

Valores negativos, não numéricos e percentuais acima de 100 são tratados com segurança. O resultado é chamado de **Impacto operacional estimado**, nunca de lucro ou economia garantida.

Disclaimer exibido obrigatoriamente: “Estimativa baseada nas informações fornecidas. Resultados reais dependem da operação, adesão, volume de atendimentos e configuração da clínica.”

Os presets atuais são apenas valores iniciais de interação, não benchmarks de clientes.
