# Roadmap DarkMarket - Marketplace Bitcoin

## Fase 1: Estrutura Inicial e Layout
- [x] Organizar estrutura de pastas (Pages, Shared, Services, Models, Data)
- [x] Criar layout principal (MainLayout, NavMenu, Header, Footer)
- [x] Definir tema escuro centralizado (CSS base)
- [x] Página de login funcional (com validação e autenticação)
- [x] Página inicial (Dashboard) protegida

---

## Fase 2: Usuário e Autenticação
- [x] Cadastro de usuário (registro)
- [x] Logout e controle de sessão
- [x] Página de perfil do usuário
- [x] Proteção de páginas (acesso só logado)
- [x] Controle de roles/permissões (admin, user, etc)

---

## Fase 3: Produtos e Marketplace
- [x] Cadastro de produtos (formulário)
- [x] Listagem de produtos (com filtros e busca)
- [x] Página de detalhes do produto
- [x] Edição e remoção de produto (CRUD completo)
- [x] Upload de imagens do produto (opcional)

---

## Fase 4: Integração com Bitcoin
- [x] Serviço para cotação do Bitcoin (exibir preço em BTC e BRL/USD)
- [x] Integração com gateway de pagamento Bitcoin (BTCPayServer, OpenNode, Blockonomics, etc)
- [x] Geração de QR Code para pagamento
- [x] Confirmação automática de pagamento

---

## Fase 5: Transações e Histórico
- [x] Registro de transações (compra/venda)
- [x] Página de histórico de transações do usuário
- [x] Detalhamento de cada transação

---

## Fase 6: Experiência do Usuário
- [x] Sistema de notificações (toast/alertas)
- [x] Validação de formulários aprimorada
- [x] Feedback visual de carregamento
- [x] Responsividade mobile

---

## Fase 7: Finalização e Reuso
- [x] Documentação básica para reuso (README, comentários)
- [x] Separação de componentes genéricos (StatCard, Toast, Breadcrumb, etc)
- [x] Preparação para deploy (Dockerfile, publish, etc)
- [x] Checklist de boas práticas (segurança, roles, uploads, validação)

---

## Fase 8: Refino, Métricas e Segurança

1. **Testes e Segurança**
   - [ ] Implementar testes automatizados (unitários e integração) **[priorizar antes de grandes refatorações/componentização]**
   - [ ] Adicionar logs em todo o sistema (ações críticas, auditoria)
   - [ ] Revisar secrets, segurança geral e ajustes para deploy
   - [ ] Revisão de permissões e segurança em endpoints sensíveis

2. **Administração e Auditoria**
   - [ ] Revisão de logs e auditoria para admins

3. **Refatoração e UX**
   - [ ] Componentizar trechos repetidos (DRY), documentando limitações do Blazor e callbacks
   - [ ] Revisão geral de UX e responsividade
   - [ ] Remover coluna ID das listagens

4. **Métricas e Dashboard**
   - [ ] Dashboard com estatísticas (usuários, vendas, volume, consultas, pendências)
   - [ ] Contabilizar e exibir consultas de cotação BTC (dashboard/admin)

5. **Funcionalidades para Usuário**
   - [ ] Implementar tela de perfil do usuário (edição, resumo, avatar)
   - [ ] Exibir valores em USD ao lado de BTC (helpers, formatação)
   - [ ] Implementar telas de About e Contact

6. **Aprimoramento de Fluxos**
   - [ ] Modal de confirmação de repasse: exibir detalhes do pedido (nome/foto/valor do produto, quantidade, taxa, valor líquido, vendedor/comprador)
   - [ ] Permitir configuração da taxa de operação pelo admin

---

## Extras/Opcional

- [ ] Internacionalização (i18n)
- [ ] SEO básico para páginas públicas
- [ ] Integração com e-mail (confirmação, notificações)
- [ ] Exportação de logs/admin
- [ ] Notificações em tempo real (SignalR)