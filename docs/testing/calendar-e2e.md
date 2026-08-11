# Cenários manuais e E2E do calendário

Os cenários mínimos da primeira entrega são:

1. autenticar e abrir `/appointments`;
2. alternar Dia, Semana, Mês e Lista;
3. avançar, voltar e retornar a Hoje;
4. filtrar por paciente, profissional, unidade, especialidade e status;
5. validar estado vazio e carregamento;
6. abrir um evento e confirmar que o drawer preserva as operações existentes;
7. validar a leitura sem IDs técnicos e a rolagem em viewport estreito.
8. abrir a agenda com `?view=week&date=AAAA-MM-DD` e confirmar que o estado é restaurado e permanece na URL ao alterar filtros.

Os estados realtime e vazio também possuem cobertura unitária em `frontend/tests/calendar-components.test.tsx`; a execução Playwright continua dependente dos serviços locais e do seed E2E.

Os testes E2E devem usar o seed determinístico já documentado em `docs/testing/e2e-execution-guide.md`. Não devem criar consultas fora do ambiente de teste nem depender de horário corrente fixo.
