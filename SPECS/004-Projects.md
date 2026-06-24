## Main goal
A Project is the process of painting a set of miniatures. A project is split into phases that focus on a reduced set of the project miniatures. It allows to structure the process of painting miniatures.

## Data Model
```csharp
public class Project
{
    public int ProjectId { get; set; }
    public string Name { get; set; }
    public int GameId { get; set; }
    public Game? Game { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Phase> Phases { get; set; } = new();
    public List<ProjectMiniature> ProjectMiniatures { get; set; } = new();
}

public class Phase
{
    public int PhaseId { get; set; }
    public string Name { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public int SortOrder { get; set; }
}

public class ProjectMiniature
{
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public int PhaseId { get; set; }
    public Phase? Phase { get; set; }
    public int MiniatureId { get; set; }
    public Miniature? Miniature { get; set; }
    public DateTime AddedAt { get; set; }
}
```

### Constraints
- A miniature belongs to **one project only**
- Each project is scoped to **one game**
- Phases are user-reorderable (SortOrder)
- Deleting a miniature auto-removes it from projects (cascade)

## Sidebar
Add a "Projects" entry in the sidebar.

## Projects Home Page
Shows project cards in a grid. Each card shows:
- Project Name
- Game/Faction
- Number of Minis
- % Completed
- Delete icon (visible on hover) → confirmation modal before deleting

Clicking a card opens the ProjectDetailPage.

"Create Project" button to start a new project.

## Project Detail Page
### Header
- Inline-editable project name
- Game name
- % Completed (calculated)

### Phases
Phases are shown as **single-open accordions** (only one open at a time).

**Accordion header** shows:
- Phase Name
- Total minis in phase
- Last update date
- % Completed (calculated)
- ▲/▼ arrows to reorder phases

**Bulk actions bar** (visible when minis are selected):
- "X selected" text
- "Remove from project" button → confirmation modal
- "Move phase" dropdown → select target phase

**Accordion body** shows mini rows with:
- Checkbox to select
- Miniature ID
- Unit name
- Army/Faction
- State (editable dropdown)
- Base Painted (editable checkbox)
- Base Magnetized (editable checkbox)
- Decals Applied (editable checkbox)

All fields are inline-editable and update the miniature directly.

### Phase Management
- "Add Phase" button in project header → modal with phase name input
- Phases can be renamed (inline edit)
- Phases can be deleted only if empty (error shown otherwise)
- Phases reorderable via ▲/▼ arrows

## How to Start a Project
### From Collection/Factions
On each unit row in Collection/Factions, there is an "Add to Project" button.

Clicking opens a modal:
1. Select existing project (dropdown) OR create new
2. If new: project name defaults to faction name (editable)
3. Checkbox list of miniatures to add (user selects specific minis)
4. Select target phase (dropdown)
5. Save

If creating new project, a default phase is created with the unit name.

### From Project Detail
Miniatures can be added to existing phases from the project detail page.

## Completion Calculation
A miniature is **complete** when: `State == Painted && DecalsApplied == true`

**Project % completed** = (complete minis in project) / (total minis in project) × 100

**Phase % completed** = (complete minis in phase) / (total minis in phase) × 100

## API Endpoints
- `GET /api/projects` — list projects with stats
- `GET /api/projects/{id}` — project detail with phases and minis
- `POST /api/projects` — create project
- `PUT /api/projects/{id}` — update project name
- `DELETE /api/projects/{id}` — delete project and all data
- `POST /api/projects/{id}/phases` — add phase
- `PUT /api/projects/{id}/phases/{phaseId}` — rename phase
- `DELETE /api/projects/{id}/phases/{phaseId}` — delete phase (only if empty)
- `PUT /api/projects/{id}/phases/reorder` — reorder phases
- `POST /api/projects/{id}/minis` — add minis to a phase
- `PUT /api/projects/{id}/minis/move` — move minis between phases
- `DELETE /api/projects/{id}/minis` — remove minis from project
