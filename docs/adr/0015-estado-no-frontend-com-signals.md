# ADR-0015 — Estado no frontend com signals, sem biblioteca de estado global

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

**Fatos:** o protótipo usa Angular 21 com signals, `computed` e `effect` em serviços (`CartService`, `CheckoutService`, `OrderService`), detecção de mudanças OnPush e nenhuma biblioteca de estado. O carrinho é sincronizado com `localStorage`.

As quatro aplicações têm complexidades de estado diferentes: o Storefront tem um carrinho e um fluxo linear; o Admin é majoritariamente cadastro; o Kitchen Display tem um quadro atualizado em tempo real; o Platform Admin é cadastro.

## Decisão

1. O estado é gerenciado com **signals em serviços**, sem biblioteca de estado global.
2. Cada funcionalidade tem o seu **store** (um serviço com signals), localizado na pasta da funcionalidade. O estado não é global por padrão.
3. Estado de sessão da aplicação (loja, usuário, permissões) fica em serviços de `core/`.
4. Toda leitura do servidor expõe **situação explícita**: inicial, carregando, sucesso, erro.
5. Estado em tempo real segue a regra do [ADR-0008](0008-tempo-real-com-signalr.md): o aviso invalida, a consulta atualiza.
6. Nenhum valor monetário autoritativo é calculado no frontend.
7. Componentes não acessam a API diretamente; passam pelo store da funcionalidade.

## Alternativas consideradas

| Alternativa | Por que não agora |
|---|---|
| Store global no estilo Redux | Cerimônia alta (ações, redutores, efeitos) para estados que são, em sua maioria, listas e formulários. |
| Store baseada em signals de uma biblioteca | Boa opção e próxima do que já existe. Adiciona dependência sem necessidade comprovada; é a primeira candidata se a complexidade crescer. |
| Biblioteca de cache de dados do servidor | Resolve cache e invalidação, mas com mais uma abstração. Reavaliar se a repetição de código de carregamento incomodar. |
| Estado só em componentes | Não atende estado compartilhado entre telas, como carrinho e sessão. |
| A mesma solução nas quatro aplicações, escolhida pela mais complexa | Imporia ao Storefront e aos cadastros o custo exigido só pelo painel da cozinha. |

## Justificativa

- É o padrão que o protótipo já usa, com bons resultados, e é o caminho recomendado pelo próprio framework.
- A complexidade atual não justifica uma biblioteca.
- Stores por funcionalidade mantêm o estado próximo de quem o usa e facilitam os testes.
- A decisão é local a cada aplicação: adotar uma biblioteca em uma delas no futuro não afeta as outras.

## Consequências

**Positivas**

- Nenhuma dependência nova; menos código.
- Curva de aprendizado mínima para quem já conhece o protótipo.
- Stores simples de testar com Vitest.

**Negativas**

- Sem convenções impostas por biblioteca, a consistência entre stores depende de disciplina e de revisão.
- Sem ferramentas de inspeção de estado prontas.
- Padrões como atualização otimista com reversão precisam ser escritos à mão, principalmente no Kitchen Display.

## Quando rever

- Duas ou mais funcionalidades precisarem coordenar atualizações otimistas sobre as mesmas entidades.
- A depuração de estado ficar difícil.
- O código de carregamento e de erro se repetir a ponto de pedir uma abstração.

Nesses casos, adotar uma store estruturada **na aplicação afetada**, não nas quatro.

## Relacionados

- [frontend-architecture.md](../architecture/frontend-architecture.md#43-gerenciamento-de-estado)
- [ADR-0008](0008-tempo-real-com-signalr.md)
