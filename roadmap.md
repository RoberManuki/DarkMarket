# Roadmap CryptoMarket - Marketplace Bitcoin

## Mapeamento de Progresso (Mar/2026)

### âœ… ConcluÃ­do
- Testes automatizados de serviÃ§os crÃ­ticos e integraÃ§Ã£o HTTP (webhook + pÃ¡ginas protegidas + fluxo compra/pedido via webhook, incluindo idempotÃªncia com webhook duplicado, evento nÃ£o-liquidado sem mutaÃ§Ã£o, invoice inexistente sem efeito, payload com campos obrigatÃ³rios vazios/whitespace/tipos invÃ¡lidos, payload acima do limite com `413` com e sem `Content-Length` e com caracteres UTF-8 multibyte, incluindo cenÃ¡rio combinado UTF-8 multibyte + `Content-Length` desconhecido, payload exatamente no limite aceito incluindo fronteira UTF-8 multibyte, payload JSON vÃ¡lido com escapes/unicode dentro do limite, validaÃ§Ã£o estrita de secret (incluindo negaÃ§Ã£o com espaÃ§os extras, header ausente, mÃºltiplos valores, duplicaÃ§Ã£o de valor vÃ¡lido, rejeiÃ§Ã£o por diferenÃ§a de casing no valor e aceitaÃ§Ã£o com casing alternado no nome do header), acesso de comprador/vendedor/admin, acesso negado para intruso, negaÃ§Ã£o quando `UserId` ausente/em branco no detalhe do pedido, negaÃ§Ã£o no `/admin` com role em branco, permissÃ£o com roles mistas contendo `admin`, negaÃ§Ã£o com roles mistas sem `admin` e permissÃ£o com `admin` duplicado, alÃ©m de integraÃ§Ã£o do fluxo de taxa operacional salva e refletida no cÃ¡lculo de repasse) implementados e verdes (`119/119` passando no `CryptoMarket.Tests`).
- Fluxo de confirmaÃ§Ã£o de pagamento unificado (`PaymentConfirmationService`) para telas de pagamento.
- Webhook BTCPay desacoplado e endurecido (`BtcPayWebhookService`): validaÃ§Ã£o de payload, limite de tamanho, comparaÃ§Ã£o de secret em tempo constante e idempotÃªncia de criaÃ§Ã£o/vÃ­nculo de pedido.
- InicializaÃ§Ã£o da aplicaÃ§Ã£o desacoplada (`AppInitializationService`) para seed de roles/admin/gateways.
- Tela de logs administrativos funcional (`/admin/logs`) com filtros e paginaÃ§Ã£o.
- Auditoria de repasse no `/admin/orders-review` com logs de sucesso e tentativas invÃ¡lidas (pedido inexistente/status incompatÃ­vel).
- Perfil do usuÃ¡rio concluÃ­do (ediÃ§Ã£o, resumo e avatar).
- ExibiÃ§Ã£o BTC+fiat concluÃ­da com fallback visual e preferÃªncia de moeda (USD/BRL) controlada no admin.
- Telas `About` e `Contact` implementadas.
- Dashboard com mÃ©tricas reais de usuÃ¡rios, vendas, volume e pendÃªncias.
- Modal de confirmaÃ§Ã£o de repasse implementado com detalhes do pedido (produto, quantidade, comprador, vendedor, valor bruto, taxa e valor lÃ­quido), incluindo evidÃªncias e prÃ©-visualizaÃ§Ã£o da imagem do produto.
- ConfiguraÃ§Ã£o de taxa operacional por admin implementada no painel administrativo com persistÃªncia em banco.
- CÃ¡lculo de repasse centralizado em serviÃ§o (`OperationFeeCalculatorService`) e reutilizado nos modais de repasse (`/admin/orders-review` e `/orders/{id}`) para refletir a taxa configurada.

### ðŸŸ¡ Parcial
- SeguranÃ§a geral e deploy: melhorias aplicadas no webhook e configuraÃ§Ã£o, mas revisÃ£o completa de produÃ§Ã£o ainda pendente.
- Contagem de consultas de cotaÃ§Ã£o BTC contabilizada no backend (`QuoteQuery`) e exibida em dashboard e admin.
- Testes de integraÃ§Ã£o com `WebApplicationFactory` cobrindo `/api/btcpay/webhook` (401 incluindo secret com espaÃ§os extras, secret ausente, secret com mÃºltiplos valores, secret vÃ¡lido duplicado e valor com casing diferente; aceitaÃ§Ã£o de secret vÃ¡lido com casing alternado no nome do header; 400 por JSON invÃ¡lido/campos obrigatÃ³rios vazios/whitespace/tipos invÃ¡lidos; 413 por payload excedido com e sem `Content-Length`, por excedente em bytes UTF-8 multibyte e por cenÃ¡rio combinado UTF-8 multibyte com `Content-Length` desconhecido; payload no limite aceito incluindo fronteira UTF-8 multibyte e payload JSON vÃ¡lido com escapes/unicode dentro do limite; confirmaÃ§Ã£o; idempotÃªncia; evento nÃ£o-liquidado sem efeito e invoice inexistente sem mutaÃ§Ã£o), pÃ¡ginas protegidas (`/orders`, `/orders/{id}`, `/admin`, incluindo request sem `UserId`, com `UserId` em branco, com role em branco, com roles mistas contendo `admin`, com roles mistas sem `admin` e com `admin` duplicado), e fluxo compra/pedido no backend (pagamento confirmado -> pedido criado -> rota de detalhe acessÃ­vel); cobertura E2E completa de UI/fluxos ainda pendente.
- Regra de autorizaÃ§Ã£o de detalhe de pedido centralizada em serviÃ§o (`OrderAccessService`) com testes unitÃ¡rios dedicados para admin/comprador/vendedor/intruso.

### ðŸ”´ Pendente
- Testes de integraÃ§Ã£o de pÃ¡ginas/fluxos end-to-end (alÃ©m dos testes de serviÃ§o atuais).
- RevisÃ£o geral de UX/responsividade final.

## Fase 1: Estrutura Inicial e Layout
- [x] Organizar estrutura de pastas (Pages, Shared, Services, Models, Data)
- [x] Criar layout principal (MainLayout, NavMenu, Header, Footer)
- [x] Definir tema escuro centralizado (CSS base)
- [x] PÃ¡gina de login funcional (com validaÃ§Ã£o e autenticaÃ§Ã£o)
- [x] PÃ¡gina inicial (Dashboard) protegida

---

## Fase 2: UsuÃ¡rio e AutenticaÃ§Ã£o
- [x] Cadastro de usuÃ¡rio (registro)
- [x] Logout e controle de sessÃ£o
- [x] PÃ¡gina de perfil do usuÃ¡rio
- [x] ProteÃ§Ã£o de pÃ¡ginas (acesso sÃ³ logado)
- [x] Controle de roles/permissÃµes (admin, user, etc)

---

## Fase 3: Produtos e Marketplace
- [x] Cadastro de produtos (formulÃ¡rio)
- [x] Listagem de produtos (com filtros e busca)
- [x] PÃ¡gina de detalhes do produto
- [x] EdiÃ§Ã£o e remoÃ§Ã£o de produto (CRUD completo)
- [x] Upload de imagens do produto (opcional)

---

## Fase 4: IntegraÃ§Ã£o com Bitcoin
- [x] ServiÃ§o para cotaÃ§Ã£o do Bitcoin (exibir preÃ§o em BTC e BRL/USD)
- [x] IntegraÃ§Ã£o com gateway de pagamento Bitcoin (BTCPayServer, OpenNode, Blockonomics, etc)
- [x] GeraÃ§Ã£o de QR Code para pagamento
- [x] ConfirmaÃ§Ã£o automÃ¡tica de pagamento

---

## Fase 5: TransaÃ§Ãµes e HistÃ³rico
- [x] Registro de transaÃ§Ãµes (compra/venda)
- [x] PÃ¡gina de histÃ³rico de transaÃ§Ãµes do usuÃ¡rio
- [x] Detalhamento de cada transaÃ§Ã£o

---

## Fase 6: ExperiÃªncia do UsuÃ¡rio
- [x] Sistema de notificaÃ§Ãµes (toast/alertas)
- [x] ValidaÃ§Ã£o de formulÃ¡rios aprimorada
- [x] Feedback visual de carregamento
- [x] Responsividade mobile

---

## Fase 7: FinalizaÃ§Ã£o e Reuso
- [x] DocumentaÃ§Ã£o bÃ¡sica para reuso (README, comentÃ¡rios)
- [x] SeparaÃ§Ã£o de componentes genÃ©ricos (StatCard, Toast, Breadcrumb, etc)
- [x] PreparaÃ§Ã£o para deploy (Dockerfile, publish, etc)
- [x] Checklist de boas prÃ¡ticas (seguranÃ§a, roles, uploads, validaÃ§Ã£o)

---

## Fase 8: Refino, MÃ©tricas e SeguranÃ§a

1. **Testes e SeguranÃ§a**
   - [ ] Implementar testes automatizados (unitÃ¡rios e integraÃ§Ã£o) **(unitÃ¡rios de serviÃ§os concluÃ­dos; integraÃ§Ã£o de UI/fluxos ainda pendente)**
   - [ ] Adicionar logs em todo o sistema (aÃ§Ãµes crÃ­ticas, auditoria) **(base implementada; cobertura total ainda pendente)**
   - [ ] Revisar secrets, seguranÃ§a geral e ajustes para deploy **(parcialmente adiantado)**
   - [ ] RevisÃ£o de permissÃµes e seguranÃ§a em endpoints sensÃ­veis **(parcialmente adiantado)**

2. **AdministraÃ§Ã£o e Auditoria**
   - [x] RevisÃ£o de logs e auditoria para admins

3. **RefatoraÃ§Ã£o e UX**
   - [ ] Componentizar trechos repetidos (DRY), documentando limitaÃ§Ãµes do Blazor e callbacks
   - [ ] RevisÃ£o geral de UX e responsividade
   - [x] Remover coluna ID das listagens

4. **MÃ©tricas e Dashboard**
   - [x] Dashboard com estatÃ­sticas (usuÃ¡rios, vendas, volume, consultas, pendÃªncias)
   - [x] Contabilizar e exibir consultas de cotaÃ§Ã£o BTC (dashboard/admin)

5. **Funcionalidades para UsuÃ¡rio**
   - [x] Implementar tela de perfil do usuÃ¡rio (ediÃ§Ã£o, resumo, avatar)
   - [x] Exibir valores em USD ao lado de BTC (helpers, formataÃ§Ã£o)
   - [x] Implementar telas de About e Contact

6. **Aprimoramento de Fluxos**
   - [x] Modal de confirmaÃ§Ã£o de repasse: exibir detalhes do pedido (nome/foto/valor do produto, quantidade, taxa, valor lÃ­quido, vendedor/comprador)
   - [x] Permitir configuraÃ§Ã£o da taxa de operaÃ§Ã£o pelo admin

---

## Extras/Opcional

- [ ] InternacionalizaÃ§Ã£o (i18n)
- [ ] SEO bÃ¡sico para pÃ¡ginas pÃºblicas
- [ ] IntegraÃ§Ã£o com e-mail (confirmaÃ§Ã£o, notificaÃ§Ãµes)
- [ ] ExportaÃ§Ã£o de logs/admin
- [ ] NotificaÃ§Ãµes em tempo real (SignalR)
