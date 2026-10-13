# ADR-0012 — Cópia imutável no pedido e representação de dinheiro

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

**Fatos sobre o protótipo:**

- O pedido guarda apenas identificadores de produto, de adicionais e de opção. Nome e preço são buscados no cardápio a cada exibição.
- Preços e totais são números de ponto flutuante (`24.9`).
- O total é calculado no navegador.

No sistema real, o cardápio muda o tempo todo: preços, nomes, opções e ofertas. Pedidos antigos precisam continuar mostrando exatamente o que foi vendido e por quanto.

## Decisão

**Sobre o histórico**

1. No momento da criação, o pedido grava uma **cópia** do nome e do preço de cada produto, do nome e do preço de cada opção escolhida, de cada desconto aplicado (rótulo e valor), das taxas e dos dados do cliente.
2. Essa cópia é **imutável**. Itens e valores de um pedido não mudam depois de criado.
3. O total é um **valor gravado**, não recalculado na leitura.
4. O item mantém uma referência ao produto original **apenas para relatórios**. Nenhuma tela de pedido lê nome ou preço pelo produto.
5. Produtos, categorias e opções são **arquivados**, não apagados.
6. A comanda guarda a própria cópia do conteúdo, como a cozinha o viu.

**Sobre o dinheiro**

7. No domínio e no banco, valores monetários usam tipo **decimal** com duas casas. Nunca ponto flutuante.
8. Nos contratos da API, valores trafegam como **inteiros em centavos**.
9. A moeda é BRL. O campo de moeda existe no pedido, para não impedir evolução.
10. O arredondamento acontece em pontos definidos do cálculo (por linha e no total), documentados e testados.
11. **Todo valor é calculado no servidor.** O cliente envia apenas o total que espera, para detectar divergência.

## Alternativas consideradas

| Alternativa | Por que não |
|---|---|
| Pedido referenciando o produto, sem cópia | Qualquer alteração no cardápio muda pedidos antigos. |
| Versionar o cardápio e apontar o pedido para a versão | Correto, porém muito mais complexo: toda edição cria versão, e toda leitura de pedido resolve versões. |
| Ponto flutuante com arredondamento na exibição | Erros acumulam em somas e percentuais e aparecem como centavos de diferença. |
| Valores decimais como texto ou número fracionário na API | Deixa a interpretação a cargo de cada cliente, com risco de voltar ao ponto flutuante no JavaScript. |
| Total recalculado na leitura | Uma mudança futura na regra de cálculo reescreveria o passado. |

## Justificativa

- Um pedido é um registro do que aconteceu; ele precisa ser estável independentemente do cardápio.
- A cópia é simples de implementar, de consultar e de auditar.
- Inteiros em centavos são exatos em qualquer linguagem, inclusive no navegador.
- Calcular no servidor é a única forma de impedir adulteração de preço.

## Consequências

**Positivas**

- Histórico de pedidos sempre correto.
- Leitura de pedidos sem junções com o catálogo.
- Sem erros de arredondamento.

**Negativas**

- Duplicação de dados entre catálogo e pedidos. É intencional.
- O frontend precisa converter centavos para exibição, e o pipe de moeda do protótipo, que recebe reais em ponto flutuante, precisa ser ajustado.
- Corrigir um pedido, no MVP, é cancelar e refazer.
- A cópia inclui dados pessoais, que precisam de anonimização ao fim do prazo de retenção.

## Quando rever

- Alterar pedidos depois de confirmados virar requisito.
- O produto passar a operar em mais de uma moeda.

## Relacionados

- [database-and-tenancy.md](../architecture/database-and-tenancy.md#7-preservação-do-histórico-dos-pedidos)
- [order-and-printing-flow.md](../architecture/order-and-printing-flow.md#4-criação-do-pedido-e-idempotência)
