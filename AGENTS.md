# 40KDB

## What is this
40KDB is a miniature database manager for Warhammer 40K. Track factions, units, paint status, and army collections.

Functional specs are in `/specs`. Each spec is numbered and covers a feature area.

## Tech Stack
- **Backend:** .NET 9 (C#) with Entity Framework Core + SQLite
- **Frontend:** Vue 3 (Composition API, `<script setup>`) + Vite + Tailwind CSS
- **DB:** SQLite via `40kdb.db`, auto-migrated on startup
- **Deploy:** Docker Compose on self-hosted runner, images pushed to GHCR

## Architecture

```
40Kdb/
├── backend/40kdb/           # ASP.NET Core Web API
│   ├── Controllers/         # REST controllers (api/[controller])
│   ├── Models/              # EF Core entities
│   ├── Data/                # AppDbContext + seed logic
│   └── Migrations/          # EF Core migrations
├── frontend/                # Vue 3 + Vite
│   └── src/
│       ├── views/           # Page-level components (one per route)
│       ├── components/      # Shared components (Sidebar, modals, etc.)
│       └── router/          # Vue Router (history mode)
├── data/                    # JSON seed data (factions.json, units_*.json)
└── specs/                   # Feature specifications
```

## Backend Patterns
- Controllers: `[ApiController]` + `[Route("api/[controller]")]`, inject `AppDbContext` directly (no service layer)
- Responses: anonymous objects in `Ok(new { ... })`, no DTO classes
- Request DTOs: `public class` or `public record` at bottom of controller file
- Stats: inline `.Count(m => m.State == MiniatureState.X)` in LINQ projections
- Enums: `MiniatureState { Sprue, Built, Primed, Painted }`, serialized as strings
- All mutations set `UpdatedAt = DateTime.UtcNow`
- Completion = `State == Painted && DecalsApplied == true`

## Frontend Patterns
- Views: Vue 3 `<script setup>`, `ref()` for state, `onMounted()` for data fetch
- API calls: raw `fetch('/api/...')` inline, no shared API layer
- Styling: Tailwind, dark theme (`gray-800/900` bg, `amber-400/500` accent)
- Routing: Vue Router history mode, routes defined in `src/router/index.js`
- No state management (no Pinia/Vuex)
- Modals emit `close` and `saved` events
- Toast notifications via `provide/inject`

## Data Model
```
Game → Faction → Unit → Miniature
Project → Phase → ProjectMiniature → Miniature
```

## Specs Progress
- 001 - Infrastructure
- 002 - Sidebar and Factions
- 003 - Collections
- 004 - Projects
- 005 - Deployment
- 006 - HomePage

## Rules
- Ask one question at a time until knowledge is aligned
- Apply grill-me skill in any analysis
- When editing, follow existing code conventions (no new patterns)
- Always run lint/typecheck after changes if available
- Never commit without explicit user request
