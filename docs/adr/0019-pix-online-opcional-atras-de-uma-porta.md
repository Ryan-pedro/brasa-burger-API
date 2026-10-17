# ADR-0019 — Pix online opcional, atrás de uma porta para o gateway

- Status: **Aceito** (2026-10-14)
- Data: 2026-10-14

## Contexto

A fase 7 prevê receber por Pix antes de o pedido ir para a cozinha. O gateway ainda não foi escolhido (DP-03), e a loja continua recebendo na entrega ou na retirada.

## Decisão

1. **Pix online é opcional**, ao lado de dinheiro, cartão e Pix na entrega. A loja liga nas configurações; o cliente escolhe no checkout.
2. Só o pedido com Pix online nasce **aguardando pagamento** e só vai para a cozinha depois da confirmação. O prazo para pagar é de 15 minutos.
3. A integração fica atrás da porta **`IPixGateway`** (criar cobrança, consultar cobrança, ler o aviso assinado). Hoje existe só o **gateway falso**, que funciona apenas em desenvolvimento e testes.
4. O aviso do gateway (webhook) é validado pela assinatura, gravado numa **caixa de entrada** com unicidade por evento e processado em segundo plano. O estado e o valor são sempre **consultados no gateway**, nunca lidos do corpo do aviso.
5. Uma **conferência periódica** cobre avisos perdidos e expira os pedidos não pagos.
6. Pagamento depois do prazo ou com valor diferente **não reabre o pedido**: o pagamento fica marcado (`PaidLate` ou `AmountMismatch`) e gera alerta para a loja devolver o dinheiro ou combinar com o cliente.

## Alternativas consideradas

| Alternativa | Avaliação |
|---|---|
| Pix online obrigatório | Simplifica a operação, mas afasta quem paga na entrega. Pode ser revisto com os dados do piloto. |
| Integrar um gateway agora | Escolha sem comparação de taxas e contratos; a porta permite decidir depois sem refazer o fluxo. |
| Confiar no corpo do aviso | Mais rápido, porém aceita avisos forjados ou fora de ordem. |

## Justificativa

- O fluxo completo (cobrança, aviso, expiração, conferência e alertas) fica pronto e testado sem depender da escolha comercial do gateway.
- A caixa de entrada e a consulta ao gateway seguem [security.md](../architecture/security.md#11-integrações-externas-webhooks-de-pagamento).

## Consequências

- O primeiro gateway real exige um adaptador de `IPixGateway` em Infrastructure e a configuração das credenciais.
- O cliente precisa da tela de pagamento no Storefront, com o código copia e cola e a atualização do estado.
- Devoluções continuam manuais.

## Quando rever

- Na escolha do gateway (DP-03), inclusive se cada loja terá a própria conta.
- Se o piloto mostrar muitos pedidos falsos com pagamento na entrega.

## Relacionados

- [implementation-roadmap.md](../architecture/implementation-roadmap.md#fase-7--pagamento-online), [ADR-0011](0011-maquinas-de-estado-separadas.md), [ADR-0007](0007-consistencia-outbox-e-fila-em-tabelas.md)
