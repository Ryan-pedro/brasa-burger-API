# ADR-0007 — Transação única entre módulos, Outbox para efeitos externos e filas em tabelas

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

A confirmação de um pedido desencadeia efeitos em outros módulos (criar comanda, criar tarefa de impressão) e fora do banco (avisar a cozinha em tempo real, avisar o agente, chamar o gateway). É preciso garantir que nada se perca se o processo cair no meio, sem introduzir infraestrutura que a equipe não consiga operar.

## Decisão

1. **Efeitos em outros módulos, dentro do banco, acontecem na mesma transação.** O módulo que reage o faz por um manipulador de evento de domínio em processo. Confirmar o pedido, criar a comanda e criar a tarefa de impressão é uma única transação.
2. **Efeitos fora do banco passam por Outbox.** A mensagem é gravada na mesma transação dos dados e entregue depois do commit por um serviço em segundo plano, com novas tentativas.
3. **Filas duráveis são tabelas do PostgreSQL.** A tabela de tarefas de impressão é a fila de impressão; a tabela do outbox é a fila de avisos. A reserva de itens usa bloqueio de linha que ignora itens já reservados.
4. **Não há broker de mensagens no MVP.**
5. **Os serviços em segundo plano rodam no processo da API no MVP**, registrados por configuração, e podem ser movidos para um processo próprio depois.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Avisar diretamente após o commit, sem Outbox | Se o processo cair entre o commit e o envio, o aviso se perde sem registro. |
| Tudo assíncrono por Outbox, inclusive criar a comanda | Módulos mais desacoplados, mas haveria um intervalo em que o pedido está confirmado e a comanda não existe, com mais estados intermediários a tratar. |
| Broker de mensagens | Mais um componente a operar, monitorar e pagar, sem consumidor fora deste processo que o justifique. |
| Biblioteca de agendamento de tarefas em segundo plano | Resolve agendamento, mas a tabela de tarefas de impressão precisa existir de qualquer forma, para histórico e reimpressão. |
| Processo separado para os serviços em segundo plano desde o início | Sem backplane, um processo separado não consegue enviar avisos pelos hubs que estão na API. |

## Justificativa

- Com um banco só, a transação local é a forma mais simples e mais forte de consistência. "Pedido confirmado implica comanda" passa a ser garantido pelo banco.
- O Outbox garante que todo efeito externo decorrente de uma mudança confirmada seja, em algum momento, entregue.
- Usar tabelas como filas elimina um componente e mantém a fila na mesma transação e no mesmo backup dos dados.
- A carga esperada é muito inferior ao que essa abordagem suporta.

## Consequências

**Positivas**

- Não há estado "pedido confirmado sem comanda".
- Nenhum componente adicional de infraestrutura.
- Fila e dados consistentes entre si, inclusive em restauração de backup.

**Negativas**

- Entrega do Outbox é "ao menos uma vez": os consumidores precisam tolerar repetição.
- Um manipulador de evento em processo que falha desfaz a transação inteira, inclusive o pedido. É o comportamento desejado, mas exige que esses manipuladores sejam simples e não façam chamadas externas.
- Os módulos compartilham a transação, o que é um acoplamento. Extrair um módulo no futuro exigirá transformar esses manipuladores em consumidores do Outbox; por isso eles já devem ser idempotentes.
- A latência dos avisos depende do intervalo de varredura do Outbox, mitigada por um sinal imediato após o commit.

## Quando rever

- Surgir um consumidor fora deste processo.
- O volume de mensagens pressionar o banco.
- Um módulo for extraído do monólito.

## Relacionados

- [order-and-printing-flow.md](../architecture/order-and-printing-flow.md#7-tarefas-de-impressão)
- [backend-architecture.md](../architecture/backend-architecture.md#15-processamento-assíncrono)
- [ADR-0008](0008-tempo-real-com-signalr.md), [ADR-0009](0009-agente-de-impressao-local.md)
