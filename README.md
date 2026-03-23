# CryptoMarket - Marketplace Bitcoin

## VisÃ£o Geral

CryptoMarket Ã© um marketplace descentralizado focado em transaÃ§Ãµes com Bitcoin, com integraÃ§Ã£o a gateways, painel administrativo, sistema de pedidos, chat e notificaÃ§Ãµes.

---

## Funcionalidades

- Cadastro e autenticaÃ§Ã£o de usuÃ¡rios (roles: admin, user)
- Fluxos de conta Identity: login, registro, recuperar senha, redefinir senha, reenviar confirmaÃ§Ã£o, confirmar e-mail e alterar senha
- Cadastro, ediÃ§Ã£o e listagem de produtos
- IntegraÃ§Ã£o com gateways Bitcoin (BTCPayServer, Testnet, etc)
- GeraÃ§Ã£o de QR Code para pagamentos
- ConfirmaÃ§Ã£o automÃ¡tica de pagamentos
- HistÃ³rico de pedidos e detalhes de transaÃ§Ãµes
- Painel administrativo completo (produtos, usuÃ¡rios, pedidos, logs)
- Sistema de chat por pedido
- NotificaÃ§Ãµes visuais (toast)
- Layout responsivo e tema escuro

---

## Como rodar localmente

### PrÃ©-requisitos
- .NET 9.0 SDK
- PostgreSQL instalado e rodando

### ConfiguraÃ§Ã£o do Ambiente

1. **Clone o repositÃ³rio**
   ```bash
   git clone <url-do-repositorio>
   cd CryptoMarket
   ```

2. **Configure o PostgreSQL**
   
   Verifique se o PostgreSQL estÃ¡ rodando:
   ```bash
   sudo systemctl status postgresql
   ```
   
   Crie o usuÃ¡rio e banco de dados:
   ```bash
   # Criar usuÃ¡rio (substitua 'suasenha' pela senha desejada)
   sudo -u postgres psql -c "CREATE USER freeza WITH PASSWORD 'theemperor';"
   
   # Criar banco de dados
   sudo -u postgres psql -c "CREATE DATABASE cryptomarket OWNER freeza;"
   
   # Conceder privilÃ©gios
   sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE cryptomarket TO freeza;"
   ```

3. **Configure o `appsettings.json`**
   
   Ajuste a string de conexÃ£o se necessÃ¡rio e configure as chaves dos gateways Bitcoin desejados.

   Para habilitar envio real de e-mails do Identity (recuperaÃ§Ã£o de senha e confirmaÃ§Ã£o), configure a seÃ§Ã£o `Email`:

   ```json
   "Email": {
     "Enabled": true,
     "Host": "smtp.seuprovedor.com",
     "Port": 587,
     "UseSsl": true,
     "Username": "__SET_VIA_USER_SECRETS__",
     "Password": "__SET_VIA_USER_SECRETS__",
     "FromEmail": "no-reply@seusite.com",
     "FromName": "CryptoMarket"
   }
   ```

   Em ambiente local, se `Enabled=false` ou sem credenciais, o sistema usa fallback em log sem quebrar os fluxos.
   Nesse modo de fallback, os e-mails tambem sao salvos em arquivos `.html` e `.txt` em `wwwroot/uploads/dev-emails` para facilitar testes locais dos links.

   PadrÃµes de seguranÃ§a de autenticaÃ§Ã£o atualmente configurados:
   - Lockout apÃ³s 5 tentativas invÃ¡lidas.
   - DuraÃ§Ã£o do lockout: 15 minutos.
    - Cookie de autenticaÃ§Ã£o com renovaÃ§Ã£o por atividade (sliding expiration).
    - ExpiraÃ§Ã£o de sessÃ£o: 60 minutos em Development e 30 minutos em Production/Staging.
    - Cookie `Secure` exige HTTPS em Production/Staging.
   - Em Production/Staging, login exige e-mail confirmado; em Development o fluxo permanece flexÃ­vel para testes locais.
    - PolÃ­tica de senha:
       - Development: mÃ­nimo 6 caracteres, com dÃ­gito e minÃºscula.
       - Production/Staging: mÃ­nimo 10 caracteres, exigindo maiÃºscula, minÃºscula, dÃ­gito, sÃ­mbolo e 3 caracteres Ãºnicos.

4. **Execute as migraÃ§Ãµes do banco de dados**
   ```bash
   dotnet ef database update
   ```

5. **Rode o projeto**
   ```bash
   dotnet watch run
   ```

6. **Acesse a aplicaÃ§Ã£o**
   
   Abra o navegador em `http://localhost:5000`

### Troubleshooting

**Erro de autenticaÃ§Ã£o PostgreSQL:**
```
password authentication failed for user "freeza"
```
- Verifique se o usuÃ¡rio foi criado corretamente
- Confirme se a senha no `appsettings.json` estÃ¡ correta
- Certifique-se que o PostgreSQL estÃ¡ rodando

**Erro de conexÃ£o com banco:**
```
database "cryptomarket" does not exist
```
- Execute os comandos de criaÃ§Ã£o do banco listados acima
- Verifique se o nome do banco no `appsettings.json` estÃ¡ correto

**Para resetar o banco (se necessÃ¡rio):**
```bash
# Remover banco existente
sudo -u postgres psql -c "DROP DATABASE IF EXISTS cryptomarket;"

# Recriar banco
sudo -u postgres psql -c "CREATE DATABASE cryptomarket OWNER freeza;"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE cryptomarket TO freeza;"

# Executar migraÃ§Ãµes novamente
dotnet ef database update
```

### Nota importante sobre paginaÃ§Ã£o e componentizaÃ§Ã£o

DecisÃ£o atual do projeto:
- As paginaÃ§Ãµes foram mantidas em cÃ³digo local das pÃ¡ginas (sem componente compartilhado de paginaÃ§Ã£o) por estabilidade operacional.

EvidÃªncias observadas no projeto:
- Na tela `"/admin/logs"`, a versÃ£o componentizada da paginaÃ§Ã£o apresentou cliques sem avanÃ§o de pÃ¡gina em ambiente real.
- Ao substituir por botÃµes locais na prÃ³pria pÃ¡gina, o comportamento voltou ao normal imediatamente.
- O comportamento foi percebido em histÃ³rico anterior do projeto e reproduzido novamente no ciclo atual.

Registro cronolÃ³gico recente (refatoraÃ§Ã£o/auditoria):
- CenÃ¡rio inicial estÃ¡vel: .NET 9 + paginaÃ§Ã£o local na tela de logs.
- Tentativa de padronizaÃ§Ã£o com componente de paginaÃ§Ã£o: regressÃ£o de clique sem avanÃ§o em logs.
- MitigaÃ§Ã£o imediata: descomponentizaÃ§Ã£o da paginaÃ§Ã£o em logs e posteriormente em todas as rotas paginadas.
- Tentativa de atualizaÃ§Ã£o para .NET 10 para eliminar hipÃ³tese de bug de versÃ£o: compilou, mas o comportamento reportado em ambiente real nÃ£o estabilizou.
- DecisÃ£o operacional: rollback completo para .NET 9 (`global.json`, `TargetFramework` da aplicaÃ§Ã£o e testes), preservando paginaÃ§Ã£o local.
- Estado validado apÃ³s rollback: comportamento voltou a funcionar no fluxo reportado.

Risco conhecido e liÃ§Ã£o aprendida:
- Risco: regressÃ£o silenciosa de interatividade em paginaÃ§Ãµes durante recomposiÃ§Ã£o/upgrade.
- LiÃ§Ã£o: preferir estabilidade observÃ¡vel em runtime real antes de consolidar abstraÃ§Ãµes compartilhadas.
- Regra prÃ¡tica: abstraÃ§Ã£o sÃ³ permanece quando o comportamento final for equivalente em todas as rotas crÃ­ticas.

Escopo atual da decisÃ£o:
- PÃ¡ginas administrativas e de usuÃ¡rio com paginaÃ§Ã£o usam implementaÃ§Ã£o local.
- O componente `Shared/Components/PaginationControls.razor` nÃ£o Ã© obrigatÃ³rio para os fluxos atuais.
- AÃ§Ãµes crÃ­ticas de filtros (`Filtrar`/`Limpar`) em pÃ¡ginas admin tambÃ©m usam botÃµes locais (sem componente intermediÃ¡rio de aÃ§Ã£o) para reduzir risco de regressÃ£o de callback.

Diretriz para futura recomposiÃ§Ã£o:
- SÃ³ reintroduzir paginaÃ§Ã£o componentizada com teste manual obrigatÃ³rio nas rotas: `/admin/logs`, `/admin/users`, `/admin/products`, `/admin/payments`, `/admin/orders`, `/admin/orders-review`, `/orders`, `/payments`, `/products`, `/marketplace`.
- Registrar evidÃªncia do teste (data, versÃ£o .NET, navegador e resultado por rota) antes de consolidar a recomposiÃ§Ã£o.
- Em caso de regressÃ£o em qualquer rota, voltar para paginaÃ§Ã£o local nessa rota.
- RecomendaÃ§Ã£o para PR de recomposiÃ§Ã£o: incluir checklist de validaÃ§Ã£o de clique, persistÃªncia de pÃ¡gina atual e comportamento apÃ³s filtro/ordenaÃ§Ã£o.

ReferÃªncias para debate na comunidade:
- Issue tracker ASP.NET Core: https://github.com/dotnet/aspnetcore/issues
- DiscussÃµes ASP.NET Core: https://github.com/dotnet/aspnetcore/discussions
- Docs de EventCallback: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling
- Docs de render modes/interatividade: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes

---

## Testes E2E (Playwright)

AlÃ©m dos testes automatizados .NET (`CryptoMarket.Tests`), o projeto possui uma suÃ­te de testes E2E de navegador na pasta [e2e](e2e) para validar interaÃ§Ãµes reais de UI/JavaScript.

### CenÃ¡rios cobertos atualmente

- Banner de consentimento de cookies (aceitar/personalizar/persistÃªncia)
- Troca de idioma via flags e persistÃªncia em rotas protegidas

### PrÃ©-requisitos

- Node.js 20+
- npm
- AplicaÃ§Ã£o rodando localmente (ex.: `http://127.0.0.1:5000`)

### Como executar

```bash
cd e2e
npm install
npm run install:browsers
npm test
```

Para apontar para outra URL:

```bash
set E2E_BASE_URL=http://127.0.0.1:5001
npm test
```

---

## Estrutura de Pastas

- `Pages/` - PÃ¡ginas principais (Marketplace, Admin, Pedidos, Pagamentos)
- `Shared/Components/` - Componentes reutilizÃ¡veis (Header, Footer, Toast, etc)
- `Models/` - Modelos de dados
- `Services/` - ServiÃ§os de integraÃ§Ã£o e lÃ³gica de negÃ³cio
- `Data/` - Contexto do banco de dados

---

## Roadmap e Progresso

Veja o arquivo [roadmap.md](roadmap.md) para detalhes das fases e prÃ³ximos passos.

**Status rÃ¡pido (Mar/2026):**
- âœ… Testes de serviÃ§os crÃ­ticos concluÃ­dos e estÃ¡veis (`50/50` passando).
- âœ… Fluxo de pagamento/webhook robustecido e desacoplado em serviÃ§os dedicados.
- âœ… Auditoria administrativa disponÃ­vel em `/admin/logs`.
- ðŸŸ¡ PrÃ³xima frente: testes de integraÃ§Ã£o de pÃ¡ginas/fluxos e fechamento dos itens de UX/finalizaÃ§Ã£o.

**Principais tarefas em andamento:**
- Testes de integraÃ§Ã£o (UI/fluxos) para complementar cobertura unitÃ¡ria atual
- MÃ©tricas de uso e dashboard com indicadores de negÃ³cio persistidos
- Refino de UX/componentizaÃ§Ã£o e limpeza visual final
- SeguranÃ§a e preparaÃ§Ã£o para produÃ§Ã£o (hardening final + revisÃ£o de permissÃµes)

---

## ContribuiÃ§Ã£o

Pull requests sÃ£o bem-vindos! Veja o roadmap e abra issues para sugestÃµes ou bugs.

---

## LicenÃ§a

MIT

