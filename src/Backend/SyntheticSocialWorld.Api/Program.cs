using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Infrastructure.Data;
using SyntheticSocialWorld.Infrastructure.Repositories;
using SyntheticSocialWorld.Domain.Interfaces;
using SyntheticSocialWorld.Simulation;
using SyntheticSocialWorld.Simulation.Services;
using SyntheticSocialWorld.Api.Services;
using System.Text.Json.Serialization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure to use D: drive for data
var dataPath = Path.Combine("D:", "SyntheticSocialWorld", "data");
Directory.CreateDirectory(dataPath);
var dbPath = Path.Combine(dataPath, "synthetic_social_world.db");

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure SQLite
builder.Services.AddDbContext<SyntheticSocialWorldDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath};Mode=ReadWriteCreate");
});

// Register repositories
builder.Services.AddScoped<INpcRepository, NpcRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IWorldRepository, WorldRepository>();
builder.Services.AddScoped<ICommunityRepository, CommunityRepository>();
builder.Services.AddScoped<IRelationshipRepository, RelationshipRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IScheduledActionRepository, ScheduledActionRepository>();
builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();

// Register AI Provider (Ollama) - falls back gracefully if unavailable
builder.Services.AddSingleton<IAIProvider>(sp =>
{
    return new OllamaAIProvider("http://localhost:11434", "qwen3:4b");
});

// Register Feed Ranking Service
builder.Services.AddSingleton<FeedRankingService>();

// Register Simulation Services
builder.Services.AddSingleton<SocialContagionService>();
builder.Services.AddSingleton<RumorPropagationService>();
builder.Services.AddSingleton<RelationshipEvolutionService>();
builder.Services.AddSingleton<CatchupSummaryService>();
builder.Services.AddSingleton<MemoryDecayService>();
builder.Services.AddSingleton<ConflictDramaService>();

// Register WebSocket Service
builder.Services.AddSingleton<WebSocketService>();

// Register application services with AI provider
builder.Services.AddScoped<ISimulationService>(sp =>
{
    var npcRepo = sp.GetRequiredService<INpcRepository>();
    var postRepo = sp.GetRequiredService<IPostRepository>();
    var worldRepo = sp.GetRequiredService<IWorldRepository>();
    var relationshipRepo = sp.GetRequiredService<IRelationshipRepository>();
    var scheduledActionRepo = sp.GetRequiredService<IScheduledActionRepository>();
    var aiProvider = sp.GetService<IAIProvider>();
    
    return new SimulationService(
        npcRepo,
        postRepo,
        worldRepo,
        relationshipRepo,
        scheduledActionRepo,
        aiProvider);
});

// Configure Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SyntheticSocialWorldDbContext>();
    
    // Enable WAL mode
    context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    context.Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");
    context.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
    
    // Ensure database is created
    context.Database.EnsureCreated();
    
    // Initialize default world if needed
    var worldRepo = scope.ServiceProvider.GetRequiredService<IWorldRepository>();
    await worldRepo.EnsureDefaultWorldExistsAsync();
}

app.UseRouting();
app.UseAuthorization();
app.UseWebSockets();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

// Debug metrics endpoint
app.MapGet("/debug/metrics", async (SyntheticSocialWorldDbContext context) =>
{
    return Results.Ok(new
    {
        npcCount = await context.NPCs.CountAsync(),
        postCount = await context.Posts.CountAsync(),
        communityCount = await context.Communities.CountAsync(),
        relationshipCount = await context.NPCRelationships.CountAsync(),
        memoryCount = await context.EpisodicMemories.CountAsync(),
        scheduledActionCount = await context.ScheduledActions.CountAsync(s => !s.IsExecuted)
    });
});

// API info endpoint
app.MapGet("/api/info", () => Results.Ok(new 
{ 
    name = "Synthetic Social World API", 
    version = "1.0.0",
    description = "A persistent AI social network simulation platform"
}));

app.Run();
