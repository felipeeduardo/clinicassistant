# Wiki interna do PlatformAdmin

A Wiki é uma central curada para o PlatformAdmin consultar documentação técnica, de negócio, implantação e governança. Ela não edita Markdown nem expõe um navegador indiscriminado do repositório: os documentos continuam versionados no Git e aparecem no registry revisado do frontend.

## Categorias

- **Técnica:** arquitetura, multi-tenant, WhatsApp, deploy e troubleshooting.
- **Negócio:** visão do produto, pricing e fluxo comercial.
- **Implantação:** onboarding e checklist de go-live.
- **Governança:** segurança, autorização e operação.

Cada entrada informa título, resumo, tags, data de atualização, caminho de origem e status (`Atual`, `Revisar` ou `Obsoleto`). A busca cobre título, descrição, tags e conteúdo curado.

## Segurança

A rota exige o papel `PlatformAdmin`. ClinicAdmin, Receptionist e Professional não recebem links nem autorização para a Wiki. Segredos, credenciais e connection strings devem permanecer fora dos documentos.

## Atualização

1. Revise o documento fonte.
2. Remova referências obsoletas e segredos.
3. Atualize a entrada correspondente no registry.
4. Rode lint, testes e build do frontend.
5. Publique a alteração junto com a revisão de código.

O inventário e os critérios de revisão estão em [wiki-inventory.md](./wiki-inventory.md).
