# Brasa Burger — API

Sistema de pedidos para uma hamburgueria: loja virtual, painel administrativo, painel da cozinha e impressão automática de comandas.

Este repositório tem a **API (.NET 10)**, o banco local e a documentação de arquitetura. O front-end (loja e painel, em Angular) fica em [brasa-burger-front-end](https://github.com/Ryan-pedro/brasa-burger-front-end).

> **Estágio atual:** fases 2 a 4 concluídas na API: cardápio completo, pedidos com pagamento presencial e cozinha em tempo real. Próximas: impressão (fase 5), produção e piloto (fase 6), Pix (fase 7).
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

| `PUT /categories/{id}/parent` (subcategoria) | `catalog.write` |
| `GET /option-groups`, `GET /offers` | `catalog.read` |
| `POST /option-groups`, `PUT /option-groups/{id}`, `PUT /option-groups/{id}/options/{optionId}/availability` | `catalog.write` |
| `POST /offers`, `PUT /offers/{id}` | `offers.write` |
| `POST /images` (multipart, campo `file`, JPEG/PNG/WebP até 2 MB) | `catalog.write` |

Dono e gerente têm essas permissões; a cozinha, nenhuma. Erros de validação voltam com `code: "validation_failed"` e as mensagens por campo em `errors`.

**Grupos de opções** cobrem adicionais (`Addon`), escolhas como a bebida do combo (`Choice`) e ingredientes removíveis (`Removal`), com mínimo e máximo de seleções. Um grupo é reutilizável entre produtos (`optionGroupIds` no produto). Escolha obrigatória sem opção disponível deixa o produto indisponível no cardápio.

**Ofertas** têm dois tipos: preço promocional de um produto (`ProductPrice`) e desconto fixo a partir de um valor (`OrderAmount`). Vale o maior desconto. O cardápio informa `promoPriceInCents`; `GET /api/v1/store/offers` lista as vigentes.

O cardápio público responde com `ETag`: envie `If-None-Match` para receber 304 quando nada mudou.

Em desenvolvimento, a API cria o cardápio do protótipo (31 produtos em 6 categorias), os adicionais, ingredientes removíveis, a bebida dos combos e a oferta "R$ 5 OFF no app" na primeira vez que sobe.

### Pedidos

Superfície pública, sob `/api/v1/store`:

| Rota | O que faz |
|---|---|
| `POST /orders/quote` | Calcula os valores no servidor. Não grava nada. |
| `POST /orders` | Cria o pedido. Exige `Idempotency-Key` e `expectedTotalInCents` da última cotação. |
| `GET /orders/{trackingToken}` | Acompanhamento pelo cliente. O número do pedido não serve como chave. |

- Tipos: `DineIn` (mesa), `Pickup` (balcão) e `Delivery`. Pagamento presencial: `Cash` (com troco opcional), `Card`, `Pix`.
- Total diferente do esperado → 409 `cart_changed`, com a nova cotação em `quote`. Item indisponível → 409 `item_unavailable`, com os itens em `errors`. Loja fechada → 409 `store_closed`. Abaixo do mínimo → 422 `below_minimum_order`. Mesma chave com outro pedido → 422 `idempotency_key_reused`.
- Repetir o mesmo checkout com a mesma chave devolve o mesmo pedido (200).
- Com aceite automático (padrão), o pedido nasce confirmado e já vai para a cozinha. Com aceite manual, fica "recebido" até alguém aceitar; sem resposta no prazo configurado, é recusado sozinho.

No painel, sob `/api/v1/admin`:

| Rota | Permissão |
|---|---|
| `GET /orders?status=&limit=&cursor=`, `GET /orders/{id}` | `orders.read` |
| `POST /orders/{id}/accept`, `/reject`, `/cancel`, `/complete`, `/payment/receive` | `orders.manage` |
| `GET` e `PUT /store/settings`, `PUT /store/status` (abrir e fechar) | `store.settings` |

O detalhe do pedido traz `actions`, a lista do que pode ser feito no estado atual. Recusar e cancelar exigem `reason`.

### Cozinha

Sob `/api/v1/kitchen`, com login de cozinha:

| Rota | Permissão |
|---|---|
| `GET /tickets` (quadro), `GET /tickets/{id}` | `kitchen.tickets.read` |
| `POST /tickets/{id}/advance`, `POST /tickets/{id}/undo`, com `{ "from": "<etapa atual>" }` | `kitchen.tickets.update` |

Etapas do preparo: `Queued` → `InPreparation` → `Ready` → `Dispatched`. O pedido acompanha o preparo. O campo `from` impede que dois toques simultâneos avancem duas vezes (o segundo recebe 409).

**Tempo real:** os hubs SignalR `/api/v1/hubs/kitchen` (cozinha) e `/api/v1/hubs/ops` (painel) enviam o evento `changed` com `{ type, id }` (`ticket.changed` ou `order.changed`). O aviso é só uma dica: o cliente busca os dados por HTTP, ao receber o aviso, ao reconectar e periodicamente.

### Com o front-end

Deixe os dois repositórios lado a lado e suba a API por aqui:

```
Documents/
├─ brasa-burger-API/        # este repositório
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
