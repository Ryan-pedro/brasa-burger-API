# Backup e restauração

Objetivos e regras em [infrastructure-and-cicd.md](../architecture/infrastructure-and-cicd.md#10-backup-e-recuperação). **Backup sem restauração testada não conta.**

## O que entra no backup

| Item | Como | Observação |
|---|---|---|
| Banco | `pg_dump` em formato próprio, uma vez por dia | Inclui as chaves de proteção de dados (sessões e tokens do agente). |
| Imagens dos produtos | `tar.gz` do volume `images` | |
| `.env` | Cópia manual em cofre de senhas | Nunca junto com os backups. |

Com banco gerenciado, mantenha também a recuperação pontual do provedor.

## Agendar

```bash
crontab -e
15 3 * * * /opt/brasa/api/infra/backup/backup.sh >> /var/log/brasa-backup.log 2>&1
```

O script grava em `BACKUP_DIR` e apaga cópias com mais de `BACKUP_RETENTION_DAYS` dias (30 por padrão). Envie a pasta para fora da máquina logo depois, por exemplo com `rclone sync` para um armazenamento de objetos com acesso restrito e cifragem.

## Restaurar

1. Pare a API: `docker compose --env-file infra/compose/.env -f infra/compose/compose.prod.yml stop api`.
2. Rode o script com os dois arquivos:

   ```bash
   infra/backup/restore.sh /var/backups/brasa/brasa-<data>.dump /var/backups/brasa/brasa-images-<data>.tar.gz
   ```

3. Suba a API e confira `/api/v1/health/ready`, o cardápio e os últimos pedidos.

## Teste trimestral

Restaure o backup mais recente em homologação, siga a lista acima e registre a data, o tempo gasto e qualquer problema encontrado.
