# OpenAPI e Swagger

A especificação é produzida em tempo de execução pela API ASP.NET Core, que é a fonte de verdade para os contratos HTTP.

- Swagger UI: `http://localhost:8080/swagger`
- Documento OpenAPI v1: `http://localhost:8080/swagger/v1/swagger.json`

Não há um arquivo OpenAPI estático versionado neste momento. A decisão evita manter um contrato manual que possa divergir dos endpoints Minimal API. A coleção Postman contém os fluxos operacionais mais usados; Swagger contém a listagem integral e atualizada dos endpoints expostos pela instância em execução.

Em uma futura governança que exija artefato versionado, a geração deve ser automatizada a partir de uma instância da API no CI, com validação de diff, e jamais editada manualmente.
