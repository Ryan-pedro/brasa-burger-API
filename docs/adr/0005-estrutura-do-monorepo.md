# ADR-0005 — Estrutura do monorepo e workspace Angular multi-projeto

- Status: **Aceito com ajuste** (2026-10-11): o gerenciador de pacotes continua sendo o **npm**. O item 5 da proposta original (pnpm) foi rejeitado; o restante foi aceito.
- **Parcialmente substituído por [ADR-0017](0017-front-end-em-repositorio-proprio.md)** (2026-10-13): o front-end saiu para o repositório `brasa-burger-front-end`. O item 1 (um repositório para tudo) não vale mais; a organização interna do workspace Angular continua valendo.
- Data: 2026-10-11

## Contexto

**Fatos:** o repositório tem um único diretório, `front-end/`, com uma aplicação Angular 21 na raiz do workspace. O `angular.json` já declara `newProjectRoot: projects`. O gerenciador é o npm, com `legacy-peer-deps=true`. Não há ESLint.

O sistema terá quatro aplicações web, um backend, um agente local, infraestrutura e documentação, com a exigência de que cada parte seja construída e implantada sem obrigar a publicar as outras.

## Decisão

1. **Um repositório** para tudo, com os diretórios de primeiro nível `front-end/`, `back-end/`, `contracts/`, `infra/`, `docs/` e `.github/`.
2. **Frontend:** um único workspace Angular em `front-end/`, com quatro aplicações (`storefront`, `admin`, `kitchen`, `platform`) e quatro bibliotecas (`ui`, `app-core`, `realtime`, `api-clients`) em `projects/`.
3. **Um único `package.json`**, portanto uma única versão do Angular para as quatro aplicações.
4. **Bibliotecas consumidas pelo código-fonte**, por mapeamento de caminhos, sem build nem publicação separados.
5. **npm como gerenciador de pacotes** (já em uso), sem dividir o frontend em vários pacotes. A proposta original era pnpm.
6. **ESLint com regras de fronteira** entre projetos.
7. **Backend:** uma solução .NET em `back-end/`, com versões de pacotes centralizadas e filtros de solução por artefato. O agente de impressão fica nessa solução.
8. **Pipelines por artefato**, disparados por filtros de caminho.

A aplicação existente é movida para `front-end/projects/storefront`. O nome do diretório `front-end/` é mantido.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Um repositório por aplicação | Mudanças que cruzam frontend, contrato e backend virariam vários pull requests coordenados à mão. |
| pnpm workspaces com um pacote por aplicação | Permitiria versões diferentes do Angular por aplicação, o que é indesejável, e duplicaria configuração. |
| Ferramenta de monorepo com grafo de dependências e cache | Vale com muitos projetos e builds lentos; excessiva para quatro aplicações. |
| pnpm como gerenciador | Era a proposta original: resolução estrita e instalação mais rápida no pipeline. **Não adotado**: o npm já funciona e a troca não é necessária agora. |
| Uma única aplicação Angular com áreas por rota | Mistura código e permissões de públicos diferentes no mesmo bundle. É a situação do protótipo que se quer corrigir. |
| Renomear `front-end/` | Sem benefício que justifique alterar o que já existe. |

## Justificativa

- O workspace multi-projeto do Angular já oferece aplicações e bibliotecas isoladas com builds independentes; o repositório já estava configurado para isso.
- Uma versão do framework para tudo simplifica atualizações.
- Bibliotecas por código-fonte evitam um passo de build intermediário e mantêm a recarga rápida no desenvolvimento.
- O monorepo permite que uma mudança de contrato da API e o ajuste no frontend sejam verificados no mesmo pull request.

## Consequências

**Positivas**

- Uma mudança que atravessa camadas é um único pull request.
- Builds e implantações independentes por filtros de caminho.
- Fronteiras entre aplicações verificadas automaticamente.

**Negativas**

- O conflito de dependências contornado por `legacy-peer-deps` continua existindo e deve ser investigado quando o ESLint for adicionado.
- Uma mudança em biblioteca compartilhada reconstrói todas as aplicações que a usam.
- Filtros de caminho com verificações obrigatórias exigem uma verificação "portão" para não bloquear merges.
- Mover a aplicação existente altera caminhos e histórico de arquivos.

## Quando rever

- O tempo de pipeline passar a incomodar: avaliar cache de build ou ferramenta de monorepo.
- Uma aplicação precisar de versão diferente do framework: avaliar pacotes separados.
- O tempo de instalação no pipeline ou dependências não declaradas virarem problema: reavaliar o pnpm.

## Relacionados

- [frontend-architecture.md](../architecture/frontend-architecture.md#2-estrutura-do-workspace)
- [infrastructure-and-cicd.md](../architecture/infrastructure-and-cicd.md#2-estrutura-do-monorepo)
- [ADR-0013](0013-contratos-openapi-por-superficie.md)
