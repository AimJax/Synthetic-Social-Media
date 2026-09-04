using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialController : ControllerBase
{
    private readonly SyntheticSocialWorldDbContext _context;

    public SocialController(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    #region Follows

    /// <summary>
    /// Follow an NPC
    /// </summary>
    [HttpPost("follow")]
    public async Task<ActionResult> Follow([FromBody] FollowDto dto)
    {
        var existing = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == dto.FollowerId && f.FollowedId == dto.FollowedId);

        if (existing != null)
            return BadRequest(new { message = "Already following" });

        var follow = new Follow
        {
            FollowerId = dto.FollowerId,
            FollowedId = dto.FollowedId
        };

        _context.Follows.Add(follow);

        // Update relationship if exists
        var relationship = await _context.NPCRelationships
            .FirstOrDefaultAsync(r => r.SourceNpcId == dto.FollowerId && r.TargetNpcId == dto.FollowedId);
        
        if (relationship != null)
        {
            relationship.Trust = Math.Min(1.0, relationship.Trust + 0.1);
        }
        else
        {
            // Create new relationship
            relationship = new NPCRelationship
            {
                SourceNpcId = dto.FollowerId,
                TargetNpcId = dto.FollowedId,
                Trust = 0.3
            };
            _context.NPCRelationships.Add(relationship);
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Followed successfully" });
    }

    /// <summary>
    /// Unfollow an NPC
    /// </summary>
    [HttpDelete("unfollow")]
    public async Task<ActionResult> Unfollow([FromBody] FollowDto dto)
    {
        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == dto.FollowerId && f.FollowedId == dto.FollowedId);

        if (follow == null)
            return NotFound();

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Check if following
    /// </summary>
    [HttpGet("following/{followerId}/{followedId}")]
    public async Task<ActionResult<bool>> IsFollowing(string followerId, string followedId)
    {
        var isFollowing = await _context.Follows
            .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);

        return Ok(isFollowing);
    }

    #endregion

    #region Relationships

    /// <summary>
    /// Get relationship between two NPCs
    /// </summary>
    [HttpGet("relationship/{sourceId}/{targetId}")]
    public async Task<ActionResult<NPCRelationship>> GetRelationship(string sourceId, string targetId)
    {
        var relationship = await _context.NPCRelationships
            .Include(r => r.SourceNpc)
            .Include(r => r.TargetNpc)
            .FirstOrDefaultAsync(r => r.SourceNpcId == sourceId && r.TargetNpcId == targetId);

        if (relationship == null)
            return NotFound();

        return Ok(relationship);
    }

    /// <summary>
    /// Update relationship dimensions
    /// </summary>
    [HttpPut("relationship")]
    public async Task<ActionResult<NPCRelationship>> UpdateRelationship([FromBody] UpdateRelationshipDto dto)
    {
        var relationship = await _context.NPCRelationships
            .FirstOrDefaultAsync(r => r.SourceNpcId == dto.SourceId && r.TargetNpcId == dto.TargetId);

        if (relationship == null)
        {
            relationship = new NPCRelationship
            {
                SourceNpcId = dto.SourceId,
                TargetNpcId = dto.TargetId
            };
            _context.NPCRelationships.Add(relationship);
        }

        // Update provided values
        if (dto.Affinity.HasValue) relationship.Affinity = dto.Affinity.Value;
        if (dto.Trust.HasValue) relationship.Trust = dto.Trust.Value;
        if (dto.Respect.HasValue) relationship.Respect = dto.Respect.Value;
        if (dto.Attraction.HasValue) relationship.Attraction = dto.Attraction.Value;
        if (dto.Hostility.HasValue) relationship.Hostility = dto.Hostility.Value;
        if (dto.Familiarity.HasValue) relationship.Familiarity = dto.Familiarity.Value;
        if (dto.MutualConnection.HasValue) relationship.MutualConnection = dto.MutualConnection.Value;

        await _context.SaveChangesAsync();
        return Ok(relationship);
    }

    /// <summary>
    /// Get all relationships for an NPC
    /// </summary>
    [HttpGet("relationships/{npcId}")]
    public async Task<ActionResult<IEnumerable<NPCRelationship>>> GetRelationships(string npcId)
    {
        var relationships = await _context.NPCRelationships
            .Include(r => r.TargetNpc)
            .Where(r => r.SourceNpcId == npcId)
            .ToListAsync();

        return Ok(relationships);
    }

    #endregion

    #region Messages

    /// <summary>
    /// Get conversation between two NPCs
    /// </summary>
    [HttpGet("messages/{userId1}/{userId2}")]
    public async Task<ActionResult<IEnumerable<Message>>> GetConversation(string userId1, string userId2, 
        [FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        // Fetch all matching messages and sort in memory (SQLite workaround for DateTimeOffset)
        var allMessages = await _context.Messages
            .Where(m => (m.SenderId == userId1 && m.RecipientId == userId2) ||
                        (m.SenderId == userId2 && m.RecipientId == userId1))
            .ToListAsync();
        
        // Order by CreatedAt in memory
        var messages = allMessages
            .OrderByDescending(m => m.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        return Ok(messages);
    }

    /// <summary>
    /// Send a message
    /// </summary>
    [HttpPost("messages")]
    public async Task<ActionResult<Message>> SendMessage([FromBody] SendMessageDto dto)
    {
        var message = new Message
        {
            SenderId = dto.SenderId,
            RecipientId = dto.RecipientId,
            Content = dto.Content
        };

        _context.Messages.Add(message);

        // Create notification
        var notification = new Notification
        {
            RecipientId = dto.RecipientId,
            Type = "message",
            Title = "New message",
            Body = $"You have a new message from {dto.SenderId}"
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetConversation), 
            new { userId1 = dto.SenderId, userId2 = dto.RecipientId }, message);
    }

    /// <summary>
    /// Mark message as read
    /// </summary>
    [HttpPut("messages/{id}/read")]
    public async Task<ActionResult> MarkAsRead(string id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
            return NotFound();

        message.IsRead = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    #endregion

    #region Notifications

    /// <summary>
    /// Get notifications for a user
    /// </summary>
    [HttpGet("notifications/{userId}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications(string userId,
        [FromQuery] bool unreadOnly = false, [FromQuery] int limit = 50)
    {
        var query = _context.Notifications.Where(n => n.RecipientId == userId);
        
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        // Order on client side to avoid SQLite DateTimeOffset issues
        var notifications = await query
            .Take(limit * 2) // Get more to account for in-memory filtering
            .ToListAsync();
        
        notifications = notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToList();

        return Ok(notifications);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPut("notifications/{id}/read")]
    public async Task<ActionResult> MarkNotificationRead(string id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("notifications/{userId}/read-all")]
    public async Task<ActionResult> MarkAllNotificationsRead(string userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
            notification.IsRead = true;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    #endregion
}

public class FollowDto
{
    public string FollowerId { get; set; } = string.Empty;
    public string FollowedId { get; set; } = string.Empty;
}

public class UpdateRelationshipDto
{
    public string SourceId { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public double? Affinity { get; set; }
    public double? Trust { get; set; }
    public double? Respect { get; set; }
    public double? Attraction { get; set; }
    public double? Hostility { get; set; }
    public double? Familiarity { get; set; }
    public double? MutualConnection { get; set; }
}

public class SendMessageDto
{
    public string SenderId { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
