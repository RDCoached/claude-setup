using Claude_Setup.Features.Console;
using Claude_Setup.Features.Skills;
using Claude_Setup.Infrastructure.Configuration;
using Claude_Setup.Infrastructure.FileSystem;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<ClaudePathResolver>();
builder.Services.AddSingleton<FrontmatterParser>();
builder.Services.AddSingleton<ClaudeFileReader>();
builder.Services.AddSingleton<ClaudeFileWriter>();
builder.Services.AddSingleton<ListSkills>();
builder.Services.AddSingleton<InteractiveMenu>();

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Auto-discover and map endpoint groups
app.MapEndpoints();

// Determine run mode
var runApiOnly = args.Contains("--api");

if (runApiOnly)
{
    Console.WriteLine("Running in API-only mode on http://localhost:5100");
    Console.WriteLine("Swagger UI: http://localhost:5100/swagger");
    await app.RunAsync("http://localhost:5100");
}
else
{
    // Run background API + interactive console
    var apiTask = app.RunAsync("http://localhost:5100");

    Console.WriteLine("Claude Configuration Manager");
    Console.WriteLine("API running in background on http://localhost:5100");
    Console.WriteLine("Swagger UI: http://localhost:5100/swagger\n");

    var menu = app.Services.GetRequiredService<InteractiveMenu>();
    var cts = new CancellationTokenSource();

    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    await menu.RunAsync(cts.Token);

    // Shutdown API after console exits
    await app.StopAsync();
}
