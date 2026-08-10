# Intents determinísticos

O `ConversationIntentResolver` normaliza caixa, acentos e espaços antes de aplicar aliases e palavras-chave. Suporta `ViewSpecialties`, `ViewProfessionals`, `CheckAvailability`, as intenções transacionais existentes, `MainMenu`, `GoBack`, `CancelCurrentFlow`, `Repeat`, `Help`, `HumanHandoff` e `Unknown`.

Números são resolvidos contra as opções persistidas da etapa atual; somente no menu principal representam as oito opções do menu.
