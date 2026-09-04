using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Domain.Interfaces;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NPCsController : ControllerBase
{
    private readonly SyntheticSocialWorldDbContext _context;
    private readonly INpcRepository _npcRepository;

    public NPCsController(SyntheticSocialWorldDbContext context, INpcRepository npcRepository)
    {
        _context = context;
        _npcRepository = npcRepository;
    }

    /// <summary>
    /// Get all NPCs
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NPCDto>>> GetAll([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        var npcs = await _context.NPCs
            .Where(n => !n.IsPlayer)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        // Order on client side to avoid SQLite DateTimeOffset issues
        npcs = npcs.OrderByDescending(n => n.LastActiveAt).ToList();

        // Load related data separately
        var npcIds = npcs.Select(n => n.Id).ToList();
        
        var personalities = await _context.NPCPersonalities
            .Where(p => npcIds.Contains(p.NPCId))
            .ToListAsync();
            
        var moods = await _context.NPCMoods
            .Where(m => npcIds.Contains(m.NPCId))
            .ToListAsync();
            
        var interests = await _context.NPCInterests
            .Where(i => npcIds.Contains(i.NPCId))
            .ToListAsync();

        var result = npcs.Select(n => new NPCDto
        {
            Id = n.Id,
            Handle = n.Handle,
            DisplayName = n.DisplayName,
            Bio = n.Bio,
            IsPlayer = n.IsPlayer,
            ActivityLevel = n.ActivityLevel,
            Reputation = n.Reputation,
            Popularity = n.Popularity,
            FollowerCount = n.FollowerCount,
            FollowingCount = n.FollowingCount,
            LastActiveAt = n.LastActiveAt,
            CreatedAt = n.CreatedAt,
            Personality = personalities.FirstOrDefault(p => p.NPCId == n.Id) != null
                ? new PersonalityDto
                {
                    Openness = personalities.First(p => p.NPCId == n.Id).Openness,
                    Extroversion = personalities.First(p => p.NPCId == n.Id).Extroversion,
                    Agreeableness = personalities.First(p => p.NPCId == n.Id).Agreeableness,
                    Conscientiousness = personalities.First(p => p.NPCId == n.Id).Conscientiousness,
                    Neuroticism = personalities.First(p => p.NPCId == n.Id).Neuroticism,
                    Confidence = personalities.First(p => p.NPCId == n.Id).Confidence,
                    Empathy = personalities.First(p => p.NPCId == n.Id).Empathy,
                    Sarcasm = personalities.First(p => p.NPCId == n.Id).Sarcasm,
                    Humor = personalities.First(p => p.NPCId == n.Id).Humor,
                    Aggression = personalities.First(p => p.NPCId == n.Id).Aggression
                }
                : null,
            Mood = moods.FirstOrDefault(m => m.NPCId == n.Id) != null
                ? new MoodDto
                {
                    Happiness = moods.First(m => m.NPCId == n.Id).Happiness,
                    Sadness = moods.First(m => m.NPCId == n.Id).Sadness,
                    Anger = moods.First(m => m.NPCId == n.Id).Anger,
                    Excitement = moods.First(m => m.NPCId == n.Id).Excitement,
                    Anxiety = moods.First(m => m.NPCId == n.Id).Anxiety,
                    PrimaryMood = moods.First(m => m.NPCId == n.Id).PrimaryMood
                }
                : null,
            Interests = interests.Where(i => i.NPCId == n.Id)
                .Select(i => new InterestDataDto { Topic = i.Topic, Weight = i.Weight })
                .ToList()
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Get NPC by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<NPCDto>> GetById(string id)
    {
        var npc = await _npcRepository.GetByIdAsync(id);
        if (npc == null)
            return NotFound();

        return Ok(ToDto(npc));
    }

    /// <summary>
    /// Get NPC by handle
    /// </summary>
    [HttpGet("by-handle/{handle}")]
    public async Task<ActionResult<NPCDto>> GetByHandle(string handle)
    {
        var npc = await _npcRepository.GetByHandleAsync(handle);
        if (npc == null)
            return NotFound();

        return Ok(ToDto(npc));
    }

    /// <summary>
    /// Get NPC's followers
    /// </summary>
    [HttpGet("{id}/followers")]
    public async Task<ActionResult<IEnumerable<NPCSummaryDto>>> GetFollowers(string id)
    {
        var followers = await _context.Follows
            .Where(f => f.FollowedId == id)
            .Include(f => f.Follower)
            .Select(f => f.Follower)
            .ToListAsync();

        return Ok(followers.Select(n => new NPCSummaryDto { Id = n.Id, Handle = n.Handle, DisplayName = n.DisplayName }));
    }

    /// <summary>
    /// Get NPCs that this NPC follows
    /// </summary>
    [HttpGet("{id}/following")]
    public async Task<ActionResult<IEnumerable<NPCSummaryDto>>> GetFollowing(string id)
    {
        var following = await _context.Follows
            .Where(f => f.FollowerId == id)
            .Include(f => f.Followed)
            .Select(f => f.Followed)
            .ToListAsync();

        return Ok(following.Select(n => new NPCSummaryDto { Id = n.Id, Handle = n.Handle, DisplayName = n.DisplayName }));
    }

    /// <summary>
    /// Get NPC's posts
    /// </summary>
    [HttpGet("{id}/posts")]
    public async Task<ActionResult<IEnumerable<PostSummaryDto>>> GetPosts(string id, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        var posts = await _context.Posts
            .Where(p => p.AuthorId == id && !p.IsDeleted)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        // Order on client side
        posts = posts.OrderByDescending(p => p.CreatedAt).ToList();

        return Ok(posts.Select(p => new PostSummaryDto
        {
            Id = p.Id,
            AuthorId = p.AuthorId,
            AuthorName = "Unknown",
            Content = p.Content,
            LikeCount = p.LikeCount,
            CommentCount = p.CommentCount,
            CreatedAt = p.CreatedAt
        }));
    }

    /// <summary>
    /// Get NPC's relationships
    /// </summary>
    [HttpGet("{id}/relationships")]
    public async Task<ActionResult<object>> GetRelationships(string id)
    {
        var outgoing = await _context.NPCRelationships
            .Where(r => r.SourceNpcId == id)
            .Include(r => r.TargetNpc)
            .ToListAsync();

        var incoming = await _context.NPCRelationships
            .Where(r => r.TargetNpcId == id)
            .Include(r => r.SourceNpc)
            .ToListAsync();

        return Ok(new 
        { 
            outgoing = outgoing.Select(r => new { r.Id, r.TargetNpcId, r.TargetNpc?.Handle, r.TargetNpc?.DisplayName, r.Affinity, r.Trust }),
            incoming = incoming.Select(r => new { r.Id, r.SourceNpcId, r.SourceNpc?.Handle, r.SourceNpc?.DisplayName, r.Affinity, r.Trust })
        });
    }

    /// <summary>
    /// Update NPC mood
    /// </summary>
    [HttpPut("{id}/mood")]
    public async Task<ActionResult<MoodDto>> UpdateMood(string id, [FromBody] MoodUpdateDto update)
    {
        var npc = await _npcRepository.GetByIdAsync(id);
        if (npc == null)
            return NotFound();

        var mood = await _context.NPCMoods.FirstOrDefaultAsync(m => m.NPCId == id);
        if (mood == null)
        {
            mood = new Mood { NPCId = id };
            _context.NPCMoods.Add(mood);
        }

        mood.Happiness = update.Happiness;
        mood.Sadness = update.Sadness;
        mood.Anger = update.Anger;
        mood.Excitement = update.Excitement;
        mood.Anxiety = update.Anxiety;
        mood.PrimaryMood = update.PrimaryMood ?? mood.PrimaryMood;

        await _context.SaveChangesAsync();
        
        return Ok(new MoodDto
        {
            Happiness = mood.Happiness,
            Sadness = mood.Sadness,
            Anger = mood.Anger,
            Excitement = mood.Excitement,
            Anxiety = mood.Anxiety,
            PrimaryMood = mood.PrimaryMood
        });
    }

    /// <summary>
    /// Trigger NPC processing
    /// </summary>
    [HttpPost("{id}/process")]
    public async Task<ActionResult> Process(string id)
    {
        var simulationService = HttpContext.RequestServices.GetRequiredService<Simulation.ISimulationService>();
        await simulationService.ProcessNpcAsync(id);
        return Ok(new { message = "NPC processed" });
    }

    private NPCDto ToDto(NPC npc)
    {
        return new NPCDto
        {
            Id = npc.Id,
            Handle = npc.Handle,
            DisplayName = npc.DisplayName,
            Bio = npc.Bio,
            IsPlayer = npc.IsPlayer,
            ActivityLevel = npc.ActivityLevel,
            Reputation = npc.Reputation,
            Popularity = npc.Popularity,
            FollowerCount = npc.FollowerCount,
            FollowingCount = npc.FollowingCount,
            LastActiveAt = npc.LastActiveAt,
            CreatedAt = npc.CreatedAt,
            Personality = npc.Personality != null ? new PersonalityDto
            {
                Openness = npc.Personality.Openness,
                Extroversion = npc.Personality.Extroversion,
                Agreeableness = npc.Personality.Agreeableness,
                Conscientiousness = npc.Personality.Conscientiousness,
                Neuroticism = npc.Personality.Neuroticism,
                Confidence = npc.Personality.Confidence,
                Empathy = npc.Personality.Empathy,
                Sarcasm = npc.Personality.Sarcasm,
                Humor = npc.Personality.Humor,
                Aggression = npc.Personality.Aggression
            } : null,
            Mood = npc.Mood != null ? new MoodDto
            {
                Happiness = npc.Mood.Happiness,
                Sadness = npc.Mood.Sadness,
                Anger = npc.Mood.Anger,
                Excitement = npc.Mood.Excitement,
                Anxiety = npc.Mood.Anxiety,
                PrimaryMood = npc.Mood.PrimaryMood
            } : null,
            Interests = npc.Interests?.Select(i => new InterestDataDto { Topic = i.Topic, Weight = i.Weight }).ToList()
        };
    }
}

// DTOs
public class NPCDto
{
    public string Id { get; set; } = "";
    public string Handle { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Bio { get; set; }
    public bool IsPlayer { get; set; }
    public double ActivityLevel { get; set; }
    public double Reputation { get; set; }
    public double Popularity { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public DateTimeOffset LastActiveAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public PersonalityDto? Personality { get; set; }
    public MoodDto? Mood { get; set; }
    public List<InterestDataDto>? Interests { get; set; }
}

public class NPCSummaryDto
{
    public string Id { get; set; } = "";
    public string Handle { get; set; } = "";
    public string DisplayName { get; set; } = "";
}

public class PersonalityDto
{
    public double Openness { get; set; }
    public double Extroversion { get; set; }
    public double Agreeableness { get; set; }
    public double Conscientiousness { get; set; }
    public double Neuroticism { get; set; }
    public double Confidence { get; set; }
    public double Empathy { get; set; }
    public double Sarcasm { get; set; }
    public double Humor { get; set; }
    public double Aggression { get; set; }
}

public class MoodDto
{
    public double Happiness { get; set; }
    public double Sadness { get; set; }
    public double Anger { get; set; }
    public double Excitement { get; set; }
    public double Anxiety { get; set; }
    public string PrimaryMood { get; set; } = "neutral";
}

public class InterestDataDto
{
    public string Topic { get; set; } = "";
    public double Weight { get; set; }
}

public class PostSummaryDto
{
    public string Id { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string Content { get; set; } = "";
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class MoodUpdateDto
{
    public double Happiness { get; set; }
    public double Sadness { get; set; }
    public double Anger { get; set; }
    public double Excitement { get; set; }
    public double Anxiety { get; set; }
    public string? PrimaryMood { get; set; }
}
