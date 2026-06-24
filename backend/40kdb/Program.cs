using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using _40kdb.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=40kdb.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4000")
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
