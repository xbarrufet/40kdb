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
