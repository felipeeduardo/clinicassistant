# Cenários E2E do Dashboard

Com os serviços e seed determinísticos disponíveis:

1. abrir `/dashboard` autenticado;
2. alternar Hoje, 7 dias, 30 dias e Este mês;
3. confirmar persistência de `?period=`;
4. validar KPIs sem dados fictícios;
5. clicar em Consultas, Pendentes, Fila humana e Falhas WhatsApp;
6. validar estado vazio/atenção quando os contadores forem zero;
7. simular erro da API e confirmar “Tentar novamente”.
