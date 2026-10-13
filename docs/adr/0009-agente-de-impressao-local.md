# ADR-0009 — Agente de impressão local em .NET

- Status: **Proposto**
- Data: 2026-10-11

## Contexto

A cozinha usa impressora térmica com impressão automática. A impressora fica na rede local da loja, atrás de NAT; a API fica em ambiente remoto e não consegue abrir conexão com ela. No protótipo, a impressão depende de alguém acionar `window.print()` no navegador.

O modelo de impressora ainda não foi definido (pendência DP-02).

## Decisão

1. Um **agente de impressão** instalado em um computador da loja, implementado como **.NET Worker Service**, publicado como executável autossuficiente e registrado como serviço do sistema.
2. O agente faz **somente conexões de saída** (HTTPS e WebSocket seguro). Nenhuma porta é aberta na loja.
3. A fila de impressão é a tabela de tarefas no servidor. O agente **reserva** tarefas por HTTP; o aviso em tempo real apenas encurta a espera.
4. O agente mantém uma **fila local em SQLite**, gravando "imprimindo" antes de enviar à impressora.
5. A entrega é **"ao menos uma vez"**, com deduplicação pelo identificador da tarefa. Quando o resultado é incerto, a nova impressão sai marcada como possível 2ª via.
6. O servidor envia um **layout independente de impressora**; o agente o traduz para **ESC/POS** conforme o **perfil** da impressora.
7. O agente se identifica por **credencial de dispositivo** obtida em um pareamento com código de uso único.
8. O protocolo entre API e agente tem **versão própria**, com compatibilidade com a versão anterior.
9. Tarefas têm **limite de tentativas e validade**. Esgotados, viram alerta.

## Alternativas consideradas

| Alternativa | Avaliação |
|---|---|
| Impressão pelo navegador do painel em modo quiosque | Sem instalação, mas depende da aba aberta e não detecta falhas. Fica como **contingência**. |
| Acesso direto do navegador à impressora por USB ou serial | Restrito a alguns navegadores e dependente de permissão manual e da aba aberta. |
| Impressoras com impressão em nuvem do fabricante | Dispensam o agente, mas exigem modelos específicos. A fila no servidor é compatível; pode ser um transporte adicional no futuro. |
| Programa de terceiros para impressão a partir do navegador | Dependência externa e possível custo de licença, sem controle sobre novas tentativas. |
| Abrir uma porta na rede da loja para a API alcançar a impressora | Inviável na prática e inseguro. |
| Agente em outra tecnologia | Funcionaria, mas o .NET já é a tecnologia do backend e permite compartilhar o projeto de contratos. |

## Justificativa

- Impressão automática exige um processo que rode sem navegador e sem alguém clicando.
- Conexões só de saída funcionam em qualquer rede comum de loja, sem configuração de roteador.
- A reserva por consulta garante a entrega mesmo com avisos perdidos.
- A fila local e a gravação prévia do estado permitem distinguir "nunca tentou" de "talvez tenha impresso".
- Manter os detalhes da impressora no agente deixa o servidor independente de modelo.

## Consequências

**Positivas**

- A comanda sai sozinha, e falhas ficam visíveis no painel e no Admin.
- Queda de rede ou do agente não perde tarefas.
- Suporte a novos modelos é um perfil novo no agente.

**Negativas**

- Há um programa a instalar, atualizar e dar suporte em computadores de clientes.
- O ambiente da loja é imprevisível: antivírus, computador desligado, atualizações do sistema.
- "Exatamente uma vez" é impossível com impressora física; em falhas raras sai uma segunda via.
- Nem todo modelo informa falta de papel, então algumas falhas só são percebidas por pessoas.
- O agente fica instalado por muito tempo: mudanças de protocolo precisam ser compatíveis com versões antigas.

## Dependência em aberto

O protocolo final, a página de código, a detecção de falhas e o transporte dependem do modelo de impressora (DP-02). Até a definição, o desenvolvimento usa uma impressora virtual. Recomendação para o equipamento de referência: térmica de 80 mm, com guilhotina, ESC/POS e interface de rede.

## Quando rever

- A maioria das lojas usar modelos com impressão em nuvem.
- O custo de suporte ao agente superar o de uma alternativa.

## Relacionados

- [order-and-printing-flow.md](../architecture/order-and-printing-flow.md#8-agente-de-impressão)
- [security.md](../architecture/security.md#5-dispositivos-agente-de-impressão)
- [ADR-0007](0007-consistencia-outbox-e-fila-em-tabelas.md), [ADR-0011](0011-maquinas-de-estado-separadas.md)
