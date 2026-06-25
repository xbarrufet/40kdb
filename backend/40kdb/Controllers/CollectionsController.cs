using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _40kdb.Data;
using _40kdb.Models;

namespace _40kdb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CollectionsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetFactionsWithMiniatures()
    {
        var factions = await _db.Factions
            .Where(f => f.Units.Any(u => u.Miniatures.Count > 0))
            .Include(f => f.Game)
            .Include(f => f.Units)
                .ThenInclude(u => u.Miniatures)
            .Select(f => new
            {
                f.FactionId,
                f.Name,
                f.FactionGroup,
                GameName = f.Game!.Name,
                f.GameId,
                Total = f.Units.SelectMany(u => u.Miniatures).Count(),
                Sprue = f.Units.SelectMany(u => u.Miniatures).Count(m => m.State == MiniatureState.Sprue),
                Built = f.Units.SelectMany(u => u.Miniatures).Count(m => m.State == MiniatureState.Built),
                Primed = f.Units.SelectMany(u => u.Miniatures).Count(m => m.State == MiniatureState.Primed),
                Painted = f.Units.SelectMany(u => u.Miniatures).Count(m => m.State == MiniatureState.Painted)
            })
            .ToListAsync();
        return Ok(factions);
    }

    [HttpGet("factions/{factionId}")]
    public async Task<IActionResult> GetUnitsWithMiniatures(int factionId)
    {
        var faction = await _db.Factions
            .Include(f => f.Game)
            .FirstOrDefaultAsync(f => f.FactionId == factionId);

        if (faction == null) return NotFound();

        var units = await _db.Units
            .Where(u => u.FactionId == factionId && u.Miniatures.Count > 0)
            .Include(u => u.Miniatures)
            .Select(u => new
            {
                u.UnitId,
                u.Name,
                u.Category,
                u.Points,
                Total = u.Miniatures.Count,
                Sprue = u.Miniatures.Count(m => m.State == MiniatureState.Sprue),
                Built = u.Miniatures.Count(m => m.State == MiniatureState.Built),
                Primed = u.Miniatures.Count(m => m.State == MiniatureState.Primed),
                Painted = u.Miniatures.Count(m => m.State == MiniatureState.Painted)
            })
            .OrderBy(u => u.Category)
            .ThenBy(u => u.Name)
            .ToListAsync();

        return Ok(new
        {
            faction.FactionId,
            faction.Name,
            faction.FactionGroup,
            GameName = faction.Game!.Name,
            Units = units
        });
    }

    [HttpGet("units/{unitId}")]
    public async Task<IActionResult> GetMiniatures(int unitId)
    {
        var unit = await _db.Units
            .Include(u => u.Faction)
            .FirstOrDefaultAsync(u => u.UnitId == unitId);

        if (unit == null) return NotFound();

        var miniatures = await _db.Miniatures
            .Where(m => m.UnitId == unitId)
            .Select(m => new { m.MiniatureId, m.State, m.Edition, m.Wargear, m.Champion, m.BasePainted, m.BaseMagnetized, m.Original, m.Proxy, m.DecalsApplied })
            .ToListAsync();

        return Ok(new
        {
            unit.UnitId,
            unit.Name,
            unit.Category,
            unit.Points,
            FactionName = unit.Faction!.Name,
            Miniatures = miniatures
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddMiniatures([FromBody] AddMiniaturesRequest request)
    {
        var unit = await _db.Units.FindAsync(request.UnitId);
        if (unit == null) return NotFound();

        for (int i = 0; i < request.Quantity; i++)
        {
            _db.Miniatures.Add(new Miniature
            {
                UnitId = request.UnitId,
                State = request.State,
                Edition = request.Edition,
                Wargear = request.Wargear,
                Champion = request.Champion,
                BasePainted = request.BasePainted,
                BaseMagnetized = request.BaseMagnetized,
                Original = request.Original,
                Proxy = request.Proxy,
                DecalsApplied = request.DecalsApplied
            });
        }
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("batch")]
    public async Task<IActionResult> BatchUpdateMiniatures([FromBody] BatchUpdateMiniaturesRequest request)
    {
        var miniatures = await _db.Miniatures
            .Where(m => request.Ids.Contains(m.MiniatureId))
            .ToListAsync();

        foreach (var miniature in miniatures)
        {
            miniature.State = request.Changes.State;
            miniature.Edition = request.Changes.Edition;
            miniature.Wargear = request.Changes.Wargear;
            miniature.Champion = request.Changes.Champion;
            miniature.BasePainted = request.Changes.BasePainted;
            miniature.BaseMagnetized = request.Changes.BaseMagnetized;
            miniature.Original = request.Changes.Original;
            miniature.Proxy = request.Changes.Proxy;
            miniature.DecalsApplied = request.Changes.DecalsApplied;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMiniature(int id, [FromBody] UpdateMiniatureRequest request)
    {
        var miniature = await _db.Miniatures.FindAsync(id);
        if (miniature == null) return NotFound();

        miniature.State = request.State;
        miniature.Edition = request.Edition;
        miniature.Wargear = request.Wargear;
        miniature.Champion = request.Champion;
        miniature.BasePainted = request.BasePainted;
        miniature.BaseMagnetized = request.BaseMagnetized;
        miniature.Original = request.Original;
        miniature.Proxy = request.Proxy;
        miniature.DecalsApplied = request.DecalsApplied;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMiniature(int id)
    {
        var miniature = await _db.Miniatures.FindAsync(id);
        if (miniature == null) return NotFound();

        _db.Miniatures.Remove(miniature);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    public class AddMiniaturesRequest
    {
        public int UnitId { get; set; }
        public MiniatureState State { get; set; }
        public int Quantity { get; set; }
        public string Edition { get; set; } = "";
        public string Wargear { get; set; } = "";
        public bool Champion { get; set; } = false;
        public bool BasePainted { get; set; } = false;
        public bool BaseMagnetized { get; set; } = false;
        public bool Original { get; set; } = true;
        public bool Proxy { get; set; } = false;
        public bool DecalsApplied { get; set; } = false;
    }

    public class UpdateMiniatureRequest
    {
        public MiniatureState State { get; set; }
        public string Edition { get; set; } = "";
        public string Wargear { get; set; } = "";
        public bool Champion { get; set; } = false;
        public bool BasePainted { get; set; } = false;
        public bool BaseMagnetized { get; set; } = false;
        public bool Original { get; set; } = true;
        public bool Proxy { get; set; } = false;
        public bool DecalsApplied { get; set; } = false;
    }

    public class BatchUpdateMiniaturesRequest
    {
        public List<int> Ids { get; set; } = new();
        public UpdateMiniatureRequest Changes { get; set; } = new();
    }
}
