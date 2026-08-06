# Performance operacional do frontend

## Decisões aplicadas

- Rotas do App Router são compiladas em chunks por página pelo Next.js; não há dependência de uma tela operacional pesada no carregamento inicial de outra.
- React Query mantém `staleTime` de 30 segundos para reduzir refetches de navegação e o cache é limpo ao encerrar sessão.
- Pacientes usam paginação de 20 itens e preservam a página anterior durante uma troca de página ou filtro.
- Filtros textuais de pacientes e auditoria usam debounce de 300 ms, evitando uma chamada HTTP por tecla digitada.
- Conversas só pesquisam ao enviar o formulário; mensagens são limitadas a 100 por requisição e pacientes a 100 no máximo pelo backend.
- Eventos SignalR são deduplicados por `eventId` e invalidam apenas as query keys afetadas. As queries HTTP autenticadas continuam como fonte de verdade.

## Limites atuais

Virtualização não é necessária no estado atual porque as telas operacionais já possuem paginação, limites de API ou uso em intervalos curtos. Reavaliar virtualização para listas de conversas ou auditoria caso a UI passe a exibir centenas de linhas simultâneas.

O dashboard é invalidado por eventos operacionais relevantes; não recebe polling contínuo. O StatusCallback e o Worker atualizam os dados de forma assíncrona, portanto o indicador final de entrega deve ser consultado no estado da mensagem.

## Verificação de release

- [ ] Avaliar Web Vitals no ambiente implantado, em desktop e dispositivo móvel.
- [ ] Verificar que buscas rápidas não geram uma requisição por tecla.
- [ ] Confirmar que troca de página não apaga a tabela enquanto a página seguinte carrega.
- [ ] Validar reconexão SignalR e ausência de duplicação de eventos.
- [ ] Revisar bundle após adicionar dependências visuais grandes.
