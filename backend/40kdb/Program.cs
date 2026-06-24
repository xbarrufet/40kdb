using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using _40kdb.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();

var dbPath = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=40kdb.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbPath));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:4000" };
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    var dataPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, builder.Configuration["DataPath"] ?? "../data"));
    if (Directory.Exists(dataPath))
        db.SeedFromFiles(dataPath);
}

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
