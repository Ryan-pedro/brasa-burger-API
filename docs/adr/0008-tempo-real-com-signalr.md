# ADR-0008 — Tempo real com SignalR: aviso por push, verdade por consulta

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

O painel da cozinha precisa mostrar pedidos novos sem recarregar; o painel administrativo precisa refletir o andamento; o agente de impressão precisa saber rápido que há trabalho. Conexões em tempo real caem, e mensagens enviadas durante a queda se perdem. A stack sugerida cita SignalR ou equivalente.

## Decisão

1. Usar **SignalR** para avisos do servidor aos clientes, com três hubs: `kitchen`, `ops` (Admin) e `agent`.
2. **Os avisos são dicas, não dados.** A mensagem carrega tipo, identificador e versão. O conteúdo é obtido por HTTP.
3. **Os hubs transmitem só do servidor para o cliente.** Comandos vão por HTTP.
4. **O servidor define o grupo** de cada conexão a partir da identidade autenticada. O cliente não informa loja nem grupo.
5. **Todo cliente reconsulta o estado completo ao reconectar** e também periodicamente.
6. **Sem backplane no MVP**, com uma única instância.
7. **O consumidor acompanha o pedido por consulta periódica** no MVP, não por conexão em tempo real.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Consulta periódica em todas as interfaces | Simples, mas atraso perceptível e carga constante no painel da cozinha, onde segundos importam. |
| Eventos enviados pelo servidor (SSE) | Bom para fluxo unidirecional, mas o SignalR já traz reconexão, grupos e alternativas de transporte integrados ao ASP.NET Core. |
| WebSocket sem biblioteca | Reimplementaria reconexão, grupos e autenticação. |
| Enviar o conteúdo completo no aviso | Tornaria a correção dependente de nenhuma mensagem se perder e faria dados trafegar fora do caminho normal de autorização. |
| Comandos pelo hub | Duplicaria validação, autorização e tratamento de erro em dois caminhos. |
| Tempo real para o consumidor desde o início | Conexões anônimas de longa duração aumentam custo e superfície de abuso, com ganho pequeno. |

## Justificativa

- Se a correção depende só de consultas, um aviso perdido custa segundos de atraso, nunca um pedido perdido.
- Mensagens sem conteúdo eliminam o risco de dados chegarem ao destinatário errado: mesmo um erro de grupo não vaza nada.
- Um único caminho para comandos mantém autorização e validação em um lugar.
- Grupos definidos pelo servidor impedem que um cliente se inscreva em eventos de outra loja.

## Consequências

**Positivas**

- Reconexão segura por construção.
- Isolamento entre lojas simples de testar.
- O mesmo padrão serve ao painel, ao admin e ao agente.

**Negativas**

- Cada aviso gera uma requisição HTTP de consulta. Aceitável na carga esperada.
- Sem backplane, só o processo que mantém as conexões consegue enviar avisos, o que obriga os serviços em segundo plano a rodarem no processo da API ([ADR-0007](0007-consistencia-outbox-e-fila-em-tabelas.md)).
- Escalar para mais de uma instância exigirá backplane e afinidade de sessão.

## Quando rever

- For necessária mais de uma instância da API: adicionar backplane.
- O volume de reconsultas pesar: incluir no aviso os dados mínimos para atualização local, mantendo a reconsulta na reconexão.
- O acompanhamento em tempo real virar requisito para o consumidor.

## Relacionados

- [frontend-architecture.md](../architecture/frontend-architecture.md#74-estado-e-tempo-real)
- [database-and-tenancy.md](../architecture/database-and-tenancy.md#87-signalr)
