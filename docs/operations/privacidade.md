# Privacidade e LGPD

A loja é a controladora dos dados dos clientes; o sistema trata nome, telefone, endereço e observações só para atender o pedido ([security.md](../architecture/security.md#12-privacidade-e-lgpd)). Prazos e papéis ainda precisam de validação jurídica (DP-09).

## Retenção

| Dado | Prazo | O que acontece |
|---|---|---|
| Nome, telefone, endereço e observações do pedido | 12 meses depois da criação, com o pedido encerrado | O nome vira "Cliente anonimizado"; os demais campos são apagados. Valores, itens e datas continuam. |
| Comanda da cozinha | Junto com o pedido | Nome do cliente e observações apagados. |
| Tarefas de impressão | Junto com o pedido | Apagadas (o documento impresso tem nome e endereço). |
| Chaves de idempotência | 24 horas | Apagadas. |
| Avisos de tempo real entregues | 7 dias | Apagados. |

A anonimização roda a cada 6 horas (`RemoveExpiredPersonalDataJob`). O prazo fica em `PersonalDataRetention.Period`.

## Pedido do titular

Quando um cliente pede a exclusão dos seus dados:

1. Confirme a identidade pelo canal de atendimento da loja (por exemplo, mensagem do mesmo telefone usado nos pedidos).
2. O proprietário chama `POST /api/v1/admin/privacy/personal-data-removals` com o telefone e, se o cliente informar, os números dos pedidos:

   ```json
   { "phone": "(31) 99999-0000", "orderNumbers": [1048] }
   ```

3. A resposta traz quantos pedidos foram anonimizados e os números dos que ainda estão em andamento. Repita depois que eles forem encerrados.
4. Responda ao cliente com a data da exclusão.

Pedidos de acesso aos dados são atendidos pelo detalhe do pedido no painel. A ação fica na auditoria (`privacy.personal_data_removed`), sem o telefone.

Os backups guardam os dados até expirarem pela retenção de 30 dias.
