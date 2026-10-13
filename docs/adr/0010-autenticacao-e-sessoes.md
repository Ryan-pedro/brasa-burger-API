# ADR-0010 — Identity com cookies para pessoas; credencial de dispositivo para agentes

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

O sistema tem atores com necessidades diferentes: consumidores, donos e gerentes, operadores de cozinha, operadores da plataforma, agentes de impressão e o gateway de pagamento. As interfaces são aplicações Angular; a API é .NET e, por decisão do [ADR-0004](0004-resolucao-de-tenant-por-host.md), é servida na mesma origem de cada interface.

O protótipo não tem autenticação.

## Decisão

| Ator | Mecanismo |
|---|---|
| Dono, gerente, cozinha, plataforma | **ASP.NET Core Identity** com **sessão em cookie**: `__Host-`, `HttpOnly`, `Secure`, `SameSite=Lax`, preso ao host da interface. |
| Plataforma | O mesmo, com **segundo fator obrigatório**. |
| Consumidor | **Sem conta no MVP.** Cada pedido tem um token de acompanhamento aleatório. |
| Agente de impressão | **Credencial de dispositivo** obtida por pareamento, trocada por **token de curta duração** com a loja e o agente. |
| Gateway de pagamento | **Assinatura do webhook**, mais consulta do estado real. |

Complementos:

- Proteção contra requisições forjadas por token antifalsificação em cabeçalho e verificação de origem.
- Usuário é global; o vínculo com cada loja e o papel nela ficam em uma entidade própria.
- A loja ativa fica na sessão e só muda por operação validada no servidor.
- Papéis de loja e papéis de plataforma são separados e não se herdam entre superfícies.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| JWT de acesso e renovação guardados no navegador | Tokens ao alcance de script injetado; revogação difícil. |
| Provedor de identidade externo | Custo por usuário e dependência externa; o modelo usuário–loja continua sendo nosso. Reavaliar se login social ou SSO virarem requisito. |
| Servidor OIDC próprio | Complexidade alta para quatro aplicações próprias, sem clientes de terceiros. |
| Um único cookie para todas as interfaces, no domínio pai | Uma loja em subdomínio poderia interferir; sessões de superfícies diferentes se misturariam. |
| Conta obrigatória para o consumidor | Atrito no checkout sem ganho no MVP. |
| Chave fixa de longa duração no agente | Sem expiração; a revogação dependeria só de apagar a chave. |

## Justificativa

- Com a API na mesma origem, o cookie de sessão é a opção mais segura e mais simples: nada sensível fica ao alcance do JavaScript.
- O prefixo `__Host-` impede que subdomínios de lojas criem ou sobrescrevam o cookie do painel.
- O Identity já resolve armazenamento de senhas, bloqueio, segundo fator e tokens de recuperação.
- Um cookie por host separa naturalmente painel, cozinha e plataforma.
- Tokens curtos no agente limitam o dano de uma credencial copiada e tornam a revogação efetiva em minutos.

## Consequências

**Positivas**

- Nenhum componente de identidade externo a operar ou pagar.
- Revogação de sessões simples.
- Superfícies isoladas por host.

**Negativas**

- É preciso implementar e testar a proteção contra requisições forjadas, porque lojas e painel pertencem ao mesmo site.
- Login social e SSO não vêm prontos.
- As chaves que cifram os cookies precisam ser persistidas fora do contêiner, ou as sessões caem a cada implantação.
- Sem conta, o consumidor não tem histórico de pedidos entre dispositivos.

## Quando rever

- Login social ou SSO virarem requisito.
- Surgirem clientes de terceiros consumindo a API.
- Funcionalidades do consumidor passarem a depender de identificação (pendência DP-05).

## Relacionados

- [security.md](../architecture/security.md)
- [ADR-0003](0003-multi-tenancy-banco-compartilhado.md), [ADR-0004](0004-resolucao-de-tenant-por-host.md)
