#!/bin/bash
# Install daily backup cron job for 40KDB
# Runs at 2:00 AM every day

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CRON_LINE="0 2 * * * $SCRIPT_DIR/backup.sh >> ~/40kdb/backups/cron.log 2>&1"

# Check if already installed
crontab -l 2>/dev/null | grep -q "backup.sh" && {
    echo "Cron job already installed."
    crontab -l | grep "backup.sh"
    exit 0
}

# Add to crontab
(crontab -l 2>/dev/null; echo "$CRON_LINE") | crontab -
echo "Cron job installed:"
echo "  $CRON_LINE"
echo ""
echo "Logs will be written to: ~/40kdb/backups/cron.log"
