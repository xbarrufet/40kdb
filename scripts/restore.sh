#!/bin/bash
# Restore 40KDB SQLite database from backup
# Usage: ./restore.sh [backup_file]
# If no file specified, lists available backups

BACKUP_DIR=~/40kdb/backups
COMPOSE_DIR=~/40kdb

if [ -z "$1" ]; then
    echo "Available backups:"
    echo ""
    ls -1t "$BACKUP_DIR"/40kdb_*.db 2>/dev/null | while read f; do
        SIZE=$(du -h "$f" | cut -f1)
        DATE=$(basename "$f" | sed 's/40kdb_//;s/\.db//;s/_/ /')
        echo "  $DATE  ($SIZE)  $f"
    done
    echo ""
    echo "Usage: ./restore.sh /path/to/backup.db"
    exit 0
fi

BACKUP_FILE="$1"

if [ ! -f "$BACKUP_FILE" ]; then
    echo "Backup file not found: $BACKUP_FILE"
    exit 1
fi

# Stop backend, swap DB, restart
echo "Stopping backend..."
docker compose -f ~/40kdb/docker-compose.prod.yml stop backend

echo "Backing up current DB..."
TIMESTAMP=$(date "+%Y%m%d_%H%M%S")
cp ~/40kdb/data/40kdb.db ~/40kdb/data/40kdb_pre_restore_${TIMESTAMP}.db 2>/dev/null

echo "Restoring from: $BACKUP_FILE"
cp "$BACKUP_FILE" ~/40kdb/data/40kdb.db

echo "Starting backend..."
docker compose -f ~/40kdb/docker-compose.prod.yml start backend

echo "Done! Database restored."
