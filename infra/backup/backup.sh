#!/usr/bin/env bash
# Cópia lógica do banco e das imagens. Agende no cron da máquina, por exemplo:
#   15 3 * * * /opt/brasa/api/infra/backup/backup.sh >> /var/log/brasa-backup.log 2>&1
# Depois, envie a pasta para um armazenamento fora da máquina (rclone, aws s3 sync etc.).
set -euo pipefail

infra_dir="$(cd "$(dirname "$0")/.." && pwd)"
compose=(docker compose --env-file "$infra_dir/compose/.env" -f "$infra_dir/compose/compose.prod.yml")
set -a; source "$infra_dir/compose/.env"; set +a

backup_dir="${BACKUP_DIR:?Defina BACKUP_DIR no .env}"
stamp="$(date -u +%Y%m%dT%H%M%SZ)"
mkdir -p "$backup_dir"

"${compose[@]}" run --rm --no-deps --entrypoint pg_dump \
  -e PGHOST="$DB_HOST" -e PGPORT="${DB_PORT:-5432}" -e PGDATABASE="${DB_NAME:-brasa}" \
  -e PGUSER="${DB_USER:-brasa}" -e PGPASSWORD="$DB_PASSWORD" \
  postgres --format=custom --no-owner > "$backup_dir/brasa-$stamp.dump"

"${compose[@]}" run --rm --no-deps --entrypoint tar api -C /data -czf - images > "$backup_dir/brasa-images-$stamp.tar.gz"

find "$backup_dir" -name 'brasa-*' -mtime "+${BACKUP_RETENTION_DAYS:-30}" -delete
echo "Backup concluído em $backup_dir ($stamp)."
