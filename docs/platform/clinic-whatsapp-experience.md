# Experiência WhatsApp do ClinicAdmin

O ClinicAdmin vê somente conceitos do produto: status do WhatsApp, canal/número mascarado, última atividade, último envio, última falha e templates.

Os estados apresentados são **Conectado**, **Em configuração**, **Requer atenção**, **Indisponível** e **Desativado**. Provider, Fake, Twilio, Account SID, Auth Token, webhooks internos e stack traces não fazem parte da interface.

O ClinicAdmin pode verificar a conexão, habilitar/desabilitar o uso do canal na clínica e enviar teste para o destinatário permitido pelo ambiente. A configuração global permanece responsabilidade de Platform/Ops.

