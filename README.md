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
- **Hot Reload** para desenvolvimento ágil
- **CSS customizado** (tema escuro centralizado)
- **Estrutura modular**: Pages, Shared, Services, Models, Data

---

## 🔒 Autenticação

- Utilizamos **ASP.NET Core Identity** para login, registro, logout e controle de sessão.
- As telas de login/registro/logout usam Razor Pages do Identity, customizadas para combinar com o layout escuro do app.
- O fluxo de logout é imediato e seguro, com redirecionamento automático para a home.
- Proteção de páginas sensíveis via `[Authorize]` e controle de sessão por cookie.

---

## 📁 Estrutura do Projeto

- `/Pages` — Páginas Blazor principais (Dashboard, etc)
- `/Shared` — Layouts, NavMenu, componentes reutilizáveis
- `/Areas/Identity/Pages` — Telas de autenticação (login, registro, logout, layout customizado)
- `/wwwroot/css/site.css` — CSS principal do app
- `/wwwroot/Identity/css/site.css` — CSS específico para telas do Identity (opcional)
- `/Data` — Contexto do Entity Framework e migrations
- `/Services` — Serviços de domínio (ex: ProductService, UserService)
- `/Models` — Modelos de domínio e ViewModels

---

## 📝 Como rodar

1. **Configure o banco de dados** no `appsettings.json` (PostgreSQL).
2. **Restaure e rode:**
   ```sh
   dotnet restore
   dotnet ef database update
   dotnet watch run
   ```
3. Acesse `http://localhost:5000`

---

## 📅 Roadmap

Veja o arquivo [`roadmap.md`](./roadmap.md) para acompanhar o progresso e as próximas etapas.

---

## 📚 Documentação e Boas Práticas

- Código limpo, comentado e modularizado.
- Estrutura pronta para expansão (novos módulos, integrações, etc).
- Telas do Identity integradas visualmente ao tema do app.
- Roadmap e documentação para facilitar reuso e contribuição.

---

## 🤝 Contribuição

- Sinta-se à vontade para sugerir melhorias ou abrir issues.
- O projeto está em evolução e aberto a feedback!

---

## 👤 Autoria

Desenvolvido por Freeza e colaboradores.

---

**Status:**  
Projeto em desenvolvimento ativo — autenticação funcional, layout escuro, dashboard protegido e estrutura pronta para expansão.
