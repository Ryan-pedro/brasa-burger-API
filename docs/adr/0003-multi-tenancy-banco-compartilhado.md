# ADR-0003 — Multi-tenancy em banco compartilhado, com quatro camadas de isolamento

- Status: **Substituído por [ADR-0016](0016-uma-unica-loja.md)** (2026-10-13): o sistema passou a atender uma única loja.
- Data: 2026-10-11

## Contexto

Cada estabelecimento precisa ter os seus dados isolados dos demais. O público-alvo são muitas lojas pequenas, o que exige custo por loja baixo e criação de loja simples. Um vazamento de dados entre lojas é o risco mais grave do produto.

## Decisão

Todas as lojas compartilham **um banco e um esquema**. O isolamento é lógico, pela coluna `TenantId`, e imposto por **quatro camadas independentes**:

1. **Contexto de tenant** definido pelo servidor, uma única vez por requisição ou item de trabalho, e imutável depois.
2. **Filtro global de consulta** no EF Core para toda entidade de loja, com carimbo e verificação do `TenantId` na gravação.
3. **Chaves estrangeiras compostas** que incluem `TenantId`, de modo que o banco recusa referência entre lojas.
4. **Row-Level Security** no PostgreSQL, com a aplicação conectando por um papel que não pode ignorar políticas.

Uma suíte automatizada de testes de isolamento é obrigatória em todo pull request.

## Alternativas consideradas

| Alternativa | Por que não agora |
|---|---|
| Schema por loja | Migrations multiplicadas pelo número de lojas; relatórios da plataforma trabalhosos; pouco ganho sobre RLS. |
| Banco por loja | Isolamento forte, mas custo e operação incompatíveis com centenas de lojas pequenas. |
| Banco compartilhado só com filtro na aplicação | Uma consulta escrita à mão sem o filtro vaza dados, sem nenhuma barreira adicional. |

## Justificativa

- É o modelo de menor custo por loja e de operação mais simples: uma migration, um backup, uma conexão.
- Cada camada cobre a falha da anterior. Um filtro esquecido é barrado pela política do banco; uma política mal configurada é coberta pelo filtro.
- Chaves compostas tornam a referência cruzada **impossível**, e não apenas improvável.
- O modelo permite mover uma loja para um banco dedicado depois, porque os dados de cada loja formam um conjunto fechado.

## Consequências

**Positivas**

- Criar uma loja é inserir linhas.
- Relatórios da plataforma são consultas simples.
- Defesa em profundidade contra o risco mais grave.

**Negativas**

- RLS com pool de conexões exige disciplina: toda unidade de trabalho em transação, com o tenant definido no escopo da transação.
- Chaves compostas tornam o esquema mais verboso.
- Não há restauração nativa de uma única loja.
- Uma loja com carga anormal pode afetar as outras; mitigado por cotas.

## Concessão admitida

Se a camada 4 atrasar o MVP, as camadas 1 a 3 podem ir primeiro. **A camada 4 precisa estar ativa antes de haver uma segunda loja real em produção.**

## Quando rever

- Uma loja degradar as demais de forma recorrente.
- Exigência contratual ou legal de isolamento físico.
- Necessidade frequente de restaurar uma loja isoladamente.

Nesses casos, o caminho é banco dedicado para as lojas afetadas, mantendo o compartilhado para as demais.

## Relacionados

- [database-and-tenancy.md](../architecture/database-and-tenancy.md#8-estratégia-multi-tenant)
- [ADR-0002](0002-postgresql.md), [ADR-0004](0004-resolucao-de-tenant-por-host.md)
