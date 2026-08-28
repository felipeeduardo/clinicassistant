# Alertas iniciais

Após obter baseline, configure alertas para API 5xx > 5% ou P95 > 2s por 5
minutos, falhas de Worker/Outbox/WhatsApp/SendGrid/lembretes acima do baseline e
Outbox pendente acima do limite operacional. Os valores são sugestões e devem
ser ajustados; Railway continua sendo a fonte de deploy e crash logs.
