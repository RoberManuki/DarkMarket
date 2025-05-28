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

## Problema conhecido: EventCallback não funciona em componente Blazor

Tentamos centralizar a lógica de exibição do pagamento em um componente (`PaymentDisplay.razor`), mas o botão de callback nunca chama o método na página pai, apesar de todas as tentativas de ajuste de tipo, rebuild, etc.

**Workaround:**  
Repetimos o código de exibição e botão nas telas `Payment.razor` e `ViewPayment.razor` até encontrar uma solução definitiva ou resposta da comunidade.

Se você souber a solução, por favor, abra uma issue ou envie um PR!

**Status:**  
Projeto em desenvolvimento ativo — autenticação funcional, layout escuro, dashboard protegido e estrutura pronta para expansão.

---

🚨 Documentação do Problema: EventCallback não funciona em componente Blazor
Resumo do problema
Criamos um componente Blazor chamado PaymentDisplay.razor para centralizar a exibição do endereço, valor e botão "Verificar pagamento".
O botão "Verificar pagamento" chama um EventCallback para executar um método na página pai.
O botão é renderizado, mas o clique nunca chama o método na página pai.
Testamos todas as variações possíveis de EventCallback, tipos de parâmetro, chamadas, rebuild, clean, etc.
O mesmo método funciona se chamado por um botão diretamente na página, mas não via componente.
O que já foi tentado
EventCallback e EventCallback<object?> com .InvokeAsync() e .InvokeAsync(null)
Parâmetro obrigatório e opcional
Teste em página mínima (Teste.razor)
Remover dependências externas e markup comentado
Clean, rebuild, restart do projeto
Conferência de namespaces e imports
Teste de log no componente e na página
Conclusão
O problema é específico do uso do componente, não do método nem do binding na página.
Decidimos remover o componente e repetir o código nas telas, até encontrar uma solução definitiva ou resposta oficial da comunidade Blazor.
