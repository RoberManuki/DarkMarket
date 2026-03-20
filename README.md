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

   Padrões de segurança de autenticação atualmente configurados:
   - Lockout após 5 tentativas inválidas.
   - Duração do lockout: 15 minutos.
    - Cookie de autenticação com renovação por atividade (sliding expiration).
    - Expiração de sessão: 60 minutos em Development e 30 minutos em Production/Staging.
    - Cookie `Secure` exige HTTPS em Production/Staging.
   - Em Production/Staging, login exige e-mail confirmado; em Development o fluxo permanece flexível para testes locais.
    - Política de senha:
       - Development: mínimo 6 caracteres, com dígito e minúscula.
       - Production/Staging: mínimo 10 caracteres, exigindo maiúscula, minúscula, dígito, símbolo e 3 caracteres únicos.

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

### Nota importante sobre paginação e componentização

Decisão atual do projeto:
- As paginações foram mantidas em código local das páginas (sem componente compartilhado de paginação) por estabilidade operacional.

Evidências observadas no projeto:
- Na tela `"/admin/logs"`, a versão componentizada da paginação apresentou cliques sem avanço de página em ambiente real.
- Ao substituir por botões locais na própria página, o comportamento voltou ao normal imediatamente.
- O comportamento foi percebido em histórico anterior do projeto e reproduzido novamente no ciclo atual.

Registro cronológico recente (refatoração/auditoria):
- Cenário inicial estável: .NET 9 + paginação local na tela de logs.
- Tentativa de padronização com componente de paginação: regressão de clique sem avanço em logs.
- Mitigação imediata: descomponentização da paginação em logs e posteriormente em todas as rotas paginadas.
- Tentativa de atualização para .NET 10 para eliminar hipótese de bug de versão: compilou, mas o comportamento reportado em ambiente real não estabilizou.
- Decisão operacional: rollback completo para .NET 9 (`global.json`, `TargetFramework` da aplicação e testes), preservando paginação local.
- Estado validado após rollback: comportamento voltou a funcionar no fluxo reportado.

Risco conhecido e lição aprendida:
- Risco: regressão silenciosa de interatividade em paginações durante recomposição/upgrade.
- Lição: preferir estabilidade observável em runtime real antes de consolidar abstrações compartilhadas.
- Regra prática: abstração só permanece quando o comportamento final for equivalente em todas as rotas críticas.

Escopo atual da decisão:
- Páginas administrativas e de usuário com paginação usam implementação local.
- O componente `Shared/Components/PaginationControls.razor` não é obrigatório para os fluxos atuais.
- Ações críticas de filtros (`Filtrar`/`Limpar`) em páginas admin também usam botões locais (sem componente intermediário de ação) para reduzir risco de regressão de callback.

Diretriz para futura recomposição:
- Só reintroduzir paginação componentizada com teste manual obrigatório nas rotas: `/admin/logs`, `/admin/users`, `/admin/products`, `/admin/payments`, `/admin/orders`, `/admin/orders-review`, `/orders`, `/payments`, `/products`, `/marketplace`.
- Registrar evidência do teste (data, versão .NET, navegador e resultado por rota) antes de consolidar a recomposição.
- Em caso de regressão em qualquer rota, voltar para paginação local nessa rota.
- Recomendação para PR de recomposição: incluir checklist de validação de clique, persistência de página atual e comportamento após filtro/ordenação.

Referências para debate na comunidade:
- Issue tracker ASP.NET Core: https://github.com/dotnet/aspnetcore/issues
- Discussões ASP.NET Core: https://github.com/dotnet/aspnetcore/discussions
- Docs de EventCallback: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling
- Docs de render modes/interatividade: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes

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
