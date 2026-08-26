# Testes de e-mail

Em testes, use `Email__Provider=Fake` (ou `Email__Enabled=false`). O
`FakeEmailSender` registra destinatário, assunto e horário em memória; nunca há
chamada ao SendGrid. O cenário de integração é: solicitar recuperação, capturar
o envio fake, redefinir com o token, confirmar login e verificar que uma segunda
tentativa falha.

Para smoke produtivo, use apenas uma conta de QA e siga o
[runbook](../operations/email-runbook.md). Nunca coloque API keys, tokens ou
senhas em collections, snapshots ou documentação.
