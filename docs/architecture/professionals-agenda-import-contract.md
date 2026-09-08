# Contrato da importação de agenda (versão 1)

Esta é a definição da próxima subetapa do módulo Profissionais e Agenda. A importação será transacional por tenant e nunca poderá criar, alterar ou excluir consultas.

O parser e os endpoints já foram adicionados: `POST /api/professionals/import/preview` apenas valida; `POST /api/professionals/import/commit` revalida e grava em transação. O frontend expõe as duas etapas para usuários com `ProfessionalsManage`.

## Abas e colunas

O arquivo XLSX terá uma aba `agenda` com as colunas obrigatórias:

| Coluna | Uso |
| --- | --- |
| `Registro profissional` | Registro do profissional existente na clínica |
| `Tipo` | `Disponibilidade`, `Bloqueio` ou `Férias` |
| `Dia da semana` | Dia recorrente (obrigatório para disponibilidade) |
| `Data inicial` / `Data final` | Data brasileira `dd/MM/aaaa` para bloqueios e férias |
| `Hora inicial` / `Hora final` | Horário `HH:mm`; férias assumem 00:00 e 23:59 quando vazias |
| `Duração (min)` | Duração entre 5 e 240 minutos (somente disponibilidade) |
| `motivo` | Motivo opcional para bloqueio/férias |

O parser mantém compatibilidade com cabeçalhos técnicos antigos, mas o modelo oficial usa os nomes em português. Datas e horas são combinadas no fuso configurado pela clínica antes da conversão interna para UTC; o usuário nunca precisa informar ISO ou timezone.

## Fluxo obrigatório

1. Baixar o template versionado.
2. Enviar o arquivo para validação (limite de 5 MB e 5.000 linhas).
3. Exibir preview e todos os conflitos por linha, sem persistir nada.
4. Confirmar o commit com uma chave de idempotência.
5. Persistir somente regras, bloqueios e férias em uma transação; registrar auditoria e resultado.

Linhas inválidas, profissionais inexistentes, períodos sobrepostos e conflitos com consultas existentes impedem o commit inteiro. O worker, o Outbox e os fluxos de WhatsApp não participam da importação.

## Segurança e operação

- O `TenantAccessGuard` deve resolver o tenant em todas as consultas e escritas.
- O endpoint exige `ProfessionalsManage`.
- O histórico deve guardar nome do arquivo, quantidade de linhas, resultado e ator, sem armazenar o arquivo nem dados sensíveis.
- A implementação deve usar um parser XLSX aprovado no backend; não aceitar CSV renomeado como `.xlsx`.
