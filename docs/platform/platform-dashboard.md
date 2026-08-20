# Dashboard da plataforma

O Dashboard da plataforma é exclusivo do perfil `PlatformAdmin` e apresenta uma visão agregada do negócio e da infraestrutura. Ele não substitui as telas operacionais de uma clínica e não expõe pacientes, consultas, conversas ou credenciais.

## Indicadores

- clínicas ativas, em configuração, suspensas e desativadas;
- novas clínicas criadas no período selecionado;
- leads recebidos e aguardando primeiro contato;
- funil comercial (novo, contactado, qualificado, demonstração e ganho);
- progresso de setup das clínicas em configuração;
- saúde observável da API, PostgreSQL, RabbitMQ e Redis.

O período visual pode ser de 7, 30 ou 90 dias, com 30 dias como padrão. Totais de clínicas são absolutos; somente métricas temporais respeitam o filtro. Métricas financeiras, MRR e conversão não são estimadas enquanto não houver eventos e contratos próprios para isso.

## Acesso e estados

O endpoint exige a policy `PlatformAdmin`. A interface possui carregamento, erro com retry, estado vazio, atualização manual, foco acessível e layout responsivo. A lista de leads mostra apenas contagens e links para a operação comercial, sem e-mail ou telefone.

