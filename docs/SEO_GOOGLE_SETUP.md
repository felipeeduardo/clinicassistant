# Configuração de SEO e Google

## Publicação

1. Configure `NEXT_PUBLIC_SITE_URL` com a URL pública do frontend (sem barra final).
2. Publique o frontend e confirme `https://seu-dominio/sitemap.xml` e `/robots.txt`.
3. No Google Search Console, adicione uma propriedade de domínio ou prefixo de URL.
4. Valide o domínio pelo DNS do Registro.br (TXT fornecido pelo Search Console).
5. Envie o sitemap completo: `https://seu-dominio/sitemap.xml`.
6. Solicite a indexação da página inicial e das páginas de intenção comercial.

## UTM e leads

O formulário `/demonstracao` preserva `utm_source`, `utm_medium`, `utm_campaign`, `utm_content`, `utm_term`, a landing page e o referrer. Os campos são opcionais e compatíveis com leads antigos. No painel PlatformAdmin, filtre por origem e consulte a atribuição no detalhe do lead.

## Cuidados

- Rotas privadas ficam bloqueadas em `robots.ts` e não entram no sitemap.
- Não inclua credenciais, dados de pacientes ou tokens em URLs.
- Após cada campanha, verifique no painel se os leads receberam origem e campanha.
