using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Api.Controllers;

[ApiController]
[Route("api/player")]
public class PlayerController : ControllerBase
{
    private readonly SyntheticSocialWorldDbContext _context;
    private readonly ILogger<PlayerController> _logger;

    public PlayerController(
        SyntheticSocialWorldDbContext context,
        ILogger<PlayerController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get the current player (requires PlayerId header).
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<CurrentPlayerDto>> GetCurrentPlayer(
        [FromHeader(Name = "X-Player-Id")] string? playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return Unauthorized(new { error = "X-Player-Id header is required" });
        }

        var player = await _context.Players
            .Include(p => p.Interests)
            .Include(p => p.Posts)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player == null)
        {
            return NotFound(new { error = "Player not found" });
        }

        // Update last active time
        player.LastActiveAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        var unreadCount = await _context.Notifications
            .CountAsync(n => n.RecipientId == playerId && 
                            n.RecipientType == AuthorType.Player && 
                            !n.IsRead);

        return Ok(new CurrentPlayerDto
        {
            Id = player.Id,
            Handle = player.Handle,
            DisplayName = player.DisplayName,
            Bio = player.Bio,
            AvatarUrl = player.AvatarUrl,
            FollowerCount = player.FollowerCount,
            FollowingCount = player.FollowingCount,
            Reputation = player.Reputation,
            CreatedAt = player.CreatedAt,
            LastActiveAt = player.LastActiveAt,
            Interests = player.Interests.Select(i => i.Topic).ToList(),
            PostCount = player.Posts.Count(p => !p.IsDeleted),
            UnreadNotificationCount = unreadCount
        });
    }

    /// <summary>
    /// Check if a player exists.
    /// </summary>
    [HttpGet("exists")]
    public async Task<ActionResult> CheckPlayerExists(
        [FromHeader(Name = "X-Player-Id")] string? playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return Ok(new { exists = false });
        }

        var exists = await _context.Players.AnyAsync(p => p.Id == playerId);
        return Ok(new { exists });
    }

    /// <summary>
    /// Create a new player.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CurrentPlayerDto>> CreatePlayer([FromBody] CreatePlayerRequest request)
    {
        // Validate handle uniqueness
        var existingPlayer = await _context.Players
            .AnyAsync(p => p.Handle.ToLower() == request.Handle.ToLower());
        
        if (existingPlayer)
        {
            return Conflict(new { error = "Handle already taken" });
        }

        // Check for existing NPC with same handle
        var existingNpc = await _context.NPCs
            .AnyAsync(n => n.Handle.ToLower() == request.Handle.ToLower());
        
        if (existingNpc)
        {
            return Conflict(new { error = "Handle already taken by an NPC" });
        }

        // Get or create world
        var world = await _context.Worlds.FirstOrDefaultAsync();
        if (world == null)
        {
            world = new World { Name = "Synthetic Social World" };
            _context.Worlds.Add(world);
            await _context.SaveChangesAsync();
        }

        // Create player
        var player = new Player
        {
            Handle = request.Handle,
            DisplayName = request.DisplayName,
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl,
            WorldId = world.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            LastActiveAt = DateTimeOffset.UtcNow
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        // Add interests
        if (request.Interests != null && request.Interests.Any())
        {
            foreach (var interest in request.Interests)
            {
                _context.PlayerInterests.Add(new PlayerInterest
                {
                    PlayerId = player.Id,
                    Topic = interest,
                    Weight = 0.5
                });
            }
            await _context.SaveChangesAsync();
        }

        // Reload with interests
        player = await _context.Players
            .Include(p => p.Interests)
            .FirstAsync(p => p.Id == player.Id);

        _logger.LogInformation("Created new player: {Handle} ({Id})", player.Handle, player.Id);

        return CreatedAtAction(nameof(GetCurrentPlayer), new CurrentPlayerDto
        {
            Id = player.Id,
            Handle = player.Handle,
            DisplayName = player.DisplayName,
            Bio = player.Bio,
            AvatarUrl = player.AvatarUrl,
            FollowerCount = 0,
            FollowingCount = 0,
            Reputation = 50.0,
            CreatedAt = player.CreatedAt,
            LastActiveAt = player.LastActiveAt,
            Interests = player.Interests.Select(i => i.Topic).ToList(),
            PostCount = 0,
            UnreadNotificationCount = 0
        });
    }

    /// <summary>
    /// Update the current player's profile.
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<CurrentPlayerDto>> UpdatePlayer(
        [FromHeader(Name = "X-Player-Id")] string? playerId,
        [FromBody] UpdatePlayerRequest request)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return Unauthorized(new { error = "X-Player-Id header is required" });
        }

        var player = await _context.Players
            .Include(p => p.Interests)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player == null)
        {
            return NotFound(new { error = "Player not found" });
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.DisplayName))
        {
            player.DisplayName = request.DisplayName;
        }

        if (request.Bio != null)
        {
            player.Bio = request.Bio;
        }

        if (request.AvatarUrl != null)
        {
            player.AvatarUrl = request.AvatarUrl;
        }

        // Update interests if provided
        if (request.Interests != null)
        {
            // Remove existing interests
            var existingInterests = await _context.PlayerInterests
                .Where(i => i.PlayerId == playerId)
                .ToListAsync();
            _context.PlayerInterests.RemoveRange(existingInterests);

            // Add new interests
            foreach (var interest in request.Interests)
            {
                _context.PlayerInterests.Add(new PlayerInterest
                {
                    PlayerId = player.Id,
                    Topic = interest,
                    Weight = 0.5
                });
            }
        }

        await _context.SaveChangesAsync();

        // Reload to get updated interests
        player = await _context.Players
            .Include(p => p.Interests)
            .Include(p => p.Posts)
            .FirstAsync(p => p.Id == playerId);

        var unreadCount = await _context.Notifications
            .CountAsync(n => n.RecipientId == playerId && 
                            n.RecipientType == AuthorType.Player && 
                            !n.IsRead);

        _logger.LogInformation("Updated player profile: {Id}", player.Id);

        return Ok(new CurrentPlayerDto
        {
            Id = player.Id,
            Handle = player.Handle,
            DisplayName = player.DisplayName,
            Bio = player.Bio,
            AvatarUrl = player.AvatarUrl,
            FollowerCount = player.FollowerCount,
            FollowingCount = player.FollowingCount,
            Reputation = player.Reputation,
            CreatedAt = player.CreatedAt,
            LastActiveAt = player.LastActiveAt,
            Interests = player.Interests.Select(i => i.Topic).ToList(),
            PostCount = player.Posts.Count(p => !p.IsDeleted),
            UnreadNotificationCount = unreadCount
        });
    }

    /// <summary>
    /// Get the current player's posts.
    /// </summary>
    [HttpGet("me/posts")]
    public async Task<ActionResult<List<PlayerPostDto>>> GetMyPosts(
        [FromHeader(Name = "X-Player-Id")] string? playerId,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return Unauthorized(new { error = "X-Player-Id header is required" });
        }

        var posts = await _context.Posts
            .Where(p => p.AuthorId == playerId && 
                        p.AuthorType == AuthorType.Player && 
                        !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        var dtos = posts.Select(p => new PlayerPostDto
        {
            Id = p.Id,
            Content = p.Content,
            AuthorId = p.AuthorId,
            AuthorName = "You",
            AuthorHandle = "me",
            LikeCount = p.LikeCount,
            CommentCount = p.CommentCount,
            CreatedAt = p.CreatedAt.ToString("o"),
            AuthorType = "player"
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Create a post as the current player.
    /// </summary>
    [HttpPost("me/posts")]
    public async Task<ActionResult<PlayerPostDto>> CreatePost(
        [FromHeader(Name = "X-Player-Id")] string? playerId,
        [FromBody] CreatePostForPlayerRequest request)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return Unauthorized(new { error = "X-Player-Id header is required" });
        }

        var player = await _context.Players.FindAsync(playerId);
        if (player == null)
        {
            return NotFound(new { error = "Player not found" });
        }

        var post = new Post
        {
            AuthorId = playerId,
            AuthorType = AuthorType.Player,
            Content = request.Content,
            CommunityId = request.CommunityId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Posts.Add(post);

        // Update player activity
        player.LastActiveAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Player {Id} created post {PostId}", playerId, post.Id);

        return CreatedAtAction(nameof(GetMyPosts), new { }, new PlayerPostDto
        {
            Id = post.Id,
            Content = post.Content,
            AuthorId = post.AuthorId,
            AuthorName = player.DisplayName,
            AuthorHandle = player.Handle,
            LikeCount = 0,
            CommentCount = 0,
            CreatedAt = post.CreatedAt.ToString("o"),
            AuthorType = "player"
        });
    }
}

/// <summary>
/// Request to create a post for the current player.
/// </summary>
public class CreatePostForPlayerRequest
{
    public string Content { get; set; } = string.Empty;
    public string? CommunityId { get; set; }
}

/// <summary>
/// Simplified post DTO for player endpoints.
/// </summary>
public class PlayerPostDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorHandle { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string AuthorType { get; set; } = "npc";
}
