## Main goal
The Home page is the landing screen of the application. It provides quick access to ongoing work (recent projects) and a high-level overview of the entire miniature collection with stats and progress indicators.

## Layout

```
+------------------------------------------------------+
|  ACTIVE PROJECTS                                     |
|  [ Project Card 1 ]  [ Project Card 2 ]             |
+------------------------------------------------------+
|  COLLECTION STATS                                    |
|  [ Game: All ▾ ]                                     |
|                                                      |
|  +--------------------+  +------------------------+  |
|  |   127 / 350        |  |  State Bar (stacked)   |  |
|  |   36% complete     |  |  [red][org][blu][grn]  |  |
|  |                    |  |  Base: 45% | Mag: 12%  |  |
|  |                    |  |  Decals: 30%            |  |
|  +--------------------+  +------------------------+  |
|                                                      |
|  (if game selected)                                  |
|  +--------------------------------------------------+|
|  | FACTION BREAKDOWN                                ||
|  | [ Faction Name  | Total | Sprue | Built | ...  ] ||
|  | [ Faction Name  | Total | Sprue | Built | ...  ] ||
|  +--------------------------------------------------+|
+------------------------------------------------------+
```

## Section 1: Active Projects

### Purpose
Show the last two most recently updated projects so the user can jump back into ongoing work.

### Behavior
- Fetch the 2 most recently updated projects (sorted by `UpdatedAt` descending).
- Each project is displayed as a **card**.
- If fewer than 2 projects exist, show only the available ones.
- If no projects exist, hide this section entirely.

### Project Card Contents
- **Project Name**
- **Game name**
- **Number of minis** in the project (e.g. "12 minis" or "0 minis" for new projects)
- **% Completed** (calculated as per spec 004: `Painted && DecalsApplied / Total × 100`)
- **Last updated** relative time (e.g. "2 hours ago", "yesterday")

### Interaction
- Clicking the card navigates to the **Project Detail Page** (spec 004).
- No inline editing from Home.

### Edge Cases
- Projects with 0 miniatures still show as cards (user can click to start adding).
- No visual distinction for stale projects (even if last updated months ago).

## Section 2: Collection Stats

### Game Filter Dropdown
- A dropdown at the top of the stats section.
- Options: `All` (default), plus one entry per game in the system.
- Selecting a game filters all stats below to that game's miniatures only.
- Changing the dropdown triggers a re-fetch of stats.

### Stats Display

#### Zero Data State
- When total miniatures is 0 (globally or for a selected game), show a **"No data"** message instead of the stats layout.
- The game filter dropdown remains visible and functional.

#### Left Side: Summary Numbers
- **Big number**: `<Completed> / <Total>` where:
  - `Total` = total miniatures matching the current filter
  - `Completed` = miniatures where `State == Painted && DecalsApplied == true`
- **Below the big number**: `<XX>% complete` (Completed / Total × 100, rounded to integer)

#### Right Side: State Progress Bar
- A **horizontal stacked bar** showing the proportion of each state:
  - **Sprue** → Light gray (`bg-gray-400`)
  - **Built** → Orange (`bg-orange-500`)
  - **Primed** → Blue (`bg-blue-500`)
  - **Painted** → Green (`bg-green-500`)
- Each segment's width is proportional to `count(state) / total × 100%`.
- Percentages are displayed as labels **below** the bar (not inside). Small segments (< 5%) still show their label below.
- Segments with 0 count are hidden entirely (bar adjusts to show only states with minis).

#### Right Side: Additional Metrics (below the bar)
Three values displayed horizontally as plain text:
- **Base Painted**: `<XX>%` of miniatures with `BasePainted == true`
- **Magnetized**: `<XX>%` of miniatures with `BaseMagnetized == true`
- **Decals Applied**: `<XX>%` of miniatures with `DecalsApplied == true`

All percentages are calculated against the total matching the current filter.

### Faction Breakdown (Drilldown)
- **Only visible** when a specific game is selected in the filter (not "All").
- **Expanded by default** when visible (no toggle needed).
- Displays a table with one row per faction that has miniatures in the collection.
- Columns:
  - Faction Name
  - Total miniatures
  - Sprue count
  - Built count
  - Primed count
  - Painted count
  - % Complete (Painted && DecalsApplied / Total × 100)
- Rows sorted by **FactionGroup first** (Imperium, Space Marines, Chaos, Xenos), then by faction name alphabetically within each group.
- Clicking a faction row navigates to the **Collection Faction Page** (spec 003).

## API Endpoints

### `GET /api/home/active-projects`
Returns the 2 most recently updated projects with stats.

**Response:**
```json
[
  {
    "projectId": 1,
    "name": "Ultramarines Army",
    "gameName": "Warhammer 40K 11th Ed",
    "totalMinis": 24,
    "completedMinis": 12,
    "percentComplete": 50,
    "updatedAt": "2026-06-23T14:30:00Z"
  }
]
```

### `GET /api/home/stats?gameId={gameId}`
Returns collection stats. If `gameId` is omitted or null, returns stats for all games.

**Response:**
```json
{
  "total": 350,
  "completed": 127,
  "percentComplete": 36,
  "sprue": 80,
  "built": 60,
  "primed": 83,
  "painted": 127,
  "basePainted": 158,
  "baseMagnetized": 42,
  "decalsApplied": 105,
  "factions": [
    {
      "factionId": 1,
      "factionName": "Ultramarines",
      "total": 50,
      "sprue": 5,
      "built": 10,
      "primed": 15,
      "painted": 20,
      "percentComplete": 40
    }
  ]
}
```

**Notes:**
- `factions` array is only populated when `gameId` is provided (not "All").
- When `gameId` is null/omitted, `factions` is `null` or omitted.
- Factions sorted by `FactionGroup` order (Imperium, Space Marines, Chaos, Xenos), then by `Name` alphabetically.

## Backend Implementation Notes

### Controller
- New controller: `HomeController` at `/api/home/`
- Follows existing pattern: inject `AppDbContext`, async methods, anonymous response objects.

### Stats Calculation
- All stats calculated inline in EF Core LINQ queries using `.Count(m => m.State == MiniatureState.X)` pattern.
- Percentages use `Math.Round()` with division-by-zero guard.

### Active Projects Query
```csharp
await _db.Projects
  .Include(p => p.Game)
  .Include(p => p.ProjectMiniatures).ThenInclude(pm => pm.Miniature)
  .OrderByDescending(p => p.UpdatedAt)
  .Take(2)
  .ToListAsync()
```

### Stats Query (filtered by game)
```csharp
var query = _db.Miniatures
  .Include(m => m.Unit).ThenInclude(u => u!.Faction)
  .AsQueryable();

if (gameId.HasValue)
  query = query.Where(m => m.Unit!.Faction!.GameId == gameId.Value);

// Then compute all stats via .Count() on the filtered query
```

## Frontend Implementation Notes

### Component Structure
- `HomePage.vue` — main layout container
- `ActiveProjects.vue` — renders the two project cards
- `CollectionStats.vue` — renders the stats section with game filter, summary numbers, state bar, and faction drilldown

### State Bar
- Use a flex container with `width: XX%` on each color segment.
- Colors: `bg-gray-400` (Sprue), `bg-orange-500` (Built), `bg-blue-500` (Primed), `bg-green-500` (Painted).
- Labels displayed below each segment with the percentage.
- Segments with 0 count are hidden. If all counts are 0, show "No data" message instead of the bar.

### Loading State
- On initial load, show **skeleton placeholders** for project cards and a **spinner** in the stats area.
- Skeleton cards mimic the card layout with animated gray blocks.
- Stats section shows a centered loading spinner until data arrives.

### Error Handling
- Each section (Active Projects, Stats) handles errors independently.
- On API failure, show an **error message with a retry button** within that section.
- Example: "Failed to load projects. [Retry]"

### Game Filter Persistence
- The selected game filter value is stored in `localStorage` under key `home-game-filter`.
- On page load, read from `localStorage` and use as initial value (default: `null` for "All").
- When the user changes the dropdown, update `localStorage`.

### Data Refresh
- Data is fetched once on page load/navigation.
- No auto-refresh or polling. User must navigate away and back to see updated stats.

### Responsive Behavior
- On large screens: left summary + right bar side by side.
- On small screens: stack vertically (summary on top, bar below).
