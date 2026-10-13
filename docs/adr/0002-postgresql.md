# ADR-0002 — PostgreSQL como banco de dados

- Status: **Aceito** (2026-10-11)
- Data: 2026-10-11

## Contexto

O sistema precisa de um banco relacional com transações, restrições de integridade e mecanismo de isolamento por linha. A stack inicialmente sugerida cita PostgreSQL e SQL Server como candidatos, e pede que a escolha seja justificada em vez de presumida.

O produto é um micro SaaS: o custo fixo mensal de infraestrutura pesa na viabilidade.

## Decisão

Usar **PostgreSQL** (versão 17 ou superior, conforme o provedor), acessado por EF Core com o provedor Npgsql.

## Alternativas consideradas

| Alternativa | Avaliação |
|---|---|
| **SQL Server** | Tecnicamente equivalente para este sistema. A edição gratuita tem limites de tamanho e de recursos; acima deles há licença ou serviço gerenciado pago. Melhor opção se a hospedagem for Azure com custo equivalente, ou se a familiaridade da equipe for determinante. |
| MySQL / MariaDB | Atende, mas sem vantagem sobre o PostgreSQL aqui e com isolamento por linha menos direto. |
| Banco de documentos | Os dados são relacionais e as garantias exigidas são transacionais. Inadequado como base principal. |
| SQLite | Sem concorrência de escrita adequada nem isolamento por linha. Usado apenas na fila local do agente de impressão. |

A comparação detalhada, por critério, está em [database-and-tenancy.md](../architecture/database-and-tenancy.md#2-postgresql-ou-sql-server).

## Justificativa

- **Custo e portabilidade:** sem licença em qualquer tamanho e disponível como serviço gerenciado em praticamente todos os provedores, inclusive opções de baixo custo.
- **Isolamento:** Row-Level Security por políticas, usada como camada de defesa em profundidade ([ADR-0003](0003-multi-tenancy-banco-compartilhado.md)).
- **Filas em tabelas:** suporte a reserva de linhas que ignora as já bloqueadas, base do outbox e da fila de impressão ([ADR-0007](0007-consistencia-outbox-e-fila-em-tabelas.md)).
- **Desenvolvimento local:** contêiner leve, igual em qualquer sistema operacional.
- Nenhum requisito favorece especificamente o SQL Server.

## Consequências

**Positivas**

- Nenhum custo de licença; liberdade para trocar de provedor.
- Recursos úteis sem custo adicional: `jsonb`, particionamento, identificadores ordenáveis por tempo nas versões recentes.

**Negativas**

- Diferenças em relação ao SQL Server exigem atenção de quem vem dele: comparação de texto sensível a maiúsculas por padrão, tratamento de data e hora com fuso, convenções de nomes.
- Em hospedagem própria, backup e recuperação pontual dependem de ferramenta externa; por isso a recomendação de banco gerenciado.

## Quando rever

- Se a hospedagem escolhida (DP-04) for Azure e o custo total com Azure SQL for menor ou igual.
- Se a curva de aprendizado atrasar a entrega de forma mensurável.

A revisão deve acontecer **até o fim da fase de fundações**. Trocar de banco é barato antes de haver dados de produção e caro depois.

## Relacionados

- [database-and-tenancy.md](../architecture/database-and-tenancy.md)
- Pendência DP-04 em [implementation-roadmap.md](../architecture/implementation-roadmap.md#6-decisões-pendentes)
