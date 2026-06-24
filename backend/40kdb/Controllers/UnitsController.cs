using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _40kdb.Data;
using _40kdb.Models;

namespace _40kdb.Controllers;

[ApiController]
[Route("api/games/{gameId}/factions/{factionId}/units")]
public class UnitsController : ControllerBase
{
    private readonly AppDbContext _db;

    public UnitsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(int gameId, int factionId)
    {
        var faction = await _db.Factions
            .FirstOrDefaultAsync(f => f.GameId == gameId && f.FactionId == factionId);

        if (faction == null) return NotFound();

        var units = await _db.Units
            .Where(u => u.FactionId == factionId)
            .OrderBy(u => u.Category)
            .ThenBy(u => u.Name)
            .Select(u => new { u.UnitId, u.Name, u.Category, u.Points })
            .ToListAsync();

        return Ok(new { faction.Name, faction.FactionGroup, Units = units });
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(int gameId, int factionId, [FromBody] List<UnitRequest> units)
    {
        var faction = await _db.Factions
            .FirstOrDefaultAsync(f => f.GameId == gameId && f.FactionId == factionId);

        if (faction == null) return NotFound();

        foreach (var req in units)
        {
            var existing = await _db.Units
                .FirstOrDefaultAsync(u => u.FactionId == factionId && u.Name == req.Name);

            if (existing != null)
            {
                existing.Points = req.Points;
                existing.Category = req.Category;
            }
            else
            {
                _db.Units.Add(new Unit
                {
                    Name = req.Name,
                    Category = req.Category,
                    Points = req.Points,
                    FactionId = factionId
                });
            }
        }
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{unitId}")]
    public async Task<IActionResult> Update(int gameId, int factionId, int unitId, [FromBody] UnitRequest req)
    {
        var unit = await _db.Units
            .FirstOrDefaultAsync(u => u.FactionId == factionId && u.UnitId == unitId);

        if (unit == null) return NotFound();

        unit.Name = req.Name;
        unit.Category = req.Category;
        unit.Points = req.Points;
        await _db.SaveChangesAsync();
        return Ok(unit);
    }

    [HttpDelete("{unitId}")]
    public async Task<IActionResult> Delete(int gameId, int factionId, int unitId)
    {
        var unit = await _db.Units
            .FirstOrDefaultAsync(u => u.FactionId == factionId && u.UnitId == unitId);

        if (unit == null) return NotFound();

        _db.Units.Remove(unit);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    public record UnitRequest(string Name, string Category, int Points);
}
