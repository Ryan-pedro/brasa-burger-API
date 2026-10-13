# Brasa Burger — API

Sistema de pedidos para uma hamburgueria: loja virtual, painel administrativo, painel da cozinha e impressão automática de comandas.

Este repositório tem a **API (.NET 10)**, o banco local e a documentação de arquitetura. O front-end (loja e painel, em Angular) fica em [brasa-burger-front-end](https://github.com/Ryan-pedro/brasa-burger-front-end).

> **Estágio atual:** Fase 2 (cardápio). A API já tem o perfil da loja, login, sessão em cookie, papéis, permissões e o cardápio (categorias e produtos).
> "Brasa Burger" é o nome da loja e o codinome do projeto. Os namespaces usam `Brasa`.

## Estrutura do repositório

```
brasa-burger/
├─ src/
│  ├─ Brasa.Domain/          # entidades e regras de negócio; não depende de nada
│  ├─ Brasa.Application/     # casos de uso; depende só de Domain
│  ├─ Brasa.Infrastructure/  # banco de dados (EF Core + PostgreSQL) e Identity
│  └─ Brasa.Api/             # endpoints HTTP e composição
├─ tests/
│  ├─ Brasa.UnitTests/          # domínio e aplicação
│  ├─ Brasa.ArchitectureTests/  # regras de dependência entre camadas
│  └─ Brasa.IntegrationTests/   # API em memória contra PostgreSQL real
├─ infra/compose/            # PostgreSQL local (Docker Compose)
├─ docs/
│  ├─ architecture/          # arquitetura do sistema (inclui a do front-end)
│  └─ adr/                   # registros de decisão
├─ Brasa.slnx                # solução
├─ Directory.Build.props     # configurações comuns a todos os projetos
└─ global.json               # versão do SDK do .NET
```

Diretório previsto e ainda não criado: `contracts/` (OpenAPI).

## Como rodar

Requisitos: .NET SDK 10 e Docker Desktop. Todos os comandos rodam na raiz do repositório.

```powershell
docker compose -f infra/compose/compose.dev.yml up -d   # PostgreSQL em localhost:5432
dotnet build
dotnet test
dotnet run --project src/Brasa.Api                      # http://localhost:5143/api/v1/health
```

Banco local: `brasa`, usuário `brasa`, senha `brasa_dev` (somente desenvolvimento).

Os testes de integração precisam do PostgreSQL no ar. Eles usam um banco à parte (`brasa_test`), recriado a cada execução.

Em desenvolvimento, a API aplica as migrations e cria a loja e a equipe de demonstração ao iniciar. O perfil público fica em http://localhost:5143/api/v1/store/profile.

### Usuários de demonstração

Todos com a senha `brasa-dev-1234` (válida somente em desenvolvimento):

| E-mail | Papel |
|---|---|
| `dono@brasa.dev` | Dono |
| `gerente@brasa.dev` | Gerente |
| `cozinha@brasa.dev` | Cozinha |

Os endpoints do painel ficam em `/api/v1/admin`. Requisições que alteram estado precisam do cabeçalho `X-Brasa-Csrf`. O arquivo `src/Brasa.Api/Brasa.Api.http` tem exemplos prontos.

### Cardápio

O cardápio público fica em `GET /api/v1/store/menu`: só categorias ativas e com produtos; produtos indisponíveis aparecem marcados (`isAvailable: false`). Preços trafegam em centavos (`priceInCents`, ADR-0012).

No painel, sob `/api/v1/admin/catalog`:

| Rota | Permissão |
|---|---|
| `GET /categories`, `GET /products?categoryId=` | `catalog.read` |
| `POST /categories`, `PUT /categories/{id}` | `catalog.write` |
| `POST /products`, `PUT /products/{id}` | `catalog.write` |
| `PUT /products/{id}/availability` | `catalog.write` |

Dono e gerente têm as duas permissões; a cozinha, nenhuma. Erros de validação voltam com `code: "validation_failed"` e as mensagens por campo em `errors`.

Em desenvolvimento, a API cria o cardápio do protótipo (31 produtos em 6 categorias) na primeira vez que sobe.

### Com o front-end

Deixe os dois repositórios lado a lado e suba a API por aqui:

```
Documents/
├─ brasa-burger/            # este repositório
└─ brasa-burger-front-end/  # loja (http://localhost:4200) e painel (http://localhost:4201)
```

## Migrations

```powershell
dotnet ef migrations add <Nome> --project src/Brasa.Infrastructure --startup-project src/Brasa.Api --output-dir Persistence/Migrations
```

As migrations ficam em `src/Brasa.Infrastructure/Persistence/Migrations`. Fora de desenvolvimento, a aplicação não as aplica sozinha.

## Documentação

Comece por [docs/architecture/architecture-overview.md](docs/architecture/architecture-overview.md).

| Documento | Conteúdo |
|---|---|
| [Roteiro de implementação](docs/architecture/implementation-roadmap.md) | Fases, critérios de aceite, riscos e decisões pendentes |
| [Backend](docs/architecture/backend-architecture.md) | Camadas, módulos e padrões da API |
| [Frontend](docs/architecture/frontend-architecture.md) | As aplicações Angular (código no outro repositório) |
| [Banco de dados](docs/architecture/database-and-tenancy.md) | PostgreSQL e modelo conceitual |
| [Pedidos e impressão](docs/architecture/order-and-printing-flow.md) | Fluxo do pedido até a comanda impressa |
| [Segurança](docs/architecture/security.md) | Identidade, autorização e ameaças |
| [Infraestrutura e CI/CD](docs/architecture/infrastructure-and-cicd.md) | Ambientes, hospedagem e pipelines |
| [Decisões (ADR)](docs/adr/README.md) | Registro das decisões de arquitetura |

## Decisões já tomadas

- Banco de dados: **PostgreSQL**.
- Backend: monólito modular em **.NET**.
- **Uma única loja por instalação** ([ADR-0016](docs/adr/0016-uma-unica-loja.md)). Outro cliente, se houver, terá uma instalação separada.
- **Front-end em repositório próprio** ([ADR-0017](docs/adr/0017-front-end-em-repositorio-proprio.md)). Mudança de contrato da API: merge da API primeiro.

## Convenções

- `main` sempre funcionando; trabalho em branches curtas a partir dela (`feat/...`, `fix/...`, `docs/...`, `chore/...`), com pull request.
- Commits no padrão Conventional Commits, com escopo: `feat(identity): ...`, `fix(api): ...`, `docs(adr): ...`.
- Avisos de compilação quebram o build (`TreatWarningsAsErrors`).
