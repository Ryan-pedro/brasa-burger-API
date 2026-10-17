# ADR-0020 — Observabilidade com OpenTelemetry e exportação OTLP

- Status: **Aceito** (2026-10-14)
- Data: 2026-10-14

## Contexto

A fase 6 exige logs, métricas e alertas fora da máquina da aplicação. O serviço de destino (DP-12) ainda não foi escolhido e depende da hospedagem (DP-04).

## Decisão

1. A API usa **OpenTelemetry** para logs, métricas e rastros.
2. A exportação é por **OTLP**, configurada só por variáveis de ambiente padrão (`OTEL_EXPORTER_OTLP_ENDPOINT` e afins). Sem endpoint, nada é exportado.
3. As métricas do negócio ficam num medidor próprio (`Brasa`): contadores no momento do fato e medidores amostrados a cada 30 segundos por uma tarefa em segundo plano.
4. A saúde tem duas rotas: `/api/v1/health` (processo no ar) e `/api/v1/health/ready` (também confere o banco).

## Alternativas consideradas

| Alternativa | Avaliação |
|---|---|
| SDK de um fornecedor específico | Prende a escolha do DP-12 ao código. |
| Só logs estruturados | Não atende os alertas de outbox, impressão e pagamentos. |

## Justificativa

- Trocar de fornecedor é mudar variáveis de ambiente.
- Os alertas de [infrastructure-and-cicd.md](../architecture/infrastructure-and-cicd.md#83-alertas-iniciais) passam a ter uma métrica correspondente ([monitoramento.md](../operations/monitoramento.md)).

## Consequências

- Pacotes novos: `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Instrumentation.AspNetCore` e `OpenTelemetry.Instrumentation.Http`.
- As regras de alerta são configuradas no serviço escolhido.

## Quando rever

- Na escolha do destino (DP-12).

## Relacionados

- [ADR-0014](0014-hospedagem-do-mvp.md), [monitoramento.md](../operations/monitoramento.md)
