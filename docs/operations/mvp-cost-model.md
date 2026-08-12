# Modelo de custos do MVP

Status: `EXTERNAL VALIDATION REQUIRED`

| Componente | Fixo | Variável | Unidade de medição | Fonte a validar |
|---|---|---|---|---|
| Twilio/WhatsApp | — | Mensagens/conversas | Por mensagem ou janela | Tabela oficial do provedor |
| Frontend | Hospedagem | Tráfego/build | Ambiente e tráfego | Provedor escolhido |
| Backend/API | Instância | CPU/egress | Serviço e volume | Provedor escolhido |
| PostgreSQL | Instância | Storage/IO | Banco e crescimento | Provedor escolhido |
| Redis | Instância | Memória/egress | Cache e filas | Provedor escolhido |
| Storage | — | GB e requisições | Mídia/documentos | Provedor escolhido |
| E-mail | — | Mensagens | Lead e notificações | Provedor escolhido |
| Observabilidade | Plano | Eventos/logs | Retenção e volume | Ferramenta escolhida |
| Domínio/TLS | Registro | Renovação | Domínio/ano | Registrador |
| IA futura | — | Tokens/requisições | Modelo e volume | Provedor de IA |

Fórmula de referência:

`MRR por clínica - custo variável - alocação de infraestrutura = margem de contribuição estimada`

Não publicar valores até preencher as fontes e aprovar premissas.
