using SyntheticSocialWorld.Domain.Entities;

namespace SyntheticSocialWorld.Domain.Interfaces;

/// <summary>
/// Repository interface for NPC operations.
/// </summary>
public interface INpcRepository
{
    Task<NPC?> GetByIdAsync(string id);
    Task<NPC?> GetByHandleAsync(string handle);
    Task<IEnumerable<NPC>> GetAllAsync();
    Task<IEnumerable<NPC>> GetActiveAsync(int count);
    Task<IEnumerable<NPC>> GetNeighborsAsync(string npcId, int depth = 1);
    Task<NPC> AddAsync(NPC npc);
    Task UpdateAsync(NPC npc);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

/// <summary>
/// Repository interface for Post operations.
/// </summary>
public interface IPostRepository
{
    Task<Post?> GetByIdAsync(string id);
    Task<IEnumerable<Post>> GetByAuthorAsync(string authorId, int limit = 50, int offset = 0);
    Task<IEnumerable<Post>> GetByCommunityAsync(string communityId, int limit = 50, int offset = 0);
    Task<IEnumerable<Post>> GetRecentAsync(int count);
    Task<Post> AddAsync(Post post);
    Task<Comment> AddCommentAsync(Comment comment);
    Task UpdateAsync(Post post);
    Task DeleteAsync(string id);
    Task IncrementEngagementAsync(string postId, string engagementType);
}

/// <summary>
/// Repository interface for World operations.
/// </summary>
public interface IWorldRepository
{
    Task<World?> GetByIdAsync(string id);
    Task<World?> GetDefaultAsync();
    Task<World> AddAsync(World world);
    Task UpdateAsync(World world);
    Task EnsureDefaultWorldExistsAsync();
}

/// <summary>
/// Repository interface for Community operations.
/// </summary>
public interface ICommunityRepository
{
    Task<Community?> GetByIdAsync(string id);
    Task<Community?> GetByHandleAsync(string handle);
    Task<IEnumerable<Community>> GetAllAsync(int limit = 50, int offset = 0);
    Task<IEnumerable<Community>> GetPopularAsync(int count);
    Task<IEnumerable<Community>> GetByMemberAsync(string npcId);
    Task<Community> AddAsync(Community community);
    Task UpdateAsync(Community community);
    Task DeleteAsync(string id);
    Task AddMemberAsync(string communityId, string npcId, string role = "member");
    Task RemoveMemberAsync(string communityId, string npcId);
}

/// <summary>
/// Repository interface for Relationship operations.
/// </summary>
public interface IRelationshipRepository
{
    Task<NPCRelationship?> GetAsync(string sourceId, string targetId);
    Task<IEnumerable<NPCRelationship>> GetBySourceAsync(string sourceId);
    Task<IEnumerable<NPCRelationship>> GetByTargetAsync(string targetId);
    Task<NPCRelationship> AddAsync(NPCRelationship relationship);
    Task UpdateAsync(NPCRelationship relationship);
    Task DeleteAsync(string sourceId, string targetId);
}

/// <summary>
/// Repository interface for Follow operations.
/// </summary>
public interface IFollowRepository
{
    Task<bool> IsFollowingAsync(string followerId, string followedId);
    Task<IEnumerable<NPC>> GetFollowersAsync(string npcId);
    Task<IEnumerable<NPC>> GetFollowingAsync(string npcId);
    Task<int> GetFollowerCountAsync(string npcId);
    Task<int> GetFollowingCountAsync(string npcId);
    Task AddAsync(string followerId, string followedId);
    Task RemoveAsync(string followerId, string followedId);
}

/// <summary>
/// Repository interface for Message operations.
/// </summary>
public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetConversationAsync(string userId1, string userId2, int limit = 50, int offset = 0);
    Task<IEnumerable<(NPC Other, Message LastMessage)>> GetConversationsAsync(string npcId);
    Task<Message> AddAsync(Message message);
    Task MarkAsReadAsync(string messageId);
}

/// <summary>
/// Repository interface for Notification operations.
/// </summary>
public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByRecipientAsync(string recipientId, bool unreadOnly = false, int limit = 50);
    Task<int> GetUnreadCountAsync(string recipientId);
    Task<Notification> AddAsync(Notification notification);
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync(string recipientId);
}

/// <summary>
/// Repository interface for Feed operations.
/// </summary>
public interface IFeedRepository
{
    Task<IEnumerable<Post>> GetFeedForNpcAsync(string npcId, int limit = 20, string? cursor = null);
    Task<IEnumerable<Post>> GetTrendingPostsAsync(int count);
    Task<IEnumerable<Post>> GetDiscoveryPostsAsync(string npcId, int count);
}

/// <summary>
/// Repository interface for Memory operations.
/// </summary>
public interface IMemoryRepository
{
    Task<EpisodicMemory> AddMemoryAsync(EpisodicMemory memory);
    Task<IEnumerable<EpisodicMemory>> GetMemoriesForNpcAsync(string npcId, int limit = 50);
    Task<IEnumerable<EpisodicMemory>> GetRelevantMemoriesAsync(string npcId, string? targetNpcId, IEnumerable<string>? topics, int limit = 20);
    Task<SemanticBelief?> GetBeliefAsync(string npcId, string subject);
    Task<SemanticBelief> AddBeliefAsync(SemanticBelief belief);
    Task UpdateBeliefAsync(SemanticBelief belief);
    Task ProcessDecayAsync(string npcId);
}

/// <summary>
/// Repository interface for ScheduledAction operations.
/// </summary>
public interface IScheduledActionRepository
{
    Task<IEnumerable<ScheduledAction>> GetDueActionsAsync(DateTimeOffset asOf);
    Task<ScheduledAction?> GetNextActionAsync();
    Task<ScheduledAction> AddAsync(ScheduledAction action);
    Task UpdateAsync(ScheduledAction action);
    Task<IEnumerable<ScheduledAction>> GetByNpcAsync(string npcId);
    Task CancelByNpcAsync(string npcId, string actionType);
}
