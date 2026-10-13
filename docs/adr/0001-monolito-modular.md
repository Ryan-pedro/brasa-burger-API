# ADR-0001 — Monólito modular em vez de microsserviços

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

O sistema tem vários domínios de negócio (lojas, identidade, catálogo, pedidos, pagamentos, cozinha, impressão, assinaturas) e será desenvolvido e operado por uma equipe muito pequena. O repositório hoje contém apenas um protótipo de frontend; não há backend.

O fluxo central exige consistência: um pedido confirmado precisa ter comanda e tarefa de impressão.

## Decisão

O backend é **um único processo implantável**, em ASP.NET Core, organizado em **módulos de negócio com fronteiras verificadas por testes de arquitetura**, sobre **um único banco de dados**.

Os módulos são pastas e namespaces dentro de quatro projetos de camada (Domain, Application, Infrastructure, Api), não um conjunto de projetos por módulo.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Microsserviços por domínio | Exigem consistência distribuída, mensageria, rastreamento e vários deploys. Nenhum requisito atual justifica esse custo. |
| Monólito sem fronteiras internas | Mais rápido no início, mas os domínios se entrelaçam e qualquer extração futura vira reescrita. |
| Um projeto .NET por módulo e por camada | Mais de trinta projetos para um sistema deste tamanho; custo de build e de navegação desproporcional. |

## Justificativa

- Não há equipes independentes, cargas com perfis de escala diferentes nem necessidade de isolar falhas entre partes do backend.
- Com um banco só, a consistência entre pedido, comanda e impressão é uma transação local.
- Um deploy, um log e um banco cabem na capacidade de operação de uma pessoa.
- Fronteiras verificadas mantêm aberta a opção de extrair um módulo depois.

## Consequências

**Positivas**

- Desenvolvimento e depuração simples; uma única implantação.
- Transações entre módulos sem coordenação distribuída.
- Custo de infraestrutura mínimo.

**Negativas**

- Todos os módulos escalam juntos.
- Uma falha grave em um módulo pode derrubar o processo inteiro.
- As fronteiras dependem de disciplina e de testes; sem eles, degradam.

## Quando rever

- Um módulo passar a exigir escala muito diferente dos demais.
- Uma segunda equipe assumir a responsabilidade por um módulo.
- O processamento assíncrono degradar a latência da API de pedidos. Nesse caso, o primeiro passo é separar o **processo** de trabalho assíncrono, mantendo o banco.

## Relacionados

- [backend-architecture.md](../architecture/backend-architecture.md)
- [ADR-0006](0006-organizacao-da-aplicacao.md), [ADR-0007](0007-consistencia-outbox-e-fila-em-tabelas.md)
