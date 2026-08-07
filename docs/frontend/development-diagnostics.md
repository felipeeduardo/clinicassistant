# Diagnóstico local

A rota `/settings/development` está disponível somente para administradores da clínica e apresenta informações sanitizadas do ambiente: provider efetivo, ambiente, sender mascarado, validação da integração, health check da API e URLs públicas de webhook/callback.

O diagnóstico não exibe Auth Token, senha, connection string ou payloads. Em produção, a tela fica indisponível por segurança; o provider e as credenciais continuam controlados exclusivamente por configuração do ambiente.
