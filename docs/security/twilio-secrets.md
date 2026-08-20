# Secrets Twilio

`Twilio__AccountSid`, `Twilio__AuthToken` e demais credenciais pertencem exclusivamente ao backend e ao Worker. Devem ser cadastrados no secret store do Railway, nunca no frontend/Vercel, banco, Postman ou repositório.

Logs podem registrar o resultado operacional, mas não tokens, headers de autorização ou URLs internas. A rotação deve substituir o secret no Railway e reiniciar os serviços; o ClinicAdmin não participa desse processo.

