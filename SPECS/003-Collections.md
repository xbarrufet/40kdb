## Main goal
This spec describes how to add miniatures to a collection, every miniature has a matching with a model of a faction/game.

## State Flow
A miniature goes through the following states:
```
SPRUE -> BUILT -> PRIMED -> PAINTED
```

## Sidebar
Add an entry called "Collections".

## Collections Main Page (Factions)
The Collections main page shows **only** factions that have miniatures (factions with 0 miniatures are hidden).
It allows to filter by game on top.

Each row shows:
- Faction name
- Total miniatures count
- Count of miniatures by state: Sprue / Built / Primed / Painted

Clicking in the row opens the Collection faction page.

## Collection Faction Page (Units)
Shows all units that have miniatures in the collection.

Each row shows:
- Unit name
- Total miniatures count
- Count of miniatures by state: Sprue / Built / Primed / Painted

Clicking on it opens the Unit collection page.
There is an "Add" button that opens the add modal.

## Unit Collection Page (Miniatures)
It shows all the minis and its state. Each row can be edited changing the state or any other field, or deleted.

### Mass Update
Each row has a checkbox in the first column. The header contains:
- A "select all / select none" toggle checkbox
- An "Edit (x)" button (visible only when x > 0), where `x` is the number of selected minis

Selected rows have a distinct background color.

**Selection rules:**
- Selections are tied to miniatureId, persist across sort and after modal close
- Unlimited selection, no cap
- No pagination (all minis rendered on one page)

**Modal:**
- Title: "Edit X miniatures"
- Same fields as individual edit: State, Edition, BasePainted, BaseMagnetized, Original, Proxy, DecalsApplied
- Pre-filled with values from the first selected mini
- All fields overwrite every selected mini on save
- Enter to save, Escape to cancel
- Save button disabled during request
- No API call or toast if nothing changed

**API:**
- `PUT /api/miniatures/batch` with body `{ ids: int[], changes: {...} }`
- Failing minis are skipped silently
- Table re-fetches from server after save

**Toast:**
- Single toast: "X miniatures updated"

## How to add a miniature
### Way 1
In the Collection faction there is a Add button that opens a modal showing:
- Dropdown: Faction Models
- Text box: Number of minis
- Radio buttons: State (Sprue / Built / Primed / Painted)
- Text field: Edition
- Checkboxes: Base Painted, Base Magnetized, Original, Proxy, Decals Applied
- Save and Cancel buttons

Every miniature generates a row in the miniatures table, it can be edited individually.

### Way 2
From the models list of the games/factions add a button on the right side of the row "Add To Collection" that opens the modal screen with the faction/model preselected.

For both methods, add below the model dropdown a text with the number of this models (just the total number) in the collection, it changes everytime the dropdown changes.

## Miniature Fields
```csharp
public enum MiniatureState
{
    Sprue,
    Built,
    Primed,
    Painted
}

public class Miniature
{
    public int MiniatureId { get; set; }
    public int UnitId { get; set; }
    public Unit? Unit { get; set; }
    public MiniatureState State { get; set; } = MiniatureState.Sprue;
    public string Edition { get; set; } = "";          // game edition the miniature was released in
    public bool BasePainted { get; set; } = false;     // the miniature has the base painted
    public bool BaseMagnetized { get; set; } = false;  // the miniature has the base magnetized
    public bool Original { get; set; } = true;         // miniature is original (not recast/clone)
    public bool Proxy { get; set; } = false;           // the model represents is not the official one
    public bool DecalsApplied { get; set; } = false;   // decals have been applied (if needed)
}
```

## Completion
A miniature is **complete** when: `State == Painted && DecalsApplied == true`
