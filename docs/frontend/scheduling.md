# Agenda operacional

A agenda oferece modos Dia, Semana, Mês e Lista na toolbar, navegação Hoje/Anterior/Próximo, timezone visível, criação, confirmação, reagendamento com `expectedVersion` e cancelamento. Dia/Lista consultam o dia selecionado, Semana consulta o intervalo domingo–sábado e Mês consulta o mês completo. A lista é a visualização principal no mobile; Dia/Semana/Mês exibem cartões responsivos e abrem o detalhe em drawer.

As regras de disponibilidade, conflito, concorrência e reagendamento permanecem no backend. O frontend não faz drag-and-drop nem atualizações otimistas: toda alteração usa o endpoint correspondente e reconcilia a agenda após sucesso.
