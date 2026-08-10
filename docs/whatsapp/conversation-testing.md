# Testes conversacionais

Os testes unitários cobrem normalização, linguagem natural, menu numérico, números contextuais, comandos globais e limite de entradas inválidas. A validação Fake WhatsApp deve exercitar: saudação, `quais especialidades?`, `quem atende`, `tem horário`, `menu`, `voltar`, `ajuda`, `atendente` e uma entrada desconhecida.

Não executar mensagens reais Twilio automaticamente.

As métricas conversacionais devem ser verificadas no coletor OTLP configurado em `OTEL_EXPORTER_OTLP_ENDPOINT`, sem incluir telefone, conteúdo ou credenciais nas tags.

## Roteiro Fake WhatsApp

Com `WHATSAPP_PROVIDER=Fake`, envie no mesmo contato, aguardando o Worker entre cada mensagem:

1. `quero marcar uma consulta`;
2. escolha uma especialidade, profissional e horário pelo número exibido;
3. responda `sim` para confirmar;
4. inicie uma nova conversa com `vou comparecer` para confirmar uma consulta pendente;
5. use `quero cancelar minha consulta` e confirme;
6. use `quero mudar meu horário`, escolha data/horário e confirme.

Valide na fila Outbox que cada operação gera uma única mutation e que conflitos retornam uma orientação humana, sem stack trace ou código técnico.
