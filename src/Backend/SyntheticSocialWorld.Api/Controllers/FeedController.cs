using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Infrastructure.Data;
using SyntheticSocialWorld.Simulation.Services;

namespace SyntheticSocialWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedController : ControllerBase
{
    private readonly SyntheticSocialWorldDbContext _context;
    private readonly FeedRankingService _rankingService;
    private readonly ILogger<FeedController> _logger;

    public FeedController(
        SyntheticSocialWorldDbContext context, 
        FeedRankingService rankingService,
        ILogger<FeedController> logger)
    {
        _context = context;
        _rankingService = rankingService;
        _logger = logger;
    }

    /// <summary>
    /// Get personalized feed for a user using multi-factor ranking
    /// Returns a plain array for Android compatibility
    /// </summary>
    [HttpGet("{npcId}")]
    public async Task<ActionResult<IEnumerable<FeedPostDto>>> GetFeed(string npcId, [FromQuery] int limit = 20, [FromQuery] string? cursor = null)
    {
        try
        {
            _logger.LogInformation("Getting feed for NPC {NpcId}", npcId);

            // Build player context for ranking
            var context = await BuildPlayerContextAsync(npcId);

            // Get candidate posts
            var candidateCount = Math.Min(limit * 5, 200); // Get more candidates than needed
            var candidates = await GetCandidatePostsAsync(npcId, candidateCount, cursor);

            if (!candidates.Any())
            {
                return Ok(new List<FeedPostDto>());
            }

            // Rank posts using multi-factor scoring
            var rankedPosts = _rankingService.RankPosts(candidates, npcId, context);

            // Apply diversity constraints
            rankedPosts = _rankingService.ApplyDiversity(rankedPosts);

            // Remove seen posts
            rankedPosts = rankedPosts.Where(p => !context.SeenPostIds.Contains(p.Id)).ToList();

            // Take final limit
            var posts = rankedPosts.Take(limit).ToList();

            // Build DTOs and return as plain array
            var result = posts.Select(p => new FeedPostDto
            {
                Id = p.Id,
                AuthorId = p.AuthorId,
                AuthorName = "Unknown",
                AuthorHandle = "",
                AuthorPopularity = 0,
                Content = p.Content,
                LikeCount = p.LikeCount,
                DislikeCount = p.DislikeCount,
                CommentCount = p.CommentCount,
                ShareCount = p.ShareCount,
                ViewCount = p.ViewCount,
                ImportanceScore = p.ImportanceScore,
                Popularity = p.Popularity,
                CreatedAt = p.CreatedAt,
                CommunityId = p.CommunityId,
                CommunityName = p.Community?.Name
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feed for {NpcId}", npcId);
            return StatusCode(500, new { error = "internal_error", message = "Failed to get feed" });
        }
    }

    /// <summary>
    /// Get trending posts (high engagement, recent)
    /// </summary>
    [HttpGet("trending")]
    public async Task<ActionResult<IEnumerable<FeedPostDto>>> GetTrending([FromQuery] int limit = 20)
    {
        var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);
        
        // Fetch all posts first, then filter and order in memory
        var allPosts = await _context.Posts
            .Include(p => p.Community)
            .Take(200)
            .ToListAsync();
        
        // Filter and order in memory
        var posts = allPosts
            .Where(p => p.IsDeleted == false && p.CreatedAt > sevenDaysAgo)
            .OrderByDescending(p => p.LikeCount + p.CommentCount * 2 + p.ShareCount * 3)
            .Take(limit)
            .ToList();

        var result = posts.Select(p => new FeedPostDto
        {
            Id = p.Id,
            AuthorId = p.AuthorId,
            AuthorName = "Unknown",
            AuthorHandle = "",
            AuthorPopularity = 0,
            Content = p.Content,
            LikeCount = p.LikeCount,
            DislikeCount = p.DislikeCount,
            CommentCount = p.CommentCount,
            ShareCount = p.ShareCount,
            ViewCount = p.ViewCount,
            ImportanceScore = p.ImportanceScore,
            Popularity = p.Popularity,
            CreatedAt = p.CreatedAt,
            CommunityId = p.CommunityId,
            CommunityName = p.Community?.Name
        });

        return Ok(result);
    }

    /// <summary>
    /// Get discovery posts (from non-followed users)
    /// </summary>
    [HttpGet("discovery")]
    public async Task<ActionResult<IEnumerable<FeedPostDto>>> GetDiscovery(string npcId, [FromQuery] int limit = 20)
    {
        var followingIds = await _context.Follows
            .Where(f => f.FollowerId == npcId)
            .Select(f => f.FollowedId)
            .ToListAsync();

        // Fetch all posts first, then filter and order in memory
        var allPosts = await _context.Posts
            .Include(p => p.Community)
            .Take(200)
            .ToListAsync();

        // Filter and order in memory
        var posts = allPosts
            .Where(p => p.IsDeleted == false 
                && p.AuthorId != npcId 
                && !followingIds.Contains(p.AuthorId))
            .OrderByDescending(p => p.Popularity)
            .ThenByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToList();

        var result = posts.Select(p => new FeedPostDto
        {
            Id = p.Id,
            AuthorId = p.AuthorId,
            AuthorName = "Unknown",
            AuthorHandle = "",
            AuthorPopularity = 0,
            Content = p.Content,
            LikeCount = p.LikeCount,
            DislikeCount = p.DislikeCount,
            CommentCount = p.CommentCount,
            ShareCount = p.ShareCount,
            ViewCount = p.ViewCount,
            ImportanceScore = p.ImportanceScore,
            Popularity = p.Popularity,
            CreatedAt = p.CreatedAt,
            CommunityId = p.CommunityId,
            CommunityName = p.Community?.Name
        });

        return Ok(result);
    }

    /// <summary>
    /// Force refresh feed cache
    /// </summary>
    [HttpPost("refresh")]
    public IActionResult RefreshFeed()
    {
        // In a real implementation, this would invalidate cached feeds
        return Ok(new { message = "Feed refresh requested" });
    }

    /// <summary>
    /// Get posts from a specific community
    /// </summary>
    [HttpGet("community/{communityId}")]
    public async Task<ActionResult<IEnumerable<FeedPostDto>>> GetCommunityFeed(
        string communityId, 
        [FromQuery] int limit = 20)
    {
        var posts = await _context.Posts
            .Include(p => p.Community)
            .Where(p => !p.IsDeleted && p.CommunityId == communityId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();

        var result = posts.Select(p => new FeedPostDto
        {
            Id = p.Id,
            AuthorId = p.AuthorId,
            AuthorName = "Unknown",
            AuthorHandle = "",
            AuthorPopularity = 0,
            Content = p.Content,
            LikeCount = p.LikeCount,
            DislikeCount = p.DislikeCount,
            CommentCount = p.CommentCount,
            ShareCount = p.ShareCount,
            ViewCount = p.ViewCount,
            ImportanceScore = p.ImportanceScore,
            Popularity = p.Popularity,
            CreatedAt = p.CreatedAt,
            CommunityId = p.CommunityId,
            CommunityName = p.Community?.Name
        });

        return Ok(result);
    }

    /// <summary>
    /// Build player context for feed ranking calculations
    /// </summary>
    private async Task<PlayerFeedContext> BuildPlayerContextAsync(string playerId)
    {
        var context = new PlayerFeedContext
        {
            WorldTime = await GetWorldTimeAsync()
        };

        // Get player relationships
        var relationships = await _context.NPCRelationships
            .Where(r => r.SourceNpcId == playerId)
            .ToListAsync();
        
        foreach (var rel in relationships)
        {
            context.Relationships[rel.TargetNpcId] = new RelationshipDimensions
            {
                Affinity = rel.Affinity,
                Trust = rel.Trust,
                Respect = rel.Respect,
                Hostility = rel.Hostility,
                Jealousy = rel.Jealousy
            };
        }

        // Get player community memberships
        var memberships = await _context.CommunityMembers
            .Where(cm => cm.NPCId == playerId)
            .Select(cm => cm.CommunityId)
            .ToListAsync();
        context.CommunityMemberships = new HashSet<string>(memberships);

        // Get author interests
        var authorIds = await _context.NPCs.Select(n => n.Id).ToListAsync();
        var interests = await _context.NPCInterests
            .Where(i => authorIds.Contains(i.NPCId))
            .ToListAsync();
        
        foreach (var interest in interests)
        {
            if (!context.AuthorInterests.ContainsKey(interest.NPCId))
                context.AuthorInterests[interest.NPCId] = new List<AuthorInterest>();
            
            context.AuthorInterests[interest.NPCId].Add(new AuthorInterest
            {
                Topic = interest.Topic,
                Weight = interest.Weight
            });
        }

        // Get community activity levels
        var communities = await _context.Communities.ToListAsync();
        foreach (var community in communities)
        {
            var memberCount = await _context.CommunityMembers
                .CountAsync(cm => cm.CommunityId == community.Id);
            context.CommunityActivity[community.Id] = memberCount * 10; // Activity = members * 10
        }

        // Get player's interaction history
        var allLikes = await _context.PostEngagements
            .Where(e => e.NPCId == playerId && e.Type == "like")
            .ToListAsync();
        
        var likes = allLikes
            .GroupBy(e => e.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count(), FirstDate = g.Min(l => l.CreatedAt) })
            .ToList();

        // Get posts liked to find authors
        var likedPostIds = likes.Select(l => l.PostId).ToList();
        var likedPosts = await _context.Posts
            .Where(p => likedPostIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.AuthorId);

        foreach (var like in likes)
        {
            if (likedPosts.TryGetValue(like.PostId, out var authorId))
            {
                if (!context.InteractionHistory.ContainsKey(authorId))
                {
                    context.InteractionHistory[authorId] = new InteractionHistory();
                }
                context.InteractionHistory[authorId].InteractionCount += like.Count;
                if (!context.InteractionHistory[authorId].LastInteraction.HasValue || 
                    like.FirstDate < context.InteractionHistory[authorId].LastInteraction)
                {
                    context.InteractionHistory[authorId].LastInteraction = like.FirstDate;
                }
            }
        }

        return context;
    }

    /// <summary>
    /// Get candidate posts for ranking
    /// </summary>
    private async Task<List<Post>> GetCandidatePostsAsync(string playerId, int limit, string? cursor)
    {
        var followingIds = await _context.Follows
            .Where(f => f.FollowerId == playerId)
            .Select(f => f.FollowedId)
            .ToListAsync();

        var communityIds = await _context.CommunityMembers
            .Where(cm => cm.NPCId == playerId)
            .Select(cm => cm.CommunityId)
            .ToListAsync();

        // Fetch all posts and filter in memory (SQLite limitations with Contains)
        var allPosts = await _context.Posts
            .Include(p => p.Community)
            .Where(p => !p.IsDeleted)
            .ToListAsync();
        
        var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);
        DateTimeOffset? cursorTime = null;
        if (!string.IsNullOrEmpty(cursor) && DateTimeOffset.TryParse(cursor, out var parsedCursor))
        {
            cursorTime = parsedCursor;
        }

        // Filter in memory
        var posts = allPosts
            .Where(p => 
                followingIds.Contains(p.AuthorId) ||
                (p.CommunityId != null && communityIds.Contains(p.CommunityId)) ||
                p.AuthorId == playerId)
            .Where(p => p.CreatedAt > sevenDaysAgo)
            .Where(p => cursorTime == null || p.CreatedAt < cursorTime.Value)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToList();

        return posts;
    }

    /// <summary>
    /// Get world time
    /// </summary>
    private async Task<DateTimeOffset> GetWorldTimeAsync()
    {
        var world = await _context.Worlds.FirstOrDefaultAsync();
        return world?.CurrentTime ?? DateTimeOffset.UtcNow;
    }
}

public class FeedResponse
{
    public List<FeedPostDto> Items { get; set; } = new();
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
}

public class FeedPostDto
{
    public string Id { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string AuthorHandle { get; set; } = "";
    public double AuthorPopularity { get; set; }
    public string Content { get; set; } = "";
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public int ViewCount { get; set; }
    public double ImportanceScore { get; set; }
    public double Popularity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CommunityId { get; set; }
    public string? CommunityName { get; set; }
}
