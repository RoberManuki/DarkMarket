# DarkMarket - Marketplace Bitcoin

## Visão Geral

DarkMarket é um marketplace descentralizado focado em transações com Bitcoin, com integração a gateways, painel administrativo, sistema de pedidos, chat e notificações.

---

## Funcionalidades

- Cadastro e autenticação de usuários (roles: admin, user)
- Cadastro, edição e listagem de produtos
- Integração com gateways Bitcoin (BTCPayServer, Testnet, etc)
- Geração de QR Code para pagamentos
- Confirmação automática de pagamentos
- Histórico de pedidos e detalhes de transações
- Painel administrativo completo (produtos, usuários, pedidos, logs)
- Sistema de chat por pedido
- Notificações visuais (toast)
- Layout responsivo e tema escuro

---

## Como rodar localmente

1. Clone o repositório
2. Configure o `appsettings.json` com as chaves dos gateways desejados
3. Execute as migrações do banco de dados
4. Rode o projeto:
   ```bash
   dotnet run
   ```
5. Acesse `http://localhost:5000`

---

## Estrutura de Pastas

- `Pages/` - Páginas principais (Marketplace, Admin, Pedidos, Pagamentos)
- `Shared/Components/` - Componentes reutilizáveis (Header, Footer, Toast, etc)
- `Models/` - Modelos de dados
- `Services/` - Serviços de integração e lógica de negócio
- `Data/` - Contexto do banco de dados

---

## Roadmap e Progresso

Veja o arquivo [roadmap.md](roadmap.md) para detalhes das fases e próximos passos.

**Principais tarefas em andamento:**
- Auditoria e logging centralizado
- Métricas de uso e dashboard
- Refino de UX e componentização
- Testes automatizados para garantir estabilidade
- Segurança e preparação para produção

---

## Contribuição

Pull requests são bem-vindos! Veja o roadmap e abra issues para sugestões ou bugs.

---

## Licença

MIT
