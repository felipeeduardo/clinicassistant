# Máquina de estados inicial

```mermaid
stateDiagram-v2
  [*] --> Initial
  Initial --> Menu: saudação
  Menu --> AwaitingSelection: intenção administrativa
  AwaitingSelection --> Menu: menu ou voltar
  Menu --> HandedOff: assunto clínico ou humano
  Menu --> Closed: despedida
  AwaitingSelection --> Menu: expiração ou cancelar fluxo
```

A classificação é determinística e administrativa. Perguntas sobre sintomas, diagnóstico, receita, medicamento ou tratamento sempre solicitam atendimento humano. A máquina retorna chaves de resposta; os textos são compostos separadamente por `IConversationResponseComposer`.
