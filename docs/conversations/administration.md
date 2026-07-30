# APIs administrativas de conversas

As rotas sob `/api/conversations` exigem autenticação. Listagem, detalhe, histórico e marcação de leitura usam a política `ClinicStaff`; assumir, liberar, pausar e retomar automação exigem `ClinicAdmin`.

Mutações recebem `ExpectedVersion`. Se a versão estiver desatualizada, a API retorna `409 Conflict`; o cliente deve recarregar o detalhe antes de tentar novamente. Todas as consultas são limitadas ao tenant do JWT.

Rotas iniciais: listagem paginada, detalhe, mensagens, leitura, assumir, liberar e controle de automação. Fila humana persistida, envio manual e SignalR serão entregues posteriormente.
