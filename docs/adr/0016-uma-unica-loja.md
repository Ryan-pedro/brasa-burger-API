# ADR-0016 — Uma única loja por instalação

- Status: **Aceito** (2026-10-13)
- Data: 2026-10-13
- Substitui: [ADR-0003](0003-multi-tenancy-banco-compartilhado.md) e, em parte, [ADR-0004](0004-resolucao-de-tenant-por-host.md)

## Contexto

A arquitetura original previa um SaaS para várias hamburguerias no mesmo sistema, com isolamento por `TenantId`, loja resolvida pelo endereço e um painel de operação da plataforma.

Depois de ver isso funcionando com duas lojas de demonstração, a decisão do responsável pelo produto foi atender **um único cliente: uma loja**.

Quando a decisão foi tomada, já existiam no código a resolução de loja pelo host, o filtro global por loja, os vínculos usuário–loja e os testes de isolamento. Nada estava em produção.

## Decisão

1. **Uma instalação atende uma loja.** Não existe `TenantId`, nem tabela de lojas, nem vínculo de usuário com loja.
2. **Os dados da loja são um registro único** (`Store`), com identificador fixo.
3. **O papel fica no próprio usuário** (proprietário, gerente ou cozinha), junto com a indicação de ativo ou desativado.
4. **As interfaces continuam separadas:** loja do consumidor, painel administrativo e painel da cozinha, cada uma com o seu endereço e a sua sessão. Isso vem do [ADR-0010](0010-autenticacao-e-sessoes.md) e continua valendo.
5. **Sai do escopo:** Platform Admin, planos, assinaturas, acesso de suporte entre lojas, resolução de loja pelo endereço, filtros globais por loja, chaves compostas e Row-Level Security.
6. **Se surgir outro cliente**, a primeira opção é uma **instalação separada**, com banco e implantação próprios, em vez de voltar ao modelo multi-loja.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Manter o multi-loja e usar com uma loja só | Paga o custo de complexidade (filtros, isolamento, testes, telas de plataforma) sem usar o benefício. |
| Manter o `TenantId` nas tabelas "por precaução" | Complexidade sem uso, e que tende a ficar inconsistente justamente por não ser exercitada. |

## Consequências

**Positivas**

- Código, banco e testes bem mais simples.
- A loja do consumidor funciona em um endereço comum, sem subdomínio por loja.
- Uma aplicação Angular a menos (Platform Admin).
- Menos riscos de segurança: não há como vazar dados entre lojas.

**Negativas**

- **Voltar a ter várias lojas no mesmo sistema seria caro:** exigiria `TenantId` em todas as tabelas, migração de dados e refazer o isolamento. Instalações separadas por cliente evitam isso, ao custo de mais implantações para manter.
- Cada instalação nova tem custo próprio de infraestrutura.

## O que muda no código existente

| Antes | Depois |
|---|---|
| `Tenant`, `TenantDomain`, `TenantSlug`, `TenantStatus` | `Store` (registro único) |
| `Membership` e `TenantRole` | `StaffRole` e `IsActive` no usuário |
| `TenantContext`, filtro global, verificação de loja na gravação | Removidos |
| Loja resolvida pelo host (`StoreTenantFilter`) | Removido; o perfil público lê o registro único |
| Troca de loja na sessão | Removida |
| Testes de isolamento entre lojas | Removidos |
| Storefront com telas de "loja não encontrada" e "loja indisponível" | Só a tela de erro de conexão |

## Relacionados

- [ADR-0003](0003-multi-tenancy-banco-compartilhado.md), [ADR-0004](0004-resolucao-de-tenant-por-host.md), [ADR-0010](0010-autenticacao-e-sessoes.md)
