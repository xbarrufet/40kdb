## Main goal
Merge Games and Collections into a single unified flow. Remove Collections from the sidebar. Games becomes the only entry point to browse factions/units, with an optional "Show only owned" filter at each level to surface collection data.

## Flow
```
Games (select game) → Factions (select faction) → Units (select unit) → Miniatures
```
Always the same path. The "Show only owned" toggle at each level filters to only show items that have miniatures in the collection.

## Sidebar
Remove the "Collections" entry. Sidebar entries become: Home, Games, Projects.

## Games View (Faction Grid)
- Dropdown to select game (unchanged)
- **NEW**: Checkbox "Show only owned" next to the dropdown, default unchecked
- When checked: only factions with at least one miniature in the collection are shown
- Factions still grouped by FactionGroup
- Click faction → always goes to FactionView

**Backend:** Modify `GET /api/games/{id}` to include `MiniatureCount` per faction (total miniatures across all units). Response shape:
```json
{
  "gameId": 1,
  "name": "Warhammer 40K",
  "factions": [
    { "factionId": 1, "name": "Space Marines", "factionGroup": "Imperium", "unitCount": 12, "miniatureCount": 5 }
  ]
}
```
The frontend filters client-side: when checkbox is checked, keep only factions where `miniatureCount > 0`.

## Faction View (Units within a Faction)
- Header: faction name + faction group (unchanged)
- **NEW**: Checkbox "Show only owned" below the faction name, default unchecked
- When checked: only units with at least one miniature in the collection are shown
- **NEW**: Each unit line shows `(xx)` at the end of the unit name, where `xx` = number of miniatures in collection for that unit. Show `(0)` or nothing when count is 0.
- Click unit name → goes to UnitCollectionView (miniatures list for that unit)
- "+ Add" button per unit → opens AddMiniatureModal (unchanged)

**Backend:** Modify `GET /api/games/{gameId}/factions/{factionId}` to include `MiniatureCount` per unit. Response shape:
```json
{
  "factionId": 1,
  "name": "Space Marines",
  "factionGroup": "Imperium",
  "units": [
    { "unitId": 1, "name": "Intercessors", "category": "Troops", "points": 90, "miniatureCount": 5 }
  ]
}
```
The frontend filters client-side: when checkbox is checked, keep only units where `miniatureCount > 0`.

## Unit Collection View (Miniatures)
No changes. This view already works as the destination for viewing/editing miniatures per unit.

## Routes
- Keep all existing routes for backward compatibility (direct URL access, bookmarks)
- `/collections` and `/collections/factions/:factionId` remain accessible but are not linked from the sidebar
- `/collections/units/:unitId` is now also reached from FactionView → click unit name

## What is removed
- Collections sidebar entry
- CollectionsView.vue is no longer the primary entry (but kept for backward compat)
- FactionCollectionView.vue is no longer navigated to from sidebar (but kept for backward compat)

## What is NOT changed
- AddMiniatureModal behavior
- UnitCollectionView (miniatures list, edit, copy, delete, batch update)
- Projects flow
- Backend CollectionsController (still serves existing endpoints)
