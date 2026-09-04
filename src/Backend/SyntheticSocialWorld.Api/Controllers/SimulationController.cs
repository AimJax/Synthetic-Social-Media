using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Infrastructure.Data;
using SyntheticSocialWorld.Simulation;

namespace SyntheticSocialWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulationController : ControllerBase
{
    private readonly SyntheticSocialWorldDbContext _context;
    private readonly ISimulationService _simulationService;

    public SimulationController(SyntheticSocialWorldDbContext context, ISimulationService simulationService)
    {
        _context = context;
        _simulationService = simulationService;
    }

    /// <summary>
    /// Get world state
    /// </summary>
    [HttpGet("world")]
    public async Task<ActionResult<World>> GetWorld()
    {
        var world = await _context.Worlds.FirstOrDefaultAsync();
        if (world == null)
            return NotFound();

        return Ok(world);
    }

    /// <summary>
    /// Pause/resume simulation
    /// </summary>
    [HttpPut("world/pause")]
    public async Task<ActionResult> TogglePause([FromBody] TogglePauseDto dto)
    {
        var world = await _context.Worlds.FirstOrDefaultAsync();
        if (world == null)
            return NotFound();

        world.IsPaused = dto.IsPaused;
        await _context.SaveChangesAsync();

        return Ok(new { isPaused = world.IsPaused });
    }

    /// <summary>
    /// Advance simulation time
    /// </summary>
    [HttpPost("advance")]
    public async Task<ActionResult> AdvanceTime([FromBody] AdvanceTimeDto dto)
    {
        var world = await _context.Worlds.FirstOrDefaultAsync();
        if (world == null)
            return NotFound();

        var delta = TimeSpan.FromMinutes(dto.Minutes);
        await _simulationService.AdvanceTimeAsync(delta);

        return Ok(new { currentTime = world.CurrentTime });
    }

    /// <summary>
    /// Process a single NPC
    /// </summary>
    [HttpPost("process/{npcId}")]
    public async Task<ActionResult> ProcessNpc(string npcId)
    {
        await _simulationService.ProcessNpcAsync(npcId);
        return Ok(new { message = "NPC processed" });
    }

    /// <summary>
    /// Simulate offline time (batch processing)
    /// </summary>
    [HttpPost("simulate-offline")]
    public async Task<ActionResult> SimulateOffline([FromBody] SimulateOfflineDto dto)
    {
        var duration = TimeSpan.FromMinutes(dto.Minutes);
        await _simulationService.SimulateOfflineAsync(duration);

        return Ok(new { message = $"Simulated {dto.Minutes} minutes" });
    }

    /// <summary>
    /// Get simulation statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var world = await _context.Worlds.FirstOrDefaultAsync();
        if (world == null)
            return NotFound();

        var npcCount = await _context.NPCs.CountAsync();
        var postCount = await _context.Posts.CountAsync(p => !p.IsDeleted);
        var commentCount = await _context.Comments.CountAsync(c => !c.IsDeleted);
        var messageCount = await _context.Messages.CountAsync();
        var communityCount = await _context.Communities.CountAsync();
        var relationshipCount = await _context.NPCRelationships.CountAsync();
        var actionCount = await _context.ScheduledActions.CountAsync(a => !a.IsExecuted);
        var memoryCount = await _context.EpisodicMemories.CountAsync();
        
        // Get engagement totals separately to avoid issues
        var posts = await _context.Posts.Where(p => !p.IsDeleted).Select(p => new { p.LikeCount, p.CommentCount, p.ViewCount }).ToListAsync();
        var totalLikes = posts.Sum(p => p.LikeCount);
        var totalComments = posts.Sum(p => p.CommentCount);
        var totalViews = posts.Sum(p => p.ViewCount);

        var stats = new
        {
            world = new { world.Name, world.CurrentTime, world.IsPaused },
            counts = new
            {
                npcs = npcCount,
                posts = postCount,
                comments = commentCount,
                messages = messageCount,
                communities = communityCount,
                relationships = relationshipCount,
                pendingActions = actionCount,
                memories = memoryCount
            },
            engagement = new
            {
                totalLikes = totalLikes,
                totalComments = totalComments,
                totalViews = totalViews
            }
        };

        return Ok(stats);
    }

    /// <summary>
    /// Get scheduled actions
    /// </summary>
    [HttpGet("scheduled-actions")]
    public async Task<ActionResult<IEnumerable<ScheduledAction>>> GetScheduledActions([FromQuery] int limit = 50)
    {
        var actions = await _context.ScheduledActions
            .Where(a => a.IsExecuted == false)
            .Take(limit)
            .Include(a => a.NPC)
            .ToListAsync();

        // Order on client side
        actions = actions.OrderBy(a => a.Priority).ToList();

        return Ok(actions);
    }

    /// <summary>
    /// Create a scheduled action
    /// </summary>
    [HttpPost("scheduled-actions")]
    public async Task<ActionResult<ScheduledAction>> CreateScheduledAction([FromBody] CreateScheduledActionDto dto)
    {
        var action = new ScheduledAction
        {
            NPCId = dto.NPCId,
            ActionType = dto.ActionType,
            ScheduledFor = dto.ScheduledFor,
            Priority = dto.Priority,
            Parameters = dto.Parameters
        };

        _context.ScheduledActions.Add(action);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetScheduledActions), action);
    }
}

public class TogglePauseDto
{
    public bool IsPaused { get; set; }
}

public class AdvanceTimeDto
{
    public double Minutes { get; set; } = 1;
}

public class SimulateOfflineDto
{
    public double Minutes { get; set; } = 60;
}

public class CreateScheduledActionDto
{
    public string NPCId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public DateTimeOffset ScheduledFor { get; set; }
    public int Priority { get; set; } = 50;
    public string? Parameters { get; set; }
}
