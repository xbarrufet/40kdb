namespace _40kdb.Models;

public class Faction
{
    public int FactionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FactionGroup { get; set; } = string.Empty;
    public int GameId { get; set; }
    public Game? Game { get; set; }
    public List<Unit> Units { get; set; } = new();
}
