# Plan: Merge Games + Collections

## Steps

### 1. Backend: Add MiniatureCount to Games endpoint
**File:** `backend/40kdb/Controllers/GamesController.cs`

Modify `Get(int id)` to include `MiniatureCount` per faction:
```csharp
MiniatureCount = f.Units.SelectMany(u => u.Miniatures).Count()
```
Add `.Include(f => f.Units).ThenInclude(u => u.Miniatures)` to the query.

### 2. Backend: Add MiniatureCount to Factions endpoint
**File:** `backend/40kdb/Controllers/FactionsController.cs`

Modify `Get(int gameId, int factionId)` to include `MiniatureCount` per unit:
```csharp
MiniatureCount = u.Miniatures.Count
```
Add `.Include(u => u.Miniatures)` to the units query.

### 3. Frontend: Remove Collections from Sidebar
**File:** `frontend/src/components/Sidebar.vue`

Delete the Collections `<router-link>` block (lines ~37-47).

### 4. Frontend: Add "Show only owned" checkbox to GamesView
**File:** `frontend/src/views/GamesView.vue`

- Add `const showOwned = ref(false)` 
- Add checkbox next to the `<select>` dropdown
- Add computed `filteredFactionsByGroup` that filters factions where `miniatureCount > 0` when `showOwned` is true
- Replace `factionsByGroup` usage in template with the filtered version
- Update `fetchFactions` to use the new endpoint response (includes `miniatureCount`)

### 5. Frontend: Add "Show only owned" checkbox + (xx) count to FactionView
**File:** `frontend/src/views/FactionView.vue`

- Add `const showOwned = ref(false)`
- Add checkbox below the faction name header
- Add `miniatureCount` to the unit fetch response parsing
- Add computed `filteredUnitsByCategory` that filters units where `miniatureCount > 0` when `showOwned` is true
- In template, append `(xx)` to unit name where xx = `unit.miniatureCount`
- Replace `unitsByCategory` usage with filtered version
- Update unit name click to navigate to `/collections/units/:unitId`

### 6. Verify
- Run `dotnet build` in backend
- Run `npm run build` (or lint/typecheck) in frontend
- Manual test: Games view shows checkbox, filtering works, faction click goes to FactionView, unit click goes to UnitCollectionView
