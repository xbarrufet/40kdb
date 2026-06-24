# Deploying 40KDB to your home server

## Prerequisites

- Docker + Docker Compose installed on the server
- SSH access to the server
- A GitHub account with the repo

## One-Time Server Setup

### 1. Create the data directory

```bash
mkdir -p ~/40kdb/data
```

### 2. Copy docker-compose to the server

```bash
scp docker-compose.prod.yml xavier@192.168.1.100:~/40kdb/
```

### 3. Install the self-hosted GitHub Actions runner

Go to GitHub → your repo → **Settings → Actions → Runners → New self-hosted runner**.

Run the commands it shows on your server:

```bash
mkdir -p ~/actions-runner && cd ~/actions-runner
curl -o actions-runner-linux-x64.tar.gz -L https://github.com/actions/runner/releases/download/v2.321.0/actions-runner-linux-x64-2.321.0.tar.gz
tar xzf actions-runner-linux-x64.tar.gz

# Replace with the config command GitHub gives you
./config.sh --url https://github.com/xbarrufet/40kdb --token XXXXXXX

# Install and start as a service
sudo ./svc.sh install
sudo ./svc.sh start
```

## How It Works

1. Push to `main` → self-hosted runner on your server picks up the job
2. Builds Docker images, pushes to `ghcr.io`
3. Runs `docker compose pull && docker compose up -d` on the same machine
4. Old images are pruned automatically

## Accessing the App

```
http://your-server-ip:4000
```

## Manual Deploy

If you ever need to deploy manually on the server:

```bash
cd ~/40kdb
docker compose -f docker-compose.prod.yml pull
docker compose -f docker-compose.prod.yml up -d
```

## Backups

### Setup

```bash
scp -r scripts/ xavier@192.168.1.100:~/40kdb/
ssh xavier@192.168.1.100
chmod +x ~/40kdb/scripts/*.sh
~/40kdb/scripts/install-cron.sh
```

Backups run daily at 2:00 AM, but only if the DB was modified that day. The last 5 backups are kept in `~/40kdb/backups/`.

### Manual Backup

```bash
~/40kdb/scripts/backup.sh
```

### Restore

```bash
~/40kdb/scripts/restore.sh          # list available backups
~/40kdb/scripts/restore.sh /path/to/40kdb_20260624_143000.db
```
