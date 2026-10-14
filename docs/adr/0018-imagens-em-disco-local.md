# ADR-0018 — Imagens do cardápio em disco local no MVP

- Status: **Proposto**
- Data: 2026-10-14

## Contexto

[infrastructure-and-cicd.md](../architecture/infrastructure-and-cicd.md#7-armazenamento-de-imagens) prevê imagens em armazenamento de objetos, entregues por um host separado, com recodificação e variações de tamanho. O provedor de hospedagem ainda não foi escolhido (DP-04), e a stack do projeto não tem biblioteca de imagens.

## Decisão

1. No MVP, a API grava as imagens enviadas pelo painel numa pasta local (`Storage:Images:Path`, padrão `storage/images`) e as serve em `/api/v1/media/{arquivo}`, com cache longo e `nosniff`.
2. O tipo é verificado pelo conteúdo (assinatura de JPEG, PNG e WebP), não pela extensão nem pelo `Content-Type`. SVG e qualquer outro formato são recusados. Limite de 2 MB.
3. O nome do arquivo é gerado pelo servidor e muda a cada envio.
4. O acesso ao armazenamento passa pela porta `IImageStorage`, para trocar por armazenamento de objetos sem mudar os casos de uso.

## Alternativas consideradas

| Alternativa | Por que não agora |
|---|---|
| Armazenamento de objetos desde já | Depende do provedor (DP-04) e de um serviço a mais no ambiente local. |
| Recodificar com uma biblioteca de imagens | Adiciona dependência e escolha de licença; a verificação por assinatura cobre o risco principal (script disfarçado de imagem). |

## Consequências

- **Sem recodificação:** metadados (como localização da câmera) permanecem na imagem. Aceitável porque só a equipe envia imagens.
- A pasta precisa de volume persistente e de backup quando houver produção.
- Imagens sem uso não são removidas.

## Quando rever

- Na escolha do provedor (DP-04) ou antes da fase 6: mover para armazenamento de objetos e adicionar recodificação e variações.
