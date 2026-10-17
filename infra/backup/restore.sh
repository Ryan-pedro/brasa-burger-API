#!/usr/bin/env bash
# Restaura uma cópia do banco e das imagens. Use primeiro em homologação.
#   infra/backup/restore.sh /var/backups/brasa/brasa-20261015T031500Z.dump /var/backups/brasa/brasa-images-20261015T031500Z.tar.gz
# Pare a API antes (docker compose ... stop api) e suba de novo ao final.
set -euo pipefail

database_dump="${1:?Informe o arquivo .dump do banco}"
images_archive="${2:?Informe o arquivo .tar.gz das imagens}"

infra_dir="$(cd "$(dirname "$0")/.." && pwd)"
compose=(docker compose --env-file "$infra_dir/compose/.env" -f "$infra_dir/compose/compose.prod.yml")
set -a; source "$infra_dir/compose/.env"; set +a

"${compose[@]}" run --rm --no-deps -T --entrypoint pg_restore \
  -e PGHOST="$DB_HOST" -e PGPORT="${DB_PORT:-5432}" -e PGDATABASE="${DB_NAME:-brasa}" \
  -e PGUSER="${DB_USER:-brasa}" -e PGPASSWORD="$DB_PASSWORD" \
  postgres --clean --if-exists --no-owner --dbname="${DB_NAME:-brasa}" < "$database_dump"

"${compose[@]}" run --rm --no-deps -T --user 0 --entrypoint sh api \
  -c 'rm -rf /data/images/* && tar -C /data -xzf - && chown -R "$APP_UID" /data/images' < "$images_archive"

echo "Restauração concluída. Confira com: curl https://\$ADMIN_DOMAIN/api/v1/health/ready"
