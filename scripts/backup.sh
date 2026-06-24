#!/bin/bash
# Backup 40KDB SQLite database
# Usage: ./backup.sh

BACKUP_DIR=~/40kdb/backups
COMPOSE_DIR=~/40kdb
DB_PATH="/data/40kdb.db"
KEEP=5

mkdir -p "$BACKUP_DIR"

# Check if DB was modified today (activity check)
DB_FILE=~/40kdb/data/40kdb.db
if [ -f "$DB_FILE" ]; then
    DB_MOD_DATE=$(stat -f "%Sm" -t "%Y-%m-%d" "$DB_FILE")
    TODAY=$(date "+%Y-%m-%d")
    if [ "$DB_MOD_DATE" != "$TODAY" ]; then
        echo "No activity today, skipping backup."
        exit 0
    fi
else
    echo "Database file not found!"
    exit 1
fi

# Create consistent backup via sqlite3 .backup
TIMESTAMP=$(date "+%Y%m%d_%H%M%S")
BACKUP_FILE="$BACKUP_DIR/40kdb_${TIMESTAMP}.db"

cd "$COMPOSE_DIR"
docker compose -f docker-compose.prod.yml exec -T backend sqlite3 "$DB_PATH" ".backup '$DB_PATH.bak'" 2>/dev/null
docker compose -f docker-compose.prod.yml cp "backend:$DB_PATH.bak" "$BACKUP_FILE"
docker compose -f docker-compose.prod.yml exec -T backend rm -f "$DB_PATH.bak"

if [ -f "$BACKUP_FILE" ]; then
    SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    echo "Backup created: $BACKUP_FILE ($SIZE)"
else
    echo "Backup failed!"
    exit 1
fi

# Remove old backups, keep last KEEP
cd "$BACKUP_DIR"
ls -1t 40kdb_*.db 2>/dev/null | tail -n +$((KEEP + 1)) | xargs rm -f 2>/dev/null
REMAINING=$(ls -1 40kdb_*.db 2>/dev/null | wc -l)
echo "Backups on disk: $REMAINING/$KEEP"
