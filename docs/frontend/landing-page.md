# Landing Page comercial

A rota pública `/` apresenta a proposta real do Clinic Assistant sem autenticação e sem dados de pacientes. Inclui hero, demo visual do WhatsApp, problema, diagrama operacional, showcase CSS em abas Dashboard/Agenda/Conversas, Bento Grid, benefícios, segurança, FAQ, CTA de demonstração e link de entrada.

A demo do Hero usa animação CSS controlada de aproximadamente 14 segundos, sem áudio ou backend. A animação é desativada por completo quando `prefers-reduced-motion: reduce` está ativo.

O CTA de demonstração usa e-mail configurável como placeholder até existir backend de leads com validação, rate limiting e anti-spam. Não há promessa de IA, certificação ou indicadores quantitativos.

## SEO e conversão

A Landing define canonical e Open Graph usando `NEXT_PUBLIC_SITE_URL` (com `http://localhost:3000` como fallback local), publica apenas a rota pública em `sitemap.xml` e mantém as áreas autenticadas bloqueadas em `robots.txt`. O CTA ainda usa `mailto` por não existir um endpoint de leads aprovado; analytics não foi adicionado porque não há solução aprovada no projeto.
