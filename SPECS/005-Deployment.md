## Main goal
Deploy 40KDB to a home server with automated CI/CD and daily backups. The deployment should be systematic — pushing to `main` auto-deploys, and the database is backed up daily with easy restore.

## Architecture
```
git push main → GitHub Actions (self-hosted runner on server)
             → build Docker images → push to ghcr.io
             → docker compose pull && up -d
```

- **Backend:** .NET 9 container, port 5050 (internal)
- **Frontend:** Vue 3 + nginx container, port 4000 (exposed)
- **nginx** proxies `/api/*` to backend — only one port exposed
- **SQLite** volume-mounted at `~/40kdb/data/40kdb.db`

## Tech Stack
- Docker + Docker Compose
- GitHub Actions (self-hosted runner on home server)
- GitHub Container Registry (ghcr.io)
- nginx (reverse proxy + SPA routing)

## Files
```
backend/40kdb/Dockerfile          # Multi-stage .NET 9 build
backend/40kdb/appsettings.Production.json  # Production config
frontend/Dockerfile               # Build Vue + nginx
frontend/nginx.conf               # SPA fallback + /api proxy
frontend/.dockerignore
.dockerignore                     # Repo-root for backend build context
docker-compose.prod.yml           # Production orchestration
.github/workflows/deploy.yml      # CI/CD pipeline
scripts/backup.sh                 # SQLite backup (activity-aware)
scripts/restore.sh                # DB restore from backup
scripts/install-cron.sh           # Install daily cron job
DEPLOY.md                         # Server setup instructions
```

## Docker Compose (Production)
```yaml
services:
  backend:
    image: ghcr.io/xbarrufet/40kdb-backend:latest
    restart: unless-stopped
    volumes:
      - /home/xavier/40kdb/data:/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    expose:
      - "5050"

  frontend:
    image: ghcr.io/xbarrufet/40kdb-frontend:latest
    restart: unless-stopped
    ports:
      - "4000:80"
    depends_on:
      - backend
```

## GitHub Actions Workflow
- Runs on **self-hosted runner** (on the home server)
- Builds both images, pushes to ghcr.io
- Runs `docker compose pull && up -d` directly on the same machine
- No SSH needed — runner and deployment are on the same host

## nginx Config
- Serves Vue SPA with `try_files $uri $uri/ /index.html`
- Proxies `/api/*` to `http://backend:5050`
- Frontend uses relative API calls (`fetch('/api/...')`) — no CORS needed

## Backend Changes for Docker
- `Program.cs`: Connection string reads from `Configuration.GetConnectionString("DefaultConnection")` with fallback to `Data Source=40kdb.db`
- `Program.cs`: CORS origins configurable via `CorsOrigins` config section
- `appsettings.Production.json`: Uses `/data/40kdb.db` and `/seed-data` for seed files

## Server One-Time Setup
1. `mkdir -p ~/40kdb/data`
2. Copy `docker-compose.prod.yml` to server
3. Install self-hosted GitHub Actions runner
4. Runner registers with repo, starts as systemd service

## Backup Strategy
- **Tool:** `sqlite3 .backup` via `docker compose exec` — creates consistent snapshot without stopping the app
- **Storage:** `~/40kdb/backups/` on the server
- **Schedule:** Daily cron at 2:00 AM, but **only runs if DB was modified that day** (activity check)
- **Retention:** Last 5 backups kept
- **Restore:** Stops backend, swaps DB file, restarts. Pre-restore backup saved automatically.

### Backup Script Logic
1. Check if `~/40kdb/data/40kdb.db` was modified today
2. If no activity → skip
3. If activity → `docker compose exec backend sqlite3 /data/40kdb.db ".backup /data/40kdb.db.bak"`
4. Copy backup out of container to `~/40kdb/backups/40kdb_YYYYMMDD_HHMMSS.db`
5. Remove backups beyond the last 5

### Restore Script Logic
1. List available backups (if no argument given)
2. Stop backend container
3. Save current DB as pre-restore backup
4. Copy selected backup to `~/40kdb/data/40kdb.db`
5. Start backend container

## Access
```
http://server-ip:4000
```

## Manual Deploy
```bash
cd ~/40kdb
docker compose -f docker-compose.prod.yml pull
docker compose -f docker-compose.prod.yml up -d
```

## Manual Backup
```bash
~/40kdb/scripts/backup.sh
```

## Restore
```bash
~/40kdb/scripts/restore.sh                                          # list backups
~/40kdb/scripts/restore.sh ~/40kdb/backups/40kdb_20260624_143000.db  # restore
```
