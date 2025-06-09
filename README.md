# DarkMarket

Marketplace de Bitcoin desenvolvido em **Blazor Server** com autenticação robusta via **ASP.NET Core Identity**.

---

## 🚀 Visão Geral

O DarkMarket é um marketplace focado em transações com Bitcoin, com arquitetura moderna, tema escuro e autenticação segura.  
O projeto está sendo desenvolvido em etapas, com foco em boas práticas, extensibilidade e experiência do usuário.

---

## 🛠️ Tecnologias e Ferramentas

- **.NET 8+**
- **Blazor Server** (SPA com C#)
- **ASP.NET Core Identity** (autenticação, registro, logout, roles)
- **Entity Framework Core** (PostgreSQL)
- **Razor Pages** (para telas do Identity)
- **CSS customizado** (tema escuro centralizado)
- **Estrutura modular**: Pages, Shared, Services, Models, Data

---

## ⚙️ Configuração e Publicação

### 1. Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/)
- (Opcional) [Docker](https://www.docker.com/) para publicação

### 2. Configuração do Banco de Dados

1. Crie um banco PostgreSQL e configure o usuário/senha.
2. No arquivo `appsettings.json` ou `appsettings.Production.json`, configure a connection string:

    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Port=5432;Database=darkmarket;Username=usuario;Password=senha"
    }
    ```

3. Rode as migrations para criar as tabelas:

    ```bash
    dotnet ef database update
    ```

### 3. Configuração de Gateways e Secrets

No `appsettings.json` ou via variáveis de ambiente, configure:

```json
"BtcPay": {
  "ApiKey": "SUA_API_KEY",
  "StoreId": "SEU_STORE_ID",
  "Url": "https://mainnet.demo.btcpayserver.org"
}
```

- Para Testnet, crie um gateway com o nome **Testnet** no admin.

### 4. Rodando Localmente

```bash
dotnet restore
dotnet build
dotnet run
```
Acesse em [http://localhost:5000](http://localhost:5000)

### 5. Publicação (Deploy)

#### Docker (exemplo básico)

Crie um arquivo `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out
WORKDIR /app/out
ENTRYPOINT ["dotnet", "DarkMarket.dll"]
```

Build e execute:

```bash
docker build -t darkmarket .
docker run -e ASPNETCORE_ENVIRONMENT=Production -p 5000:80 darkmarket
```

#### Publicação manual

```bash
dotnet publish -c Release -o ./publish
# Copie o conteúdo da pasta ./publish para seu servidor
```

### 6. Variáveis de Ambiente

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=...`
- `BtcPay__ApiKey=...`
- `BtcPay__StoreId=...`
- `BtcPay__Url=...`

---

## 📁 Estrutura do Projeto

- `/Pages` — Páginas Blazor principais (Marketplace, Dashboard, Admin, etc)
- `/Shared` — Layouts, NavMenu, Breadcrumb, componentes reutilizáveis
- `/Areas/Identity/Pages` — Telas de autenticação (login, registro, logout, layout customizado)
- `/wwwroot/css/site.css` — CSS principal do app
- `/Data` — Contexto do Entity Framework e migrations
- `/Services` — Serviços de domínio (ex: ProductService, UserService)
- `/Models` — Modelos de domínio e ViewModels

---

## 📅 Roadmap

Veja o arquivo [`roadmap.md`](./roadmap.md) para detalhes de progresso, próximos passos e prioridades.

---

## ⚠️ Limitações do Blazor e Componentização (MVP)

Durante o desenvolvimento, encontramos **limitações sérias do Blazor** ao tentar componentizar partes do sistema, especialmente:

### 1. Filtros reutilizáveis (`FilterBar.razor`)

- **Problema:** O two-way binding (`@bind-Name`, etc) entre pai e filho não dispara setters ou métodos no componente pai de forma confiável.
- **Tentativas:** Usamos `[Parameter]`, `EventCallback<T>`, setters com lógica, eventos explícitos, debounce, etc.
- **Resultado:** O filtro só funciona se o código for replicado diretamente na página, inviabilizando o DRY.

### 2. Componente de Pagamento (`PaymentDisplay.razor`)

- **Problema:** Botões no componente filho não conseguem disparar métodos do pai via `EventCallback` de forma consistente.
- **Tentativas:** Mesmo padrão, mesmo resultado: só funciona se o botão estiver na página pai.

#### O que já foi tentado

- Uso correto de `[Parameter]` e `EventCallback<T>`
- Propriedades com setter no pai
- Teste com e sem `@bind-Value:event="oninput"`
- Clean, rebuild, restart do projeto
- Teste em página mínima isolada
- Conferência de namespaces e imports
- Teste de log no componente e na página

#### Workaround MVP

> **Para não travar o projeto, replicamos o código de filtro e pagamento nas páginas onde são usados.**

#### Pós-MVP

- Refatorar para componentes reutilizáveis assim que o Blazor corrigir essas limitações ou surgir um padrão confiável.
- Revisitar este README para atualizar a estratégia.

---

## 🧩 Features a serem desacopladas para componentes (DRY)

Devido às limitações acima, **as próximas features planejadas para serem componentizadas** (mas que, por ora, estão sendo replicadas nas páginas) são:

1. **Paginação**  
   - Idealmente um componente reutilizável para todas as listagens.
2. **Filtros**  
   - Já tentado, mas não viável no MVP por problemas de callback.
3. **Exportações**  
   - Exportar dados (CSV, PDF, etc) de qualquer listagem via componente.
4. **Outros componentes de ação**  
   - Ex: botões de ação em tabelas, toolbars, notificações customizadas, etc.

> **Todas essas features seriam componentes DRY, caso o Blazor suportasse callbacks e binding de forma confiável entre pai e filho.**

---

**Se você é dev e conhece uma solução definitiva para esse cenário, por favor, abra uma issue ou PR!**

---

## 🤝 Contribuição

- Sinta-se à vontade para sugerir melhorias ou abrir issues.
- O projeto está em evolução e aberto a feedback!

---

## 👤 Autoria

Desenvolvido por Freeza e colaboradores.
