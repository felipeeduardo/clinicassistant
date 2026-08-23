# Intents determinísticos

O `ConversationIntentResolver` normaliza caixa, acentos e espaços antes de aplicar aliases e palavras-chave. Suporta `ViewSpecialties`, `ViewProfessionals`, `CheckAvailability`, as intenções transacionais existentes, `MainMenu`, `GoBack`, `CancelCurrentFlow`, `Repeat`, `Help`, `HumanHandoff` e `Unknown`.

Números são resolvidos contra as opções persistidas da etapa atual; no menu principal representam as sete opções operacionais. O item `7` encaminha para atendimento humano. O item `8` é aceito apenas como alias silencioso de compatibilidade para mensagens enviadas com o menu antigo, mas nunca é exibido.
