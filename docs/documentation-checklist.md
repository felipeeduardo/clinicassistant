# Checklist de atualização documental

Use este checklist em toda alteração de feature, endpoint, migration ou fluxo operacional.

- [ ] OpenAPI atualizado ou comportamento de geração confirmado.
- [ ] Collection e environments Postman atualizados.
- [ ] Documentação da feature atualizada na fonte canônica.
- [ ] Fluxo E2E manual e automatizado revisado.
- [ ] `.env.example` revisado, sem segredos.
- [ ] Policies e permissões documentadas.
- [ ] Migration e procedimento de atualização documentados.
- [ ] Erros esperados e ações de recuperação documentados.
- [ ] Exemplos sanitizados: sem senhas, tokens, dados reais ou credenciais de provedor.
- [ ] Links Markdown e JSON Postman validados.

O [template de Pull Request](../.github/pull_request_template.md) referencia este checklist. Alterações de API também exigem conferência de drift entre OpenAPI e Postman.

## Validação local

```bash
bash scripts/docs/validate-docs.sh
bash scripts/postman/validate-collection.sh
bash scripts/postman/check-openapi-drift.sh http://127.0.0.1:8080/swagger/v1/swagger.json
```

O último comando requer a API em execução. Ele confirma que cada rota HTTP da collection, exceto health e Swagger, está presente no OpenAPI gerado pela instância.
