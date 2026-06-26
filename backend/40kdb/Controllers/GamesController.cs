using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _40kdb.Data;
using _40kdb.Models;

namespace _40kdb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly AppDbContext _db;

    public GamesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var games = await _db.Games
            .Include(g => g.Factions)
            .ThenInclude(f => f.Units)
            .Select(g => new { g.GameId, g.Name, FactionCount = g.Factions.Count(f => f.Units.Count > 0) })
            .ToListAsync();
        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var game = await _db.Games
            .Include(g => g.Factions)
            .ThenInclude(f => f.Units)
            .ThenInclude(u => u.Miniatures)
            .FirstOrDefaultAsync(g => g.GameId == id);

        if (game == null) return NotFound();

        return Ok(new
        {
            game.GameId,
            game.Name,
            Factions = game.Factions.Select(f => new
            {
                f.FactionId,
                f.Name,
                f.FactionGroup,
                UnitCount = f.Units.Count,
                MiniatureCount = f.Units.SelectMany(u => u.Miniatures).Count()
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGameRequest request)
    {
        var game = new Game { Name = request.Name };
        _db.Games.Add(game);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = game.GameId }, game);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateGameRequest request)
    {
        var game = await _db.Games.FindAsync(id);
        if (game == null) return NotFound();

        game.Name = request.Name;
        await _db.SaveChangesAsync();
        return Ok(game);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game == null) return NotFound();

        _db.Games.Remove(game);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    public record CreateGameRequest(string Name);
}
