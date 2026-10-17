# Monitoramento

A API emite logs, métricas e rastros por OpenTelemetry. O destino (DP-12) é um serviço externo à máquina, configurado por variáveis de ambiente padrão:

| Variável | Exemplo |
|---|---|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `https://otlp.provedor.com` |
| `OTEL_EXPORTER_OTLP_HEADERS` | `Authorization=Bearer <chave>` |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | `http/protobuf` |
| `OTEL_SERVICE_NAME` | `brasa-api` |

Sem `OTEL_EXPORTER_OTLP_ENDPOINT`, nada é exportado e os logs continuam na saída do contêiner.

## O que é coletado

- **Rastros:** requisições HTTP (menos as de saúde), chamadas HTTP de saída e consultas ao PostgreSQL.
- **Métricas padrão:** ASP.NET Core (requisições, duração, erros), conexões SignalR, limitador de taxa, runtime do .NET e Npgsql.
- **Métricas do negócio** (medidor `Brasa`):

| Métrica | Tipo | Significado |
|---|---|---|
| `brasa.orders.created` | contador (`type`, `payment_method`) | Pedidos criados |
| `brasa.auth.login_failures` | contador (`reason`) | Logins recusados |
| `brasa.payments.pix_webhooks` | contador (`outcome`: accepted, duplicate, rejected) | Avisos de Pix recebidos |
| `brasa.payments.alerts` | contador (`kind`: PaidAfterExpiration, AmountMismatch) | Pagamentos que exigem atenção |
| `brasa.outbox.oldest_pending_age` | medidor, segundos | Idade do aviso de tempo real mais antigo não entregue |
| `brasa.printing.pending_jobs` | medidor | Tarefas de impressão esperando |
| `brasa.printing.oldest_pending_age` | medidor, segundos | Idade da tarefa pendente mais antiga |
| `brasa.printing.online_agents` | medidor | Agentes conectados |
| `brasa.payments.waiting_pix` | medidor | Pix online aguardando pagamento |

Os medidores são atualizados a cada 30 segundos.

## Alertas iniciais

Limiares de [infrastructure-and-cicd.md](../architecture/infrastructure-and-cicd.md#83-alertas-iniciais), a calibrar com o uso:

| Alerta | Regra |
|---|---|
| Indisponibilidade | Verificação externa de `https://<ADMIN_DOMAIN>/api/v1/health/ready` falha por 2 minutos |
| Erros | Respostas 5xx acima de 2% por 5 minutos (`http.server.request.duration` com `http.response.status_code` 5xx) |
| Outbox parado | `brasa.outbox.oldest_pending_age` acima de 60 por 2 minutos |
| Impressão parada | `brasa.printing.oldest_pending_age` acima de 120 com `brasa.printing.online_agents` maior que zero |
| Agente offline | `brasa.printing.online_agents` igual a zero no horário de funcionamento |
| Pagamentos | Qualquer aumento de `brasa.payments.alerts`; `brasa.payments.pix_webhooks{outcome="rejected"}` acima de 5 em 10 minutos |
| Força bruta | `brasa.auth.login_failures` acima de 30 em 5 minutos |
| Backup | Último arquivo em `BACKUP_DIR` com mais de 26 horas (verificação na máquina) |
| Disco | Uso acima de 80% (verificação na máquina) |

Teste cada alerta provocando a condição em homologação antes do piloto.
