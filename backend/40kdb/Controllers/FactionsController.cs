using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _40kdb.Data;
using _40kdb.Models;

namespace _40kdb.Controllers;

[ApiController]
[Route("api/games/{gameId}/factions")]
public class FactionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public FactionsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(int gameId)
    {
        var factions = await _db.Factions
            .Where(f => f.GameId == gameId)
            .Include(f => f.Units)
            .ThenInclude(u => u.Miniatures)
            .Where(f => f.Units.Count > 0)
            .GroupBy(f => f.FactionGroup)
            .Select(g => new
            {
                Group = g.Key,
                Factions = g.Select(f => new
                {
                    f.FactionId,
                    f.Name,
                    UnitCount = f.Units.Count,
                    MiniatureCount = f.Units.SelectMany(u => u.Miniatures).Count()
                })
            })
            .ToListAsync();
        return Ok(factions);
    }

    [HttpGet("{factionId}")]
    public async Task<IActionResult> Get(int gameId, int factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Units)
            .ThenInclude(u => u.Miniatures)
            .FirstOrDefaultAsync(f => f.GameId == gameId && f.FactionId == factionId);

        if (faction == null) return NotFound();

        return Ok(new
        {
            faction.FactionId,
            faction.Name,
            faction.FactionGroup,
            Units = faction.Units.Select(u => new
            {
                u.UnitId,
                u.Name,
                u.Category,
                u.Points,
                MiniatureCount = u.Miniatures.Count
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(int gameId, [FromBody] List<FactionRequest> factions)
    {
        foreach (var req in factions)
        {
            var existing = await _db.Factions
                .FirstOrDefaultAsync(f => f.GameId == gameId && f.Name == req.Name);

            if (existing != null)
            {
                existing.FactionGroup = req.FactionGroup;
            }
            else
            {
                _db.Factions.Add(new Faction
                {
                    Name = req.Name,
                    FactionGroup = req.FactionGroup,
                    GameId = gameId
                });
            }
        }
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{factionId}")]
    public async Task<IActionResult> Delete(int gameId, int factionId)
    {
        var faction = await _db.Factions
            .FirstOrDefaultAsync(f => f.GameId == gameId && f.FactionId == factionId);

        if (faction == null) return NotFound();

        _db.Factions.Remove(faction);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    public record FactionRequest(string Name, string FactionGroup);
}
