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

### Pré-requisitos
- .NET 9.0 SDK
- PostgreSQL instalado e rodando

### Configuração do Ambiente

1. **Clone o repositório**
   ```bash
   git clone <url-do-repositorio>
   cd DarkMarket
   ```

2. **Configure o PostgreSQL**
   
   Verifique se o PostgreSQL está rodando:
   ```bash
   sudo systemctl status postgresql
   ```
   
   Crie o usuário e banco de dados:
   ```bash
   # Criar usuário (substitua 'suasenha' pela senha desejada)
   sudo -u postgres psql -c "CREATE USER freeza WITH PASSWORD 'theemperor';"
   
   # Criar banco de dados
   sudo -u postgres psql -c "CREATE DATABASE darkmarket OWNER freeza;"
   
   # Conceder privilégios
   sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE darkmarket TO freeza;"
   ```

3. **Configure o `appsettings.json`**
   
   Ajuste a string de conexão se necessário e configure as chaves dos gateways Bitcoin desejados.

4. **Execute as migrações do banco de dados**
   ```bash
   dotnet ef database update
   ```

5. **Rode o projeto**
   ```bash
   dotnet watch run
   ```

6. **Acesse a aplicação**
   
   Abra o navegador em `http://localhost:5000`

### Troubleshooting

**Erro de autenticação PostgreSQL:**
```
password authentication failed for user "freeza"
```
- Verifique se o usuário foi criado corretamente
- Confirme se a senha no `appsettings.json` está correta
- Certifique-se que o PostgreSQL está rodando

**Erro de conexão com banco:**
```
database "darkmarket" does not exist
```
- Execute os comandos de criação do banco listados acima
- Verifique se o nome do banco no `appsettings.json` está correto

**Para resetar o banco (se necessário):**
```bash
# Remover banco existente
sudo -u postgres psql -c "DROP DATABASE IF EXISTS darkmarket;"

# Recriar banco
sudo -u postgres psql -c "CREATE DATABASE darkmarket OWNER freeza;"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE darkmarket TO freeza;"

# Executar migrações novamente
dotnet ef database update
```

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
