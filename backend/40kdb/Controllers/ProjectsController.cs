using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _40kdb.Data;
using _40kdb.Models;

namespace _40kdb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _db.Projects
            .Include(p => p.Game)
            .Include(p => p.Phases)
            .Include(p => p.ProjectMiniatures)
                .ThenInclude(pm => pm.Miniature)
            .Select(p => new
            {
                p.ProjectId,
                p.Name,
                GameName = p.Game!.Name,
                p.GameId,
                TotalMinis = p.ProjectMiniatures.Count,
                CompletedMinis = p.ProjectMiniatures.Count(pm =>
                    pm.Miniature!.State == MiniatureState.Painted && pm.Miniature.DecalsApplied),
                p.CreatedAt,
                p.UpdatedAt
            })
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Game)
            .Include(p => p.Phases.OrderBy(ph => ph.SortOrder))
            .Include(p => p.ProjectMiniatures)
                .ThenInclude(pm => pm.Miniature)
                    .ThenInclude(m => m!.Unit)
                        .ThenInclude(u => u!.Faction)
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null) return NotFound();

        var phases = project.Phases.Select(ph => new
        {
            ph.PhaseId,
            ph.Name,
            ph.SortOrder,
            Minis = project.ProjectMiniatures
                .Where(pm => pm.PhaseId == ph.PhaseId)
                .Select(pm => new
                {
                    pm.Miniature!.MiniatureId,
                    UnitName = pm.Miniature.Unit!.Name,
                    FactionName = pm.Miniature.Unit.Faction!.Name,
                    pm.Miniature.State,
                    pm.Miniature.BasePainted,
                    pm.Miniature.BaseMagnetized,
                    pm.Miniature.DecalsApplied,
                    pm.Miniature.Original,
                    pm.Miniature.Proxy,
                    pm.Miniature.Edition,
                    pm.AddedAt
                })
                .OrderBy(m => m.MiniatureId)
                .ToList()
        }).ToList();

        var totalMinis = project.ProjectMiniatures.Count;
        var completedMinis = project.ProjectMiniatures.Count(pm =>
            pm.Miniature!.State == MiniatureState.Painted && pm.Miniature.DecalsApplied);

        return Ok(new
        {
            project.ProjectId,
            project.Name,
            GameName = project.Game!.Name,
            project.GameId,
            TotalMinis = totalMinis,
            CompletedMinis = completedMinis,
            PercentComplete = totalMinis > 0 ? Math.Round((double)completedMinis / totalMinis * 100) : 0,
            Phases = phases,
            project.CreatedAt,
            project.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var game = await _db.Games.FindAsync(request.GameId);
        if (game == null) return BadRequest("Game not found");

        var project = new Project
        {
            Name = request.Name,
            GameId = request.GameId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(request.InitialPhaseName))
        {
            var phase = new Phase
            {
                Name = request.InitialPhaseName,
                ProjectId = project.ProjectId,
                SortOrder = 0
            };
            _db.Phases.Add(phase);
            await _db.SaveChangesAsync();
        }

        return Ok(new { project.ProjectId, project.Name });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest request)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound();

        project.Name = request.Name;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound();

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/phases")]
    public async Task<IActionResult> AddPhase(int id, [FromBody] AddPhaseRequest request)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound();

        var maxOrder = await _db.Phases
            .Where(p => p.ProjectId == id)
            .MaxAsync(p => (int?)p.SortOrder) ?? -1;

        var phase = new Phase
        {
            Name = request.Name,
            ProjectId = id,
            SortOrder = maxOrder + 1
        };
        _db.Phases.Add(phase);
        await _db.SaveChangesAsync();

        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { phase.PhaseId, phase.Name, phase.SortOrder });
    }

    [HttpPut("{id}/phases/{phaseId}")]
    public async Task<IActionResult> UpdatePhase(int id, int phaseId, [FromBody] UpdatePhaseRequest request)
    {
        var phase = await _db.Phases
            .FirstOrDefaultAsync(p => p.PhaseId == phaseId && p.ProjectId == id);
        if (phase == null) return NotFound();

        phase.Name = request.Name;
        await _db.SaveChangesAsync();

        var project = await _db.Projects.FindAsync(id);
        if (project != null)
        {
            project.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpDelete("{id}/phases/{phaseId}")]
    public async Task<IActionResult> DeletePhase(int id, int phaseId)
    {
        var phase = await _db.Phases
            .FirstOrDefaultAsync(p => p.PhaseId == phaseId && p.ProjectId == id);
        if (phase == null) return NotFound();

        var hasMinis = await _db.ProjectMiniatures.AnyAsync(pm => pm.PhaseId == phaseId);
        if (hasMinis) return BadRequest("Cannot delete phase with miniatures. Move them first.");

        _db.Phases.Remove(phase);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/phases/reorder")]
    public async Task<IActionResult> ReorderPhases(int id, [FromBody] ReorderPhasesRequest request)
    {
        var phases = await _db.Phases
            .Where(p => p.ProjectId == id)
            .ToListAsync();

        foreach (var phase in phases)
        {
            var newIndex = request.PhaseIds.IndexOf(phase.PhaseId);
            if (newIndex >= 0)
                phase.SortOrder = newIndex;
        }

        await _db.SaveChangesAsync();

        var project = await _db.Projects.FindAsync(id);
        if (project != null)
        {
            project.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpPost("{id}/minis")]
    public async Task<IActionResult> AddMinis(int id, [FromBody] AddMinisToProjectRequest request)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound();

        var phase = await _db.Phases
            .FirstOrDefaultAsync(p => p.PhaseId == request.PhaseId && p.ProjectId == id);
        if (phase == null) return BadRequest("Phase not found in this project");

        foreach (var miniId in request.MiniatureIds)
        {
            var exists = await _db.ProjectMiniatures.AnyAsync(pm =>
                pm.MiniatureId == miniId && pm.ProjectId == id);
            if (exists) continue;

            var mini = await _db.Miniatures.FindAsync(miniId);
            if (mini == null) continue;

            _db.ProjectMiniatures.Add(new ProjectMiniature
            {
                ProjectId = id,
                PhaseId = request.PhaseId,
                MiniatureId = miniId,
                AddedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("{id}/minis/move")]
    public async Task<IActionResult> MoveMinis(int id, [FromBody] MoveMinisRequest request)
    {
        var phase = await _db.Phases
            .FirstOrDefaultAsync(p => p.PhaseId == request.TargetPhaseId && p.ProjectId == id);
        if (phase == null) return BadRequest("Target phase not found in this project");

        var minis = await _db.ProjectMiniatures
            .Where(pm => request.MiniatureIds.Contains(pm.MiniatureId) && pm.ProjectId == id)
            .ToListAsync();

        foreach (var mini in minis)
        {
            mini.PhaseId = request.TargetPhaseId;
        }

        await _db.SaveChangesAsync();

        var project = await _db.Projects.FindAsync(id);
        if (project != null)
        {
            project.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpDelete("{id}/minis")]
    public async Task<IActionResult> RemoveMinis(int id, [FromBody] RemoveMinisRequest request)
    {
        var minis = await _db.ProjectMiniatures
            .Where(pm => request.MiniatureIds.Contains(pm.MiniatureId) && pm.ProjectId == id)
            .ToListAsync();

        _db.ProjectMiniatures.RemoveRange(minis);
        await _db.SaveChangesAsync();

        var project = await _db.Projects.FindAsync(id);
        if (project != null)
        {
            project.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    public class CreateProjectRequest
    {
        public string Name { get; set; } = "";
        public int GameId { get; set; }
        public string? InitialPhaseName { get; set; }
    }

    public class UpdateProjectRequest
    {
        public string Name { get; set; } = "";
    }

    public class AddPhaseRequest
    {
        public string Name { get; set; } = "";
    }

    public class UpdatePhaseRequest
    {
        public string Name { get; set; } = "";
    }

    public class ReorderPhasesRequest
    {
        public List<int> PhaseIds { get; set; } = new();
    }

    public class AddMinisToProjectRequest
    {
        public int PhaseId { get; set; }
        public List<int> MiniatureIds { get; set; } = new();
    }

    public class MoveMinisRequest
    {
        public int TargetPhaseId { get; set; }
        public List<int> MiniatureIds { get; set; } = new();
    }

    public class RemoveMinisRequest
    {
        public List<int> MiniatureIds { get; set; } = new();
    }
}
