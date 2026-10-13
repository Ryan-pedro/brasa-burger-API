# ADR-0014 — Hospedagem do MVP em uma máquina com Docker Compose

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

**Fato:** não existe nenhuma infraestrutura definida no repositório.

O sistema precisa de: uma API com conexões WebSocket de longa duração, roteamento por host com domínio curinga, um banco PostgreSQL, armazenamento de imagens e arquivos estáticos de quatro aplicações. A carga esperada no início é pequena, e a operação será feita por uma equipe muito pequena, com orçamento restrito.

O provedor e a região não estão definidos (pendência DP-04).

## Decisão

1. Homologação e produção rodam, cada uma, em **uma máquina virtual com Docker Compose**.
2. Nessa máquina ficam o **proxy de borda** (TLS, roteamento por host, arquivos estáticos, cabeçalhos de segurança) e o **contêiner da API**, que também executa os serviços em segundo plano.
3. O **banco é gerenciado**, fora da máquina, se o orçamento permitir. Banco em contêiner é aceitável apenas enquanto não houver lojas pagantes, com backup para fora da máquina e restauração testada.
4. As **imagens** ficam em armazenamento de objetos compatível com S3.
5. **Logs, métricas e alertas** vão para um serviço externo à máquina.
6. **Sem Kubernetes.**
7. A arquitetura **não depende de serviço proprietário** de nenhum provedor: contêineres, PostgreSQL e armazenamento compatível com S3.

## Alternativas consideradas

| Alternativa | Avaliação |
|---|---|
| Plataforma de contêineres gerenciada | Menos operação. Custo maior; suporte a domínio curinga e a conexões longas varia; nem todas têm região no Brasil. Boa segunda opção. |
| Serviços gerenciados de um grande provedor | Mais robusto, com região em São Paulo. Custo e curva de aprendizado maiores do que o estágio do produto pede. |
| Kubernetes | Complexidade operacional incompatível com a equipe e com a carga. |
| Funções sem servidor | Inadequadas para conexões de longa duração e serviços em segundo plano contínuos. |
| Aplicações estáticas em um serviço de hospedagem próprio e API em outro lugar | Viável, mas quebraria o modelo de API na mesma origem ([ADR-0004](0004-resolucao-de-tenant-por-host.md)) ou exigiria configurar o roteamento nesse serviço. |

## Justificativa

- É a opção de menor custo e a mais simples de entender e de depurar.
- Dá controle total sobre WebSocket, domínio curinga e roteamento por host, que são os requisitos menos uniformes entre plataformas.
- O que é insubstituível (o banco) fica fora da máquina; todo o resto pode ser recriado a partir do repositório.
- A portabilidade deixa a escolha de provedor reversível.

## Consequências

**Positivas**

- Custo mensal baixo e previsível.
- Ambiente local, homologação e produção com a mesma forma.
- Sem dependência de provedor.

**Negativas**

- A máquina é ponto único de falha. Uma queda derruba todas as lojas.
- Atualizações do sistema operacional, do Docker e do proxy são responsabilidade nossa.
- Implantações podem ter segundos de indisponibilidade.
- Escalar horizontalmente exigirá mudanças: balanceador, backplane, limitador de taxa compartilhado.

## Requisitos para a escolha do provedor (DP-04)

- Região em São Paulo ou próxima.
- Suporte a WebSocket e a domínio curinga.
- PostgreSQL gerenciado com recuperação pontual.
- Armazenamento de objetos.
- Custo mensal previsível.

Os preços devem ser levantados na data da contratação.

## Quando rever

- For necessária alta disponibilidade.
- A carga exigir mais de uma instância.
- O tempo gasto com operação da máquina superar o custo de uma plataforma gerenciada.

## Relacionados

- [infrastructure-and-cicd.md](../architecture/infrastructure-and-cicd.md#5-hospedagem)
- [ADR-0002](0002-postgresql.md), [ADR-0008](0008-tempo-real-com-signalr.md)
