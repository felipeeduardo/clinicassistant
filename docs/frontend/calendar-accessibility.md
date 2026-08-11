# Acessibilidade do calendário

- O seletor de visualização usa `role="group"` e `aria-pressed`.
- Navegação anterior, próxima e escolha de data têm nomes acessíveis.
- Eventos são botões focáveis com nome contendo paciente, horário e status.
- Estados de carregamento usam rótulo acessível; estado vazio informa como prosseguir.
- A região da agenda usa `aria-live="polite"` para mudanças de período e filtros.
- A grade de semana e mês mantém rolagem horizontal em telas estreitas, sem comprimir conteúdo operacional.

Novos controles devem manter foco visível, rótulo associado e suporte a teclado antes de serem adicionados à agenda.
