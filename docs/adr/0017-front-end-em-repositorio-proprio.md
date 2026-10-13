# ADR-0017 — Front-end em repositório próprio

- Status: **Aceito** (2026-10-13)
- Data: 2026-10-13
- Atualização (2026-10-13): o repositório da API foi recriado do zero como **`brasa-burger-API`**, com histórico novo; o antigo `brasa-burger` fica arquivado. Onde este ADR diz `brasa-burger`, leia `brasa-burger-API`.
- Substitui parcialmente: [ADR-0005](0005-estrutura-do-monorepo.md) (item 1, "um repositório para tudo")

## Contexto

O [ADR-0005](0005-estrutura-do-monorepo.md) colocou front-end, back-end, infraestrutura e documentação num único repositório. Na prática, o trabalho do front-end e o do back-end acabaram misturados na mesma branch (`feat/back-end-esqueleto`), o que dificulta revisar e publicar cada lado separadamente.

Decisão do responsável pelo projeto em 2026-10-13: o front-end passa a ter um repositório só dele.

## Decisão

1. O workspace Angular sai de `front-end/` no repositório `brasa-burger` e passa a ser a raiz do repositório **`brasa-burger-front-end`**.
2. O histórico da pasta `front-end/` é preservado com `git subtree split`. Commits anteriores à criação da pasta (do protótipo original) ficam só no repositório `brasa-burger`.
3. O repositório `brasa-burger` continua com a API (.NET), a infraestrutura local, os contratos e a documentação de arquitetura, inclusive a do front-end.
4. O restante do ADR-0005 continua valendo dentro de cada repositório: um único workspace Angular com várias aplicações em `projects/`, um único `package.json` e npm.
5. Cada repositório tem a própria integração contínua.
6. Sem o front-end, o conteúdo de `back-end/` (solução, `src/`, `tests/`, `Directory.Build.props`, `global.json`) passa para a raiz do repositório `brasa-burger`, ao lado de `docs/` e `infra/`. Os comandos do .NET passam a rodar na raiz.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Manter o monorepo e separar só as branches (`feat/front-end` e `feat/back-end-...`) | Resolve a mistura de branches, mas não foi a escolha: o front-end deve ter vida própria. |
| Submódulo do Git apontando para o repositório do front | Complica clone, branches e CI para pouco ganho. |
| Um repositório por aplicação Angular (storefront, admin, ...) | Duplicaria configuração e versões do Angular; o workspace multi-projeto continua adequado. |

## Consequências

**Positivas**

- Histórico, branches, pull requests e CI do front-end independentes da API.
- O front pode ser publicado sem tocar no repositório da API, e vice-versa.

**Negativas**

- Uma mudança que altera o contrato da API vira **dois pull requests**, um em cada repositório. Regra: faça o merge da API primeiro e mantenha a API compatível com a versão anterior do front durante a transição.
- Os contratos OpenAPI ([ADR-0013](0013-contratos-openapi-por-superficie.md)) ficam no repositório da API. Quando o front passar a gerar clientes a partir deles, vai buscá-los numa versão publicada (arquivo de release ou o endpoint `/openapi` da API), não por caminho relativo.
- O desenvolvimento local exige os dois repositórios lado a lado. O proxy do Angular (`/api` → `http://localhost:5143`) não muda.
- A documentação de arquitetura do front-end fica no repositório da API, longe do código que descreve.

## Quando rever

- Mudanças de contrato coordenadas entre os dois repositórios passarem a ser frequentes e a quebrar o front: avaliar voltar ao monorepo.

## Relacionados

- [ADR-0005](0005-estrutura-do-monorepo.md)
- [ADR-0013](0013-contratos-openapi-por-superficie.md)
- Repositório do front-end: [Ryan-pedro/brasa-burger-front-end](https://github.com/Ryan-pedro/brasa-burger-front-end)
