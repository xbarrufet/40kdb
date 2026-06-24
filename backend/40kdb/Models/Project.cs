namespace _40kdb.Models;

public class Project
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = "";
    public int GameId { get; set; }
    public Game? Game { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<Phase> Phases { get; set; } = new();
    public List<ProjectMiniature> ProjectMiniatures { get; set; } = new();
}

public class Phase
{
    public int PhaseId { get; set; }
    public string Name { get; set; } = "";
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
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
