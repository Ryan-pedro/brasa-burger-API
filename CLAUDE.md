# CLAUDE.md — Brasa Burger · API

Instruções permanentes para trabalhar neste repositório. Valem para toda tarefa, salvo pedido explícito em contrário.

## O sistema

- Sistema de pedidos de **uma única hamburgueria** (ADR-0016). A loja é um registro único (`Store.SingletonId`). Não existe multi-loja, `TenantId` nem resolução de loja por endereço.
- Dois repositórios (ADR-0017):
  - `brasa-burger-API` (este): API .NET, banco local, documentação de arquitetura (`docs/architecture`) e ADRs (`docs/adr`).
  - `brasa-burger-front-end`: workspace Angular (loja e painel).
- A API é a fonte da verdade para preço, permissão e regra de negócio.
- Antes de uma decisão de arquitetura, leia o ADR relacionado. Decisão que contraria um ADR aceito exige um ADR novo no mesmo PR.

## Stack (o que existe, não acrescente outra)

- .NET 10 (SDK fixado em `global.json`), Minimal APIs, Problem Details.
- EF Core 10 + Npgsql + `EFCore.NamingConventions` (snake_case), PostgreSQL 17 via Docker Compose (`infra/compose/compose.dev.yml`).
- ASP.NET Core Identity (`IdentityUserContext<AppUser, Guid>`) com sessão em cookie.
- Testes: xUnit 2.9.3, `Microsoft.AspNetCore.Mvc.Testing`, PostgreSQL real nos testes de integração.
- Sem MediatR e sem repositório genérico (ADR-0006); mapeamento manual, sem AutoMapper. Não adicione pacotes sem pedido explícito.

## Arquitetura

```
src/
├─ Brasa.Domain/          # entidades e regras; não depende de nada
├─ Brasa.Application/     # casos de uso; depende só de Domain
├─ Brasa.Infrastructure/  # EF Core, Identity, migrations, seed de desenvolvimento
└─ Brasa.Api/             # endpoints, filtros, autenticação, composição
tests/
├─ Brasa.UnitTests/          # domínio e aplicação
├─ Brasa.ArchitectureTests/  # dependências entre camadas
└─ Brasa.IntegrationTests/   # API em memória contra PostgreSQL
```

- A direção das dependências é verificada em `LayerDependencyTests`. Domain não referencia EF Core, ASP.NET Core nem Npgsql.
- Organização por fatia vertical dentro de cada camada: `Application/Catalog/Products/CreateProductHandler.cs`, `Domain/Catalog/Product.cs`, `Api/Endpoints/Admin/AdminCatalogEndpoints.cs`.

### Domain

- Entidades `sealed`, setters privados, construtor privado sem parâmetros para o EF e construtor público que deixa a entidade válida.
- Invariantes protegidas nos métodos (`Rename`, `ChangeDetails`, `SetAvailability`), lançando `ArgumentException`/`ArgumentOutOfRangeException`.
- Ids com `Guid.CreateVersion7()`. Limites de tamanho como constantes públicas (`NameMaxLength`), reaproveitadas na configuração do EF e na validação.
- Dinheiro é `decimal` com duas casas (ADR-0012).

### Application

- Um handler por caso de uso (`XxxHandler`, método `HandleAsync`), registrado como `Scoped` em `AddApplication()`.
- Acesso a dados por `IAppDbContext`. Leitura com `AsNoTracking()`.
- Falha esperada volta como `Result<T>` com `Error` (`Validation`, `NotFound`, `Conflict`) e `code` estável em snake_case (`category_name_taken`, `product_not_found`). Exceção é para erro inesperado.
- Validação de entrada com `FieldErrors`, chaves no nome do campo do JSON (camelCase), mensagens em português. Valide antes de chamar o domínio.
- Contratos (`XxxRequest`, `XxxResponse`) são `record`. Nos contratos, dinheiro trafega em centavos (`long PriceInCents`) e é convertido com `Money`.
- Usuário corrente via `CurrentUser`, preenchido pelo servidor.

### Infrastructure

- Uma `IEntityTypeConfiguration<T>` por entidade, schema por módulo (`store`, `identity`, `catalog`), tabelas em snake_case.
- `numeric(10,2)` para dinheiro; índices e chaves únicas declarados na configuração.
- Migrations geradas com `dotnet ef migrations add <Nome>` (comando no README). Nunca edite migration já aplicada; crie outra.
- `DevelopmentDatabase` só roda em Development e é idempotente.

### Api

- Rotas sob `/api/v1`. Superfície pública em `/store`, painel em `/admin`.
- O grupo `/admin` tem `CsrfHeaderFilter`; rotas autenticadas usam `RequireAuthorization("admin")` + `AdminSessionFilter`, que relê o usuário no banco a cada requisição.
- Autorização por **permissão, nunca por papel**: `.RequirePermission(Permissions.CatalogWrite)`. Papéis viram permissões só em `Permissions.For(role)`.
- Erros sempre por `ApiProblems` (`Create`, `From`, `Match`), com `code` e, em validação, `errors` por campo. Não exponha mensagem de exceção.
- Cookie de sessão HttpOnly, SameSite=Lax, `__Host-` fora de Development/Testing. Não enfraqueça essas regras.
- Exemplos de chamada atualizados em `src/Brasa.Api/Brasa.Api.http`.

## Código

- `Directory.Build.props` liga `Nullable` e `TreatWarningsAsErrors`: o build não aceita aviso de compilação.
- **Sem comentários no código novo.** Nomes claros substituem explicações. Exceção só se o usuário pedir.
- Sem código morto, sem duplicação, sem abstração "para o futuro".
- Classes `sealed` por padrão; `internal` quando não precisam sair do projeto. Construtor primário para injeção.
- Nomes de código em inglês; mensagens, títulos de Problem Details e nomes de teste em português (`Gerente_marca_produto_como_indisponivel`).
- Indentação de 4 espaços em `.cs`/`.csproj`/`.props` (`.editorconfig`).

## Preservar o que existe

- Não altere comportamento fora do escopo da tarefa. Refatoração ampla só com pedido explícito.
- Não mude rota, nome de campo JSON, `code` de erro ou permissão já publicados: o front depende deles. Mudança de contrato vai em PR próprio, com o merge da API antes do front.
- Trabalho sempre em branch (`feat/...`, `fix/...`, `chore/...`, `docs/...`) a partir da `main`. **Nunca grave alterações com a `main` ativa**: confirme a branch antes de editar.
- Ao entregar algo que precisa de pacote novo, migration ou comando, liste os comandos em bloco separado, na ordem de execução.
- Commits em Conventional Commits com escopo: `feat(catalog): ...`, `fix(identity): ...`, `docs(adr): ...`.

## Testes, build e validação

Na raiz do repositório:

```powershell
docker compose -f infra/compose/compose.dev.yml up -d
dotnet build
dotnet test
```

- Regra de domínio nova → teste em `Brasa.UnitTests`. Endpoint novo → teste em `Brasa.IntegrationTests` cobrindo sucesso, validação, permissão (401/403) e, se aplicável, CSRF e 404/409.
- Testes de integração usam `StoreApiFactory` (banco `brasa_test`, recriado a cada execução) e a coleção `api`. Cada teste cria os próprios dados com nomes únicos.
- Entidade nova exige migration gerada; sem ela, todos os testes de integração falham na preparação do banco.
- Só entregue com build sem avisos e todos os testes passando.

## Interfaces e UI

Esta API não tem telas. Qualquer tela que consuma um endpoint novo segue as regras de UI/UX do `CLAUDE.md` do `brasa-burger-front-end`: direção visual definida e aprovada **antes** de implementar, proibição de padrões genéricos de IA e revisão visual em desktop e 390 px após implementar. Ao desenhar um contrato, entregue o que a tela precisa para seus estados (vazio, erro por `code`, campos com `errors`) sem obrigar o front a adivinhar.
