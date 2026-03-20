# DarkMarket - Marketplace Bitcoin

## Visão Geral

DarkMarket é um marketplace descentralizado focado em transações com Bitcoin, com integração a gateways, painel administrativo, sistema de pedidos, chat e notificações.

---

## Funcionalidades

- Cadastro e autenticação de usuários (roles: admin, user)
- Fluxos de conta Identity: login, registro, recuperar senha, redefinir senha, reenviar confirmação, confirmar e-mail e alterar senha
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

   Para habilitar envio real de e-mails do Identity (recuperação de senha e confirmação), configure a seção `Email`:

   ```json
   "Email": {
     "Enabled": true,
     "Host": "smtp.seuprovedor.com",
     "Port": 587,
     "UseSsl": true,
     "Username": "__SET_VIA_USER_SECRETS__",
     "Password": "__SET_VIA_USER_SECRETS__",
     "FromEmail": "no-reply@seusite.com",
     "FromName": "DarkMarket"
   }
   ```

   Em ambiente local, se `Enabled=false` ou sem credenciais, o sistema usa fallback em log sem quebrar os fluxos.
   Nesse modo de fallback, os e-mails tambem sao salvos em arquivos `.html` e `.txt` em `wwwroot/uploads/dev-emails` para facilitar testes locais dos links.

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

**Status rápido (Mar/2026):**
- ✅ Testes de serviços críticos concluídos e estáveis (`50/50` passando).
- ✅ Fluxo de pagamento/webhook robustecido e desacoplado em serviços dedicados.
- ✅ Auditoria administrativa disponível em `/admin/logs`.
- 🟡 Próxima frente: testes de integração de páginas/fluxos e fechamento dos itens de UX/finalização.

**Principais tarefas em andamento:**
- Testes de integração (UI/fluxos) para complementar cobertura unitária atual
- Métricas de uso e dashboard com indicadores de negócio persistidos
- Refino de UX/componentização e limpeza visual final
- Segurança e preparação para produção (hardening final + revisão de permissões)

---

## Contribuição

Pull requests são bem-vindos! Veja o roadmap e abra issues para sugestões ou bugs.

---

## Licença

MIT
