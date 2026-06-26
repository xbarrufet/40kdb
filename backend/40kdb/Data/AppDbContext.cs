using Microsoft.EntityFrameworkCore;
using _40kdb.Models;
using System.Text.Json;

namespace _40kdb.Data;

public class AppDbContext : DbContext
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Faction> Factions => Set<Faction>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Miniature> Miniatures => Set<Miniature>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Phase> Phases => Set<Phase>();
    public DbSet<ProjectMiniature> ProjectMiniatures => Set<ProjectMiniature>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Faction>()
            .HasIndex(f => new { f.GameId, f.Name })
            .IsUnique();

        modelBuilder.Entity<Unit>()
            .HasIndex(u => new { u.FactionId, u.Name })
            .IsUnique();

        modelBuilder.Entity<ProjectMiniature>()
            .HasKey(pm => new { pm.ProjectId, pm.MiniatureId });

        modelBuilder.Entity<ProjectMiniature>()
            .HasIndex(pm => new { pm.PhaseId, pm.MiniatureId })
            .IsUnique();

        modelBuilder.Entity<ProjectMiniature>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.ProjectMiniatures)
            .HasForeignKey(pm => pm.ProjectId);

        modelBuilder.Entity<ProjectMiniature>()
            .HasOne(pm => pm.Miniature)
            .WithMany()
            .HasForeignKey(pm => pm.MiniatureId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectMiniature>()
            .HasOne(pm => pm.Phase)
            .WithMany()
            .HasForeignKey(pm => pm.PhaseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Phase>()
            .HasOne(p => p.Project)
            .WithMany(p => p.Phases)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void SeedFromFiles(string dataPath)
    {
        var game = Games.FirstOrDefault();
        if (game == null)
        {
            game = new Game { Name = "Warhammer 40K 11th Edition" };
            Games.Add(game);
            SaveChanges();
        }

        var factionsJson = File.ReadAllText(Path.Combine(dataPath, "factions.json"));
        var factionGroups = JsonSerializer.Deserialize<Dictionary<string, List<FactionEntry>>>(factionsJson);
        if (factionGroups == null) return;

        foreach (var (group, factions) in factionGroups)
        {
            foreach (var factionEntry in factions)
            {
                var faction = Factions.FirstOrDefault(f => f.GameId == game.GameId && f.Name == factionEntry.name);
                if (faction == null)
                {
                    faction = new Faction
                    {
                        Name = factionEntry.name,
                        FactionGroup = group,
                        GameId = game.GameId
                    };
                    Factions.Add(faction);
                    SaveChanges();
                }

                var unitsFile = Path.Combine(dataPath, $"units_{factionEntry.slug}.json");
                if (!File.Exists(unitsFile)) continue;

                var unitsJson = File.ReadAllText(unitsFile);
                var unitsData = JsonSerializer.Deserialize<UnitsFile>(unitsJson);
                if (unitsData?.units == null) continue;

                var existingNames = Units
                    .Where(u => u.FactionId == faction.FactionId)
                    .Select(u => u.Name)
                    .ToHashSet();

                var newUnits = unitsData.units
                    .Where(u => !existingNames.Contains(u.name))
                    .Select(u => new Unit
                    {
                        Name = u.name,
                        Category = u.category,
                        Points = int.TryParse(u.points, out var pts) ? pts : 0,
                        FactionId = faction.FactionId
                    })
                    .ToList();

                if (newUnits.Count > 0)
                {
                    Units.AddRange(newUnits);
                    SaveChanges();
                }
            }
        }
    }

    public record FactionEntry(string name, string url, string slug);
    public record UnitsFile(string faction, string slug, int units_count, List<UnitEntry>? units);
    public record UnitEntry(string name, string category, string url, string slug, string points);
}
