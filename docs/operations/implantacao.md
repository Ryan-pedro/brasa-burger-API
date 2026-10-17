# Implantação

Roteiro para colocar a loja no ar em uma máquina com Docker Compose ([ADR-0014](../adr/0014-hospedagem-do-mvp.md)). Serve para homologação e produção; a hospedagem (DP-04) ainda não foi escolhida, então nada aqui depende de um provedor.

## O que a máquina precisa

- Linux com Docker e o plugin Compose.
- Portas 80 e 443 abertas para a internet.
- Dois nomes no DNS apontando para a máquina: um para a loja (`STORE_DOMAIN`) e um para o painel (`ADMIN_DOMAIN`). O Caddy emite os certificados sozinho.
- PostgreSQL 17. Prefira um banco gerenciado, com recuperação pontual. O perfil `banco-local` sobe o banco num contêiner, aceitável só enquanto não houver operação real.

## Primeira implantação

1. Clone este repositório em `/opt/brasa/api`.
2. Gere os builds do front-end (`npm run build` no `brasa-burger-front-end`) e copie `dist/storefront/browser` e `dist/admin/browser` para a máquina.
3. Copie `infra/compose/.env.example` para `infra/compose/.env` e preencha domínios, nome da loja, banco e as pastas do front-end. O `.env` nunca vai para o Git.
4. Atualize o banco e crie a loja:

   ```bash
   docker compose --env-file infra/compose/.env -f infra/compose/compose.prod.yml run --rm api migrate
   ```

5. Gere o convite do proprietário. O comando imprime o link; abra no navegador e crie a senha:

   ```bash
   docker compose --env-file infra/compose/.env -f infra/compose/compose.prod.yml run --rm api invite-owner dono@loja.com.br "Nome do dono"
   ```

6. Suba tudo:

   ```bash
   docker compose --env-file infra/compose/.env -f infra/compose/compose.prod.yml up -d --build
   ```

7. Confira `https://<ADMIN_DOMAIN>/api/v1/health/ready` (deve responder `Healthy`).
8. Agende o backup ([backup-e-restauracao.md](backup-e-restauracao.md)) e configure a verificação externa de saúde e os alertas ([monitoramento.md](monitoramento.md)).

Com o banco no contêiner, acrescente `--profile banco-local` aos comandos e use `DB_HOST=postgres`.

## Configurar a loja

Com o proprietário logado no painel:

1. Configurações da loja: tipos de pedido, taxa de entrega, pedido mínimo e aceite automático ou manual.
2. Equipe: convide gerentes e a cozinha. O link é enviado por WhatsApp ou outro canal; vale por 3 dias.
3. Cardápio: categorias, produtos, grupos de opções e ofertas.
4. Impressão: instale o agente (abaixo).
5. Abra a loja.

## Instalar o agente de impressão

1. No painel, em Impressão, gere um código de pareamento (vale 10 minutos).
2. No computador da loja, instale o agente e informe o endereço do painel e o código.
3. Cadastre a impressora do agente (largura do papel e endereço, por exemplo `tcp://192.168.0.50:9100`) e marque como padrão.
4. Imprima a página de teste.
5. Faça um pedido de teste na loja e confira a comanda impressa.

> O programa do agente ainda não existe. A API já atende o protocolo descrito no README.

## Pix online

Só depois de escolher o gateway (DP-03) e implementar o adaptador de `IPixGateway`:

1. Configure `Payments:Pix:Provider` e as credenciais do gateway por variável de ambiente.
2. No gateway, cadastre o aviso de pagamento em `https://<STORE_DOMAIN>/api/v1/webhooks/pix`.
3. Ligue o Pix online nas configurações da loja e faça um pedido real de valor baixo.

O gateway falso (`Fake`) recusa a inicialização fora de desenvolvimento e testes.

## Nova versão

```bash
git pull
docker compose --env-file infra/compose/.env -f infra/compose/compose.prod.yml build api
docker compose --env-file infra/compose/.env -f infra/compose/compose.prod.yml run --rm api migrate
docker compose --env-file infra/compose/.env -f infra/compose/compose.prod.yml up -d
```

As migrations precisam ser compatíveis com a versão anterior em execução ([infrastructure-and-cicd.md](../architecture/infrastructure-and-cicd.md#64-migrations-seguras)). A troca do contêiner causa alguns segundos de indisponibilidade.

## Antes do piloto

- [ ] Restauração testada em homologação ([backup-e-restauracao.md](backup-e-restauracao.md)).
- [ ] Alertas disparados em um teste provocado ([monitoramento.md](monitoramento.md)).
- [ ] Teste de carga em homologação (`infra/load/checkout.js`).
- [ ] Lista de verificação de [security.md](../architecture/security.md#14-verificações-de-segurança-antes-de-produção) conferida.
- [ ] Política de privacidade revisada por advogado e publicada (DP-09).
- [ ] Um turno completo com pedidos reais, sem intervenção técnica.
