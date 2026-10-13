# ADR-0004 — Tenant resolvido pelo host e API na mesma origem

- Status: **Parcialmente substituído por [ADR-0016](0016-uma-unica-loja.md)** (2026-10-13). Deixou de valer a resolução da loja pelo endereço, porque existe uma loja só. Continuam valendo um endereço por interface e a API na mesma origem de cada uma.
- Data: 2026-10-11

## Contexto

Cada loja terá um endereço próprio, como `loja-a.dominio.com.br`. O backend precisa saber a qual loja uma requisição pública se refere, sem confiar em nada informado pelo navegador. As interfaces internas usam sessão em cookie, e as lojas ficam em subdomínios do mesmo domínio que o painel.

## Decisão

1. **Um host por interface:** `{slug}.dominio.com.br` para cada Storefront; `painel`, `cozinha` e `plataforma` para as interfaces internas; `agentes` para os agentes de impressão; `api` para webhooks.
2. **A API é publicada na mesma origem de cada interface.** O proxy de borda encaminha os prefixos `/api` e `/hubs` de cada host para o mesmo backend, e cada host só encaminha os prefixos da sua superfície.
3. **Na superfície pública, o tenant é deduzido do host**, repassado pelo proxy. A API só aceita esse cabeçalho do proxy confiável.
4. **Nas superfícies autenticadas, o tenant vem da sessão** (ou do token do agente), validado no servidor.
5. Nomes de sistema são slugs reservados e não podem ser usados por lojas.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| API em um único host, com o slug da loja no caminho | Funciona, mas exige CORS em todas as interfaces e faz o navegador informar a loja. |
| Identificador da loja em cabeçalho enviado pelo frontend | O cliente passa a ser a fonte do tenant, o que é exatamente o que se quer evitar. |
| Loja no caminho de um domínio único (`dominio.com.br/loja-a`) | Todas as lojas compartilhariam origem, armazenamento local e escopo de cookies; pior identidade para a loja. |
| Domínio próprio por loja desde o início | Emissão de certificados e verificação de posse por loja; fica como evolução. |

## Justificativa

- O navegador nunca escolhe o tenant: na loja pública ele é o host; nas interfaces internas, a sessão.
- Sem requisições entre origens, não há política de CORS a manter nem a errar.
- Cada interface tem cookie preso ao próprio host, com prefixo `__Host-`. Uma loja não consegue criar nem sobrescrever o cookie do painel.
- Rotas administrativas não são alcançáveis pelos hosts das lojas, o que reduz a superfície exposta.
- Subdomínios distintos dão a cada loja armazenamento local próprio no navegador.
- Domínio próprio por loja se encaixa depois sem redesenho: basta outro host apontando para a mesma loja.

## Consequências

**Positivas**

- Modelo de segurança simples de explicar e de testar.
- Nenhuma configuração de URL de API por ambiente no frontend.

**Negativas**

- Exige DNS curinga, certificado curinga e um proxy de borda com roteamento por host.
- O proxy passa a ser um componente crítico: um erro de roteamento expõe ou esconde uma superfície.
- Como as lojas e o painel pertencem ao mesmo site, `SameSite` sozinho não protege contra requisições forjadas; é necessário token antifalsificação e verificação de origem ([security.md](../architecture/security.md#82-csrf)).
- O perfil da loja é carregado depois do HTML, então pré-visualizações de link não mostram a marca da loja no MVP.

## Quando rever

- Se o provedor de hospedagem escolhido (DP-04) não suportar roteamento por host com domínio curinga.
- Se o risco de as lojas compartilharem o site com o painel se mostrar relevante. A evolução nesse caso é mover as lojas para um domínio registrável separado.

## Relacionados

- [architecture-overview.md](../architecture/architecture-overview.md#8-hosts-e-superfícies-da-api)
- [ADR-0003](0003-multi-tenancy-banco-compartilhado.md), [ADR-0010](0010-autenticacao-e-sessoes.md)
