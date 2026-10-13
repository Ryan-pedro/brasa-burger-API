# ADR-0013 — Contratos OpenAPI por superfície, versionados no repositório

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

Quatro aplicações Angular e um agente consomem a mesma API. Sem um contrato verificado, frontend e backend divergem em silêncio: um campo renomeado só é percebido em produção.

**Fato:** o `README` do protótipo sugere que os modelos TypeScript existentes "podem servir de base para os DTOs", isto é, tipos mantidos à mão dos dois lados.

Além disso, o consumidor não deve receber, nem no bundle, contratos de recursos administrativos ou de cozinha.

## Decisão

1. A API gera **um documento OpenAPI por superfície**: `store`, `admin`, `kitchen`, `platform` e `agent`.
2. Os documentos são **versionados** em `contracts/openapi/`.
3. O pipeline da API **falha** se o documento gerado pelo código for diferente do versionado.
4. Uma verificação classifica cada mudança como **compatível ou incompatível**.
5. Os **clientes e tipos TypeScript são gerados** a partir dos arquivos versionados, um ponto de entrada por superfície. Tipos de API não são escritos à mão.
6. Cada aplicação só pode importar o cliente da sua superfície, verificado por lint.
7. O código gerado **não é versionado**: é produzido no build.
8. A interface interativa de documentação fica disponível só em desenvolvimento e homologação.
9. O protocolo do agente é descrito também por um projeto de contratos .NET compartilhado, com versão própria.

A escolha da ferramenta de geração é a pendência DP-10.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Tipos mantidos à mão nos dois lados | Divergem com o tempo, sem nenhum aviso. |
| Um único documento OpenAPI | O cliente do consumidor carregaria contratos administrativos; qualquer mudança dispararia o build de todas as aplicações. |
| Documento gerado só em tempo de execução, sem versionar | Mudanças de contrato não apareceriam na revisão do pull request. |
| Escrever o OpenAPI primeiro e gerar o servidor | Processo mais pesado; para uma equipe pequena, gerar a partir do código com verificação dá o mesmo resultado. |
| GraphQL ou RPC tipado | Outra pilha a aprender e operar, sem necessidade que a justifique. |
| Versionar o código gerado | Diferenças grandes e ruidosas nos pull requests; risco de edição manual. |

## Justificativa

- O contrato versionado torna toda mudança de API **visível na revisão**.
- A geração de clientes faz o compilador apontar incompatibilidades no mesmo pull request, graças ao monorepo.
- A separação por superfície reforça o isolamento entre públicos e permite builds independentes: mudar o contrato administrativo não reconstrói o Storefront.

## Consequências

**Positivas**

- Frontend e backend não divergem sem que o pipeline acuse.
- Mudanças incompatíveis são identificadas antes do merge.
- Cada aplicação conhece apenas a sua parte da API.

**Negativas**

- Quem altera um endpoint precisa regenerar e incluir o documento no pull request.
- Depende da qualidade da ferramenta de geração e da fidelidade do documento gerado pelo .NET.
- Cinco documentos a manter coerentes entre si em convenções de erro e paginação.

## Quando rever

- A ferramenta de geração se mostrar instável ou gerar código inadequado.
- Surgirem consumidores externos da API, o que exigirá documentação pública e política formal de depreciação.

## Relacionados

- [backend-architecture.md](../architecture/backend-architecture.md#13-documentação-openapi)
- [infrastructure-and-cicd.md](../architecture/infrastructure-and-cicd.md#23-contratos-openapi-como-elo-entre-frontend-e-backend)
- [ADR-0005](0005-estrutura-do-monorepo.md)
