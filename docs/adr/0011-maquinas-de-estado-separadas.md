# ADR-0011 — Máquinas de estado separadas para pedido, pagamento, comanda e impressão

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

**Fato:** o protótipo tem um único campo de status no pedido, com cinco passos exibidos ao cliente (recebido, confirmado, em preparação, pronto, finalizado), avançados por um botão de simulação.

No sistema real, quatro coisas evoluem de forma independente: o pedido como registro comercial, o pagamento, o trabalho da cozinha e a impressão do papel. Uma impressão pode falhar enquanto o preparo segue normalmente; um pagamento pode demorar enquanto nada acontece na cozinha.

## Decisão

Cada conceito tem os seus próprios estados e as suas próprias regras de transição:

| Conceito | Estados | Pertence a |
|---|---|---|
| **Pedido** | `AwaitingPayment`, `Placed`, `Confirmed`, `InPreparation`, `Ready`, `OutForDelivery`, `Completed`, `Cancelled`, `Rejected`, `Expired` | Ordering |
| **Pagamento** | `Pending`, `Paid`, `Failed`, `Expired`, `Refunded` | Payments |
| **Comanda — documento** | `Issued`, `Closed`, `Voided` | Kitchen |
| **Comanda — preparo** | `Queued`, `InPreparation`, `Ready`, `Dispatched` | Kitchen |
| **Tarefa de impressão** | `Pending`, `Leased`, `Printed`, `Abandoned`, `Cancelled` | Printing |

Regras:

1. Cada máquina é alterada **apenas pelo seu módulo**.
2. A influência entre máquinas acontece por **eventos**, conforme uma tabela explícita em [order-and-printing-flow.md](../architecture/order-and-printing-flow.md#35-como-as-máquinas-se-influenciam).
3. **O estado da impressão nunca altera o pedido nem o preparo.** Falha de impressão gera alerta.
4. Os estados `InPreparation` e `Ready` do pedido são **reflexo** do preparo, não uma segunda fonte.
5. Toda transição é condicional ao estado atual, de modo que duas requisições simultâneas não avançam duas vezes.
6. Os cinco passos do protótipo continuam sendo o que o consumidor vê, mapeados a partir do estado do pedido.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Um único status no pedido | Obriga a escolher um só valor para situações simultâneas (em preparo **e** com impressão falha), e mistura o que o cliente vê com detalhes internos. |
| Status do pedido mais indicadores booleanos (`impresso`, `pago`) | Booleanos não representam tentativas, falhas com motivo, expiração nem estorno. |
| Preparo por item desde o início | Mais fiel a cozinhas com estações, mas desnecessário enquanto houver uma fila única. Fica como evolução. |
| Comanda sem estado próprio, derivada do pedido | Impediria anular uma comanda, reemiti-la ou, no futuro, ter mais de uma por pedido. |

## Justificativa

- Reflete o que acontece de fato: são processos diferentes, com atores e falhas diferentes.
- Permite a regra operacional mais importante do fluxo: a cozinha não para porque a impressora parou.
- Mantém cada módulo dono do seu ciclo de vida, coerente com as fronteiras do monólito modular.
- Evita expor ao consumidor estados internos.

## Consequências

**Positivas**

- Cada falha tem um lugar próprio para ser representada e observada.
- Novos estados em uma máquina não afetam as outras.
- Histórico de tentativas de impressão e de pagamento naturalmente modelado.

**Negativas**

- Mais estados e mais transições a implementar e testar.
- A tabela de influência entre máquinas precisa ser mantida como parte do desenho; um evento esquecido deixa as máquinas dessincronizadas.
- Relatórios que combinam os conceitos exigem junções.

## Quando rever

- Estações de cozinha forem introduzidas: o preparo passa a ser por estação.
- Pedidos puderem ser alterados depois de confirmados: a comanda passa a ter versões.

## Relacionados

- [order-and-printing-flow.md](../architecture/order-and-printing-flow.md#3-as-quatro-máquinas-de-estado)
- [ADR-0007](0007-consistencia-outbox-e-fila-em-tabelas.md), [ADR-0009](0009-agente-de-impressao-local.md)
