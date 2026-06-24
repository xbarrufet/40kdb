namespace _40kdb.Models;

public class Game
{
    public int GameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Faction> Factions { get; set; } = new();
}
