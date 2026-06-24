namespace _40kdb.Models;

public class Unit
{
    public int UnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Points { get; set; }
    public int FactionId { get; set; }
    public Faction? Faction { get; set; }
    public List<Miniature> Miniatures { get; set; } = new();
}
