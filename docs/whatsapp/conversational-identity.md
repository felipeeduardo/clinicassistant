# Identidade conversacional por clínica

Cada clínica pode definir o campo `AssistantDisplayName` na tela **Clínica → Editar clínica**. O nome aparece apenas na saudação inicial; o menu e sua numeração permanecem padronizados.

## Comportamento

- Primeiro contato: apresenta o nome configurado, informa que é uma assistente virtual e exibe o menu de 1 a 7.
- Retorno ao menu: usa uma mensagem curta (`Claro. Como posso ajudar agora?`) sem repetir a apresentação.
- Nome vazio ou inexistente: utiliza `IA Recepção`.
- Nome da clínica: usa o nome fantasia; se indisponível, usa `a clínica`.
- O nome da assistente é da automação e não substitui o nome do atendente humano.

O campo aceita até 60 caracteres, remove espaços nas extremidades e rejeita caracteres de markup (`<` e `>`). A migration `202608230001_AssistantDisplayName` cria o campo com fallback seguro para instalações existentes.

## Exemplo

```text
Olá! 👋

Eu sou a Ana, assistente virtual da Clínica Minimal.
Estou por aqui para ajudar com sua consulta.

Como posso ajudar?

1 - Ver especialidades
2 - Ver profissionais
3 - Consultar disponibilidade
4 - Agendar consulta
5 - Reagendar consulta
6 - Cancelar consulta
7 - Falar com atendente
```

O fluxo, a máquina de estados, as ações numéricas, os provedores e o handoff humano não são alterados por essa configuração.
