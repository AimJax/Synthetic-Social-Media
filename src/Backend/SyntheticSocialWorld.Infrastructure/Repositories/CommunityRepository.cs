using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Domain.Interfaces;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Infrastructure.Repositories;

public class CommunityRepository : ICommunityRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public CommunityRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<Community?> GetByIdAsync(string id)
    {
        return await _context.Communities
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Community?> GetByHandleAsync(string handle)
    {
        return await _context.Communities
            .FirstOrDefaultAsync(c => c.Handle == handle);
    }

    public async Task<IEnumerable<Community>> GetAllAsync(int limit = 50, int offset = 0)
    {
        return await _context.Communities
            .OrderByDescending(c => c.Popularity)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Community>> GetPopularAsync(int count)
    {
        return await _context.Communities
            .OrderByDescending(c => c.Popularity)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Community>> GetByMemberAsync(string npcId)
    {
        return await _context.CommunityMembers
            .Where(cm => cm.NPCId == npcId)
            .Include(cm => cm.Community)
            .Select(cm => cm.Community!)
            .ToListAsync();
    }

    public async Task<Community> AddAsync(Community community)
    {
        _context.Communities.Add(community);
        await _context.SaveChangesAsync();
        return community;
    }

    public async Task UpdateAsync(Community community)
    {
        community.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Communities.Update(community);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var community = await _context.Communities.FindAsync(id);
        if (community != null)
        {
            _context.Communities.Remove(community);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddMemberAsync(string communityId, string npcId, string role = "member")
    {
        var existing = await _context.CommunityMembers
            .FirstOrDefaultAsync(cm => cm.CommunityId == communityId && cm.NPCId == npcId);
        
        if (existing == null)
        {
            var member = new CommunityMember
            {
                CommunityId = communityId,
                NPCId = npcId,
                Role = role
            };
            _context.CommunityMembers.Add(member);

            var community = await _context.Communities.FindAsync(communityId);
            if (community != null)
            {
                community.MemberCount++;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveMemberAsync(string communityId, string npcId)
    {
        var member = await _context.CommunityMembers
            .FirstOrDefaultAsync(cm => cm.CommunityId == communityId && cm.NPCId == npcId);
        
        if (member != null)
        {
            _context.CommunityMembers.Remove(member);

            var community = await _context.Communities.FindAsync(communityId);
            if (community != null && community.MemberCount > 0)
            {
                community.MemberCount--;
            }

            await _context.SaveChangesAsync();
        }
    }
}

public class RelationshipRepository : IRelationshipRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public RelationshipRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<NPCRelationship?> GetAsync(string sourceId, string targetId)
    {
        return await _context.NPCRelationships
            .FirstOrDefaultAsync(r => r.SourceNpcId == sourceId && r.TargetNpcId == targetId);
    }

    public async Task<IEnumerable<NPCRelationship>> GetBySourceAsync(string sourceId)
    {
        return await _context.NPCRelationships
            .Where(r => r.SourceNpcId == sourceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<NPCRelationship>> GetByTargetAsync(string targetId)
    {
        return await _context.NPCRelationships
            .Where(r => r.TargetNpcId == targetId)
            .ToListAsync();
    }

    public async Task<NPCRelationship> AddAsync(NPCRelationship relationship)
    {
        _context.NPCRelationships.Add(relationship);
        await _context.SaveChangesAsync();
        return relationship;
    }

    public async Task UpdateAsync(NPCRelationship relationship)
    {
        relationship.UpdatedAt = DateTimeOffset.UtcNow;
        _context.NPCRelationships.Update(relationship);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string sourceId, string targetId)
    {
        var relationship = await GetAsync(sourceId, targetId);
        if (relationship != null)
        {
            _context.NPCRelationships.Remove(relationship);
            await _context.SaveChangesAsync();
        }
    }
}

public class MessageRepository : IMessageRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public MessageRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Message>> GetConversationAsync(string userId1, string userId2, int limit = 50, int offset = 0)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == userId1 && m.RecipientId == userId2) ||
                        (m.SenderId == userId2 && m.RecipientId == userId1))
            .OrderByDescending(m => m.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<(NPC Other, Message LastMessage)>> GetConversationsAsync(string npcId)
    {
        var lastMessages = await _context.Messages
            .Where(m => m.SenderId == npcId || m.RecipientId == npcId)
            .GroupBy(m => m.SenderId == npcId ? m.RecipientId : m.SenderId)
            .Select(g => new
            {
                OtherId = g.Key,
                LastMessage = g.OrderByDescending(m => m.CreatedAt).FirstOrDefault()
            })
            .Where(x => x.LastMessage != null)
            .ToListAsync();

        var result = new List<(NPC, Message)>();
        foreach (var item in lastMessages)
        {
            var other = await _context.NPCs.FindAsync(item.OtherId);
            if (other != null && item.LastMessage != null)
            {
                result.Add((other, item.LastMessage));
            }
        }
        return result;
    }

    public async Task<Message> AddAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task MarkAsReadAsync(string messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}

public class NotificationRepository : INotificationRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public NotificationRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetByRecipientAsync(string recipientId, bool unreadOnly = false, int limit = 50)
    {
        var query = _context.Notifications.Where(n => n.RecipientId == recipientId);
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }
        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string recipientId)
    {
        return await _context.Notifications
            .CountAsync(n => n.RecipientId == recipientId && !n.IsRead);
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string recipientId)
    {
        var unread = await _context.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .ToListAsync();
        
        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }
        await _context.SaveChangesAsync();
    }
}

public class FeedRepository : IFeedRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public FeedRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetFeedForNpcAsync(string npcId, int limit = 20, string? cursor = null)
    {
        var query = _context.Posts
            .Include(p => p.Community)
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrEmpty(cursor))
        {
            // Parse cursor and filter
            if (DateTimeOffset.TryParse(cursor, out var cursorTime))
            {
                query = query.Where(p => p.CreatedAt < cursorTime);
            }
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.ImportanceScore)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetTrendingPostsAsync(int count)
    {
        return await _context.Posts
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.LikeCount + p.CommentCount + p.ShareCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetDiscoveryPostsAsync(string npcId, int count)
    {
        // Get posts from NPCs this user doesn't follow
        var followingIds = await _context.Follows
            .Where(f => f.FollowerId == npcId)
            .Select(f => f.FollowedId)
            .ToListAsync();

        return await _context.Posts
            .Where(p => !p.IsDeleted && !followingIds.Contains(p.AuthorId) && p.AuthorId != npcId)
            .OrderByDescending(p => p.Popularity)
            .ThenByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
