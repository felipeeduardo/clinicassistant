# Validação manual do Dashboard — Etapa 9.8

## Pré-requisitos

1. Subir API, banco, worker e frontend.
2. Entrar com um usuário autorizado do tenant.
3. Garantir que exista ao menos uma consulta, uma conversa e uma integração WhatsApp no ambiente de validação.

## Períodos

- Acessar `/dashboard` e validar `Hoje`, `7 dias`, `30 dias` e `Este mês`.
- Selecionar `Personalizado`, informar datas válidas e confirmar que a URL contém `period=custom`, `from` e `to`.
- Informar um intervalo inválido e confirmar que nenhum período inconsistente é enviado.

## Widgets e drill-down

- Confirmar que os KPIs exibem números reais ou estado vazio, sem valores inventados.
- No widget WhatsApp, conferir recebidas, enviadas, falhas e conversas abertas.
- Clicar em `Operar WhatsApp` e confirmar a abertura da área de integração.
- Clicar em `Ver agenda` ou em uma próxima consulta e confirmar a data filtrada.
- Clicar em `Abrir conversas`, `Ver fila` e `Consultar auditoria` no bloco de acesso rápido.
- Quando houver falha de envio ou fila, clicar no alerta e confirmar o destino operacional.

## Responsividade

Validar em 1440px, 1024px e 390px:

- nenhum scroll horizontal;
- filtros e períodos quebram em linhas legíveis;
- cards não cortam títulos ou valores;
- links e botões têm área de toque confortável;
- skeleton, erro e estado vazio continuam compreensíveis.

## Critério de aprovação

Registrar data, ambiente, usuário e evidências visuais. A etapa só deve ser encerrada quando os itens acima estiverem marcados e não houver erro funcional bloqueante.
