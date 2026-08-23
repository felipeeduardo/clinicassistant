# Atendimento humano

`atendente`, `humano`, `recepção`, `falar com alguém` e a opção 7 do menu interrompem imediatamente a automação. O item 8 continua aceito apenas como alias silencioso para compatibilidade com mensagens do menu antigo. O orquestrador muda a conversa para `Human`, cria ou reutiliza o item da fila e publica o evento operacional. Mensagens posteriores não recebem resposta automática enquanto o humano estiver atendendo.
