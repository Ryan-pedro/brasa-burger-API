# ADR-0006 — Fatias verticais, CQRS lógico, sem MediatR e sem repositório genérico

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

A arquitetura do backend prevê camadas Domain, Application, Infrastructure e Api. É preciso decidir como os casos de uso são organizados dentro delas e quais padrões comuns do ecossistema .NET são adotados. O pedido explícito é evitar padrões introduzidos apenas por convenção.

## Decisão

| Padrão | Decisão |
|---|---|
| Organização | **Fatias verticais:** uma pasta por caso de uso, com requisição, manipulador, validador e resultado. |
| CQRS | **Apenas a separação lógica** entre comandos e consultas, sobre o mesmo banco e as mesmas tabelas. |
| MediatR | **Não adotar.** Os endpoints recebem os manipuladores por injeção de dependência. |
| Repositório genérico | **Não adotar.** A camada Application usa o EF Core por uma interface `IAppDbContext`. |
| Repositório específico | **Permitido** quando o carregamento de um agregado tem regra própria e se repete. |
| Eventos de domínio | **Adotar**, despachados em processo, dentro da transação. |
| Mapeador automático | **Não adotar.** Mapeamento e projeção explícitos. |
| Falhas de regra de negócio | **Resultado**, não exceção. |
| Event sourcing | **Não adotar.** |

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Serviços por entidade (`ProductService`, `OrderService`) | Tendem a crescer sem limite e a misturar casos de uso sem relação. |
| MediatR com comportamentos de pipeline | O que ele entregaria aqui é coberto por injeção direta e filtros de endpoint. É distribuído sob licença comercial desde 2025, com termos a conferir se a decisão for revista. |
| Repositório e unidade de trabalho sobre o EF Core | Duplicam o que o `DbContext` já é e escondem recursos úteis de consulta. |
| CQRS com modelo de leitura separado | Dobra a persistência e introduz consistência eventual sem requisito que peça. |
| Application sem nenhuma referência ao EF Core | Pureza de camadas ao custo de uma camada inteira de repasse. |

## Justificativa

- Fatias verticais mantêm cada mudança localizada: alterar um caso de uso toca uma pasta.
- Comandos e consultas têm necessidades diferentes (rastrear e validar contra projetar e devolver); separá-los logicamente custa pouco e simplifica os dois lados.
- Sem um intermediário de despacho, o caminho do endpoint ao manipulador é direto e navegável.
- O EF Core já implementa repositório e unidade de trabalho. Abstraí-lo por completo só se justificaria se trocar de ORM fosse um cenário real.
- Eventos de domínio em processo desacoplam módulos sem abrir mão da transação.

## Consequências

**Positivas**

- Menos indireção e menos dependências externas.
- Regras de negócio no domínio, testáveis sem banco.
- Consultas eficientes, projetadas direto para a resposta.

**Negativas**

- A camada Application depende das abstrações do EF Core. É uma concessão declarada.
- Comportamentos transversais (validação, idempotência, log) ficam em filtros de endpoint; se crescerem muito, o pipeline de um mediador faria falta.
- Sem repositórios, consultas parecidas podem se repetir em manipuladores diferentes.

## Regra complementar

Conferir a licença de toda biblioteca no momento da adoção. Em 2025, MediatR, AutoMapper, MassTransit e FluentAssertions passaram a ter licença comercial. Nenhuma é necessária nesta arquitetura.

## Quando rever

- Os filtros de endpoint não atenderem aos comportamentos transversais.
- A repetição de consultas justificar repositórios específicos em mais agregados.
- Um módulo precisar de modelo de leitura próprio por desempenho.

## Relacionados

- [backend-architecture.md](../architecture/backend-architecture.md#6-padrões-avaliados)
- [ADR-0001](0001-monolito-modular.md), [ADR-0007](0007-consistencia-outbox-e-fila-em-tabelas.md)
