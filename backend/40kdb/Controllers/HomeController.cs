using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _40kdb.Data;
using _40kdb.Models;

namespace _40kdb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db) => _db = db;

    [HttpGet("active-projects")]
    public async Task<IActionResult> GetActiveProjects()
    {
        var projects = await _db.Projects
            .Include(p => p.Game)
            .Include(p => p.ProjectMiniatures)
                .ThenInclude(pm => pm.Miniature)
            .OrderByDescending(p => p.UpdatedAt)
            .Take(2)
            .Select(p => new
            {
                p.ProjectId,
                p.Name,
                GameName = p.Game!.Name,
                TotalMinis = p.ProjectMiniatures.Count,
                CompletedMinis = p.ProjectMiniatures.Count(pm =>
                    pm.Miniature!.State == MiniatureState.Painted && pm.Miniature.DecalsApplied),
                PercentComplete = p.ProjectMiniatures.Count > 0
                    ? Math.Round((double)p.ProjectMiniatures.Count(pm =>
                        pm.Miniature!.State == MiniatureState.Painted && pm.Miniature.DecalsApplied)
                        / p.ProjectMiniatures.Count * 100)
                    : 0,
                p.UpdatedAt
            })
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] int? gameId)
    {
        var query = _db.Miniatures
            .Include(m => m.Unit)
                .ThenInclude(u => u!.Faction)
                    .ThenInclude(f => f!.Game)
            .AsQueryable();

        if (gameId.HasValue)
            query = query.Where(m => m.Unit!.Faction!.GameId == gameId.Value);

        var minis = await query.ToListAsync();

        if (minis.Count == 0)
        {
            return Ok(new
            {
                Total = 0,
                Completed = 0,
                PercentComplete = 0,
                Sprue = 0,
                Built = 0,
                Primed = 0,
                Painted = 0,
                BasePainted = 0,
                BaseMagnetized = 0,
                DecalsApplied = 0,
                Factions = gameId.HasValue ? new object[0] : null
            });
        }

        var total = minis.Count;
        var completed = minis.Count(m => m.State == MiniatureState.Painted && m.DecalsApplied);
        var sprue = minis.Count(m => m.State == MiniatureState.Sprue);
        var built = minis.Count(m => m.State == MiniatureState.Built);
        var primed = minis.Count(m => m.State == MiniatureState.Primed);
        var painted = minis.Count(m => m.State == MiniatureState.Painted);
        var basePainted = minis.Count(m => m.BasePainted);
        var baseMagnetized = minis.Count(m => m.BaseMagnetized);
        var decalsApplied = minis.Count(m => m.DecalsApplied);

        var groupOrder = new Dictionary<string, int>
        {
            { "Imperium", 0 },
            { "Space Marines", 1 },
            { "Chaos", 2 },
            { "Xenos", 3 }
        };

        object[]? factions = null;
        if (gameId.HasValue)
        {
            factions = minis
                .GroupBy(m => new
                {
                    m.Unit!.Faction!.FactionId,
                    m.Unit.Faction.Name,
                    m.Unit.Faction.FactionGroup
                })
                .Select(g => new
                {
                    g.Key.FactionId,
                    FactionName = g.Key.Name,
                    g.Key.FactionGroup,
                    Total = g.Count(),
                    Sprue = g.Count(m => m.State == MiniatureState.Sprue),
                    Built = g.Count(m => m.State == MiniatureState.Built),
                    Primed = g.Count(m => m.State == MiniatureState.Primed),
                    Painted = g.Count(m => m.State == MiniatureState.Painted),
                    PercentComplete = Math.Round(
                        (double)g.Count(m => m.State == MiniatureState.Painted && m.DecalsApplied)
                        / g.Count() * 100)
                })
                .OrderBy(f => groupOrder.TryGetValue(f.FactionGroup, out var order) ? order : 99)
                .ThenBy(f => f.FactionName)
                .ToArray();
        }

        return Ok(new
        {
            Total = total,
            Completed = completed,
            PercentComplete = Math.Round((double)completed / total * 100),
            Sprue = sprue,
            Built = built,
            Primed = primed,
            Painted = painted,
            BasePainted = basePainted,
            BaseMagnetized = baseMagnetized,
            DecalsApplied = decalsApplied,
            Factions = factions
        });
    }
}
