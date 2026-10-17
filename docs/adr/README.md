# Registros de decisão de arquitetura (ADR)

Cada arquivo registra **uma** decisão: o contexto, o que foi decidido, as alternativas consideradas, a justificativa e as consequências.

## Situações possíveis

| Status | Significado |
|---|---|
| **Proposto** | Recomendação aguardando revisão. Ainda não é definitiva. |
| **Aceito** | Revisado e adotado. |
| **Rejeitado** | Revisado e descartado; fica no histórico com o motivo. |
| **Substituído por ADR-nnnn** | Trocado por uma decisão posterior. |

Os registros marcados como **Proposto** ainda não foram validados por quem vai implementar.

## Regras

- Um ADR aceito não é editado para mudar a decisão. Cria-se um novo, e o antigo passa a "Substituído".
- A numeração é sequencial e nunca é reutilizada.
- Uma mudança que contraria um ADR aceito exige um novo ADR no mesmo pull request.
- Questões ainda sem resposta não são ADRs: ficam no registro de pendências em [implementation-roadmap.md](../architecture/implementation-roadmap.md#6-decisões-pendentes).

## Índice

| # | Decisão | Status |
|---|---|---|
| [0001](0001-monolito-modular.md) | Monólito modular em vez de microsserviços | Proposto |
| [0002](0002-postgresql.md) | PostgreSQL como banco de dados | Aceito |
| [0003](0003-multi-tenancy-banco-compartilhado.md) | Multi-tenancy em banco compartilhado, com quatro camadas de isolamento | Substituído por 0016 |
| [0004](0004-resolucao-de-tenant-por-host.md) | Tenant resolvido pelo host e API na mesma origem | Parcialmente substituído por 0016 |
| [0005](0005-estrutura-do-monorepo.md) | Estrutura do monorepo e workspace Angular multi-projeto | Aceito com ajuste (npm); parcialmente substituído por 0017 |
| [0006](0006-organizacao-da-aplicacao.md) | Fatias verticais, CQRS lógico, sem MediatR e sem repositório genérico | Proposto |
| [0007](0007-consistencia-outbox-e-fila-em-tabelas.md) | Transação única entre módulos, Outbox para efeitos externos e filas em tabelas | Proposto |
| [0008](0008-tempo-real-com-signalr.md) | Tempo real com SignalR: aviso por push, verdade por consulta | Proposto |
| [0009](0009-agente-de-impressao-local.md) | Agente de impressão local em .NET | Proposto |
| [0010](0010-autenticacao-e-sessoes.md) | Identity com cookies para pessoas; credencial de dispositivo para agentes | Proposto |
| [0011](0011-maquinas-de-estado-separadas.md) | Máquinas de estado separadas para pedido, pagamento, comanda e impressão | Proposto |
| [0012](0012-snapshot-de-pedido-e-dinheiro.md) | Cópia imutável no pedido e representação de dinheiro | Proposto |
| [0013](0013-contratos-openapi-por-superficie.md) | Contratos OpenAPI por superfície, versionados no repositório | Proposto |
| [0014](0014-hospedagem-do-mvp.md) | Hospedagem do MVP em uma máquina com Docker Compose | Proposto |
| [0015](0015-estado-no-frontend-com-signals.md) | Estado no frontend com signals, sem biblioteca de estado global | Proposto |
| [0016](0016-uma-unica-loja.md) | Uma única loja por instalação | Aceito |
| [0017](0017-front-end-em-repositorio-proprio.md) | Front-end em repositório próprio | Aceito |
| [0018](0018-imagens-em-disco-local.md) | Imagens do cardápio em disco local no MVP | Proposto |
| [0019](0019-pix-online-opcional-atras-de-uma-porta.md) | Pix online opcional, atrás de uma porta para o gateway | Aceito |
| [0020](0020-observabilidade-com-opentelemetry.md) | Observabilidade com OpenTelemetry e exportação OTLP | Aceito |

## Modelo

```
# ADR-nnnn — Título

- Status: Proposto | Aceito | Rejeitado | Substituído por ADR-nnnn
- Data: AAAA-MM-DD

## Contexto
## Decisão
## Alternativas consideradas
## Justificativa
## Consequências
## Quando rever
## Relacionados
```
