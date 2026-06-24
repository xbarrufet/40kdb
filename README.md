# 40KDB

A miniature database manager for Warhammer 40K — track your factions, units, paint status, and army collections.

## Tech Stack

- **Backend:** .NET 9 (C#) with Entity Framework Core + SQLite
- **Frontend:** Vue 3 + Vite + Tailwind CSS
- **Data:** Seed data from JSON files (factions, units)

## Project Structure

```
40Kdb/
├── backend/40kdb/    # .NET 9 Web API
├── frontend/         # Vue 3 + Vite + Tailwind
├── data/             # JSON seed data (factions, units)
├── SPECS/            # Feature specifications
└── run.sh            # Start/stop script
```

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18+)

### Quick Start

```bash
./run.sh start
```

This starts both services:

| Service  | URL                     |
|----------|-------------------------|
| Backend  | http://localhost:5050   |
| Frontend | http://localhost:4000   |

### Manual Start

**Backend:**

```bash
cd backend/40kdb
dotnet run --urls "http://localhost:5050"
```

**Frontend:**

```bash
cd frontend
npm install
npm run dev
```

### Other Commands

```bash
./run.sh stop      # Stop all services
./run.sh restart   # Restart all services
./run.sh status    # Check running status
```

## API

The backend exposes REST endpoints:

- `GET /health` — Health check
- `/factions` — Faction CRUD
- `/units` — Unit CRUD
- `/collections` — Collection management
- `/projects` — Project tracking
- `/games` — Game records

## Data

Seed data is loaded automatically from `data/` on first run. Faction and unit JSON files are imported into the SQLite database (`40kdb.db`).

## License

Private project.
