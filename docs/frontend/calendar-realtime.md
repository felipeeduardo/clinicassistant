# Realtime da agenda

A toolbar apresenta o estado do canal SignalR por meio do contexto global: `Connected`, `Reconnecting` ou `Offline`. A ponte realtime já invalida as chaves `appointments` e `availability` para eventos de criação, atualização e cancelamento, fazendo a agenda consultar novamente apenas o período/filtros ativos. A fonte de dados continua sendo a API de consultas e as regras permanecem no backend.

São tratados eventos de criação, atualização, confirmação, reagendamento e cancelamento de consultas, além de disponibilidade, bloqueios e férias. A invalidação ampla de disponibilidade é o fallback seguro até que os contratos tragam o identificador e o período afetado para atualização incremental mais granular.
