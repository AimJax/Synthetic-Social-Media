using SyntheticSocialWorld.Domain.Entities;

namespace SyntheticSocialWorld.Simulation.Services;

/// <summary>
/// Generates "While you were away" summary when player returns
/// Based on FEED_SYSTEM.md Section 5
/// </summary>
public class CatchupSummaryService
{
    private readonly Random _random = new();
    
    /// <summary>
    /// Function to get author name by ID. Set during GenerateSummary call.
    /// </summary>
    private Func<string, string> GetAuthorName = id => "Someone";
    
    /// <summary>
    /// Generate a comprehensive summary of what happened while the player was away
    /// </summary>
    public CatchupSummary GenerateSummary(
        string playerId,
        DateTimeOffset lastSeen,
        DateTimeOffset currentTime,
        IEnumerable<Post> newPosts,
        IEnumerable<Comment> newComments,
        IEnumerable<Follow> newFollows,
        IEnumerable<NPCRelationship> relationshipChanges,
        IEnumerable<Rumor> activeRumors,
        IEnumerable<Event> upcomingEvents,
        Func<string, string>? getAuthorName = null)
    {
        // Default implementation if none provided
        GetAuthorName = getAuthorName ?? (id => "Someone");
        
        var summary = new CatchupSummary
        {
            PlayerId = playerId,
            StartTime = lastSeen,
            EndTime = currentTime,
            Duration = currentTime - lastSeen
        };
        
        // Process each category
        
        // 1. Posts from followed NPCs
        foreach (var post in newPosts.Take(10))
        {
            summary.PostSummaries.Add(GeneratePostSummary(post, GetAuthorName));
        }
        
        // 2. Comments on player's posts
        var playerPostIds = newPosts.Where(p => p.AuthorId == playerId).Select(p => p.Id).ToHashSet();
        var relevantComments = newComments.Where(c => playerPostIds.Contains(c.PostId));
        foreach (var comment in relevantComments.Take(5))
        {
            summary.CommentNotifications.Add(GenerateCommentSummary(comment, GetAuthorName));
        }
        
        // 3. Follower changes
        var newFollowers = newFollows.Where(f => f.FollowedId == playerId);
        var unfollows = newFollows.Where(f => f.FollowerId == playerId); // Could track unfollows separately
        
        summary.Stats.NewFollowers = newFollowers.Count();
        summary.Stats.FollowerCount = newFollowers.Count();
        
        foreach (var follower in newFollowers.Take(5))
        {
            summary.FollowerUpdates.Add(new FollowUpdate
            {
                NpcId = follower.FollowerId,
                NpcName = follower.Follower?.DisplayName ?? "Someone",
                Action = "started following you",
                TimeAgo = FormatTimeAgo(follower.CreatedAt)
            });
        }
        
        // 4. Relationship changes
        foreach (var change in relationshipChanges.Take(3))
        {
            if (Math.Abs(change.Affinity) > 0.1 || Math.Abs(change.Trust) > 0.1)
            {
                summary.RelationshipUpdates.Add(new RelationshipUpdate
                {
                    NpcId = change.TargetNpcId,
                    NpcName = change.TargetNpc?.DisplayName ?? "Someone",
                    Change = FormatRelationshipChange(change),
                    TimeAgo = FormatTimeAgo(change.UpdatedAt)
                });
            }
        }
        
        // 5. Active rumors
        foreach (var rumor in activeRumors.Take(3))
        {
            summary.RumorUpdates.Add(new RumorUpdate
            {
                RumorId = rumor.Id,
                AboutNpcId = rumor.OriginatorId,
                AboutNpcName = rumor.Originator?.DisplayName ?? rumor.Subject,
                Description = GenerateRumorDescription(rumor.Subject, rumor.Content),
                Confidence = rumor.Confidence,
                TimeAgo = FormatTimeAgo(rumor.CreatedAt)
            });
        }
        
        // 6. Upcoming events
        foreach (var evt in upcomingEvents.Where(e => e.StartTime > currentTime).Take(3))
        {
            summary.EventReminders.Add(new EventReminder
            {
                EventId = evt.Id,
                EventName = evt.Title,
                StartTime = evt.StartTime,
                TimeUntil = FormatTimeUntil(evt.StartTime)
            });
        }
        
        // 7. Generate narrative summary
        summary.NarrativeSummary = GenerateNarrativeSummary(summary);
        
        return summary;
    }
    
    /// <summary>
    /// Generate a brief summary for a single post
    /// </summary>
    private PostSummary GeneratePostSummary(Post post, Func<string, string> getAuthorName)
    {
        return new PostSummary
        {
            PostId = post.Id,
            AuthorId = post.AuthorId,
            AuthorName = getAuthorName(post.AuthorId),
            Preview = TruncateContent(post.Content, 100),
            Engagement = post.LikeCount + post.CommentCount,
            TimeAgo = FormatTimeAgo(post.CreatedAt)
        };
    }
    
    /// <summary>
    /// Generate notification for a comment
    /// </summary>
    private CommentNotification GenerateCommentSummary(Comment comment, Func<string, string> getAuthorName)
    {
        return new CommentNotification
        {
            CommentId = comment.Id,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            AuthorName = getAuthorName(comment.AuthorId),
            Content = TruncateContent(comment.Content, 80),
            TimeAgo = FormatTimeAgo(comment.CreatedAt)
        };
    }
    
    /// <summary>
    /// Format relationship change for display
    /// </summary>
    private string FormatRelationshipChange(NPCRelationship change)
    {
        if (change.Affinity > 0.1)
            return $"seems more friendly (+{change.Affinity:F2})";
        if (change.Affinity < -0.1)
            return $"seems more distant ({change.Affinity:F2})";
        if (change.Trust > 0.1)
            return "trusts you more";
        if (change.Trust < -0.1)
            return "trusts you less";
        if (change.Hostility > 0.1)
            return "seems hostile toward you";
        return "the relationship feels different";
    }
    
    /// <summary>
    /// Generate human-readable rumor description
    /// </summary>
    private string GenerateRumorDescription(string subject, string content)
    {
        return $"About {subject}: {content}";
    }
    
    /// <summary>
    /// Generate narrative summary combining all events
    /// </summary>
    private string GenerateNarrativeSummary(CatchupSummary summary)
    {
        var lines = new List<string>();
        
        // Duration header
        var hours = summary.Duration.TotalHours;
        var durationText = hours switch
        {
            < 1 => $"{summary.Duration.TotalMinutes:F0} minutes",
            < 24 => $"{hours:F1} hours",
            _ => $"{hours / 24:F1} days"
        };
        
        lines.Add($"While you were away ({durationText}):");
        lines.Add("");
        
        // Follower changes
        if (summary.Stats.NewFollowers > 0)
        {
            var followerNames = summary.FollowerUpdates
                .Take(3)
                .Select(f => f.NpcName)
                .ToList();
            
            if (followerNames.Count == 1)
                lines.Add($"• {followerNames[0]} started following you");
            else if (followerNames.Count > 1)
                lines.Add($"• {string.Join(", ", followerNames.Take(2))} and {summary.Stats.NewFollowers - 2} others followed you");
        }
        
        // Top posts
        var topPosts = summary.PostSummaries
            .OrderByDescending(p => p.Engagement)
            .Take(2)
            .ToList();
        
        foreach (var post in topPosts)
        {
            lines.Add($"• {post.AuthorName} posted: \"{TruncateContent(post.Content, 50)}\"");
        }
        
        // Comments on player's posts
        if (summary.CommentNotifications.Any())
        {
            var topComment = summary.CommentNotifications.First();
            lines.Add($"• {topComment.AuthorName} commented on your post");
        }
        
        // Rumors
        if (summary.RumorUpdates.Any())
        {
            var rumor = summary.RumorUpdates.First();
            lines.Add($"• A rumor about {rumor.AboutNpcName}: \"{TruncateContent(rumor.Description, 60)}\"");
        }
        
        // Relationship changes
        if (summary.RelationshipUpdates.Any())
        {
            var change = summary.RelationshipUpdates.First();
            lines.Add($"• {change.NpcName} {change.Change}");
        }
        
        // Upcoming events
        if (summary.EventReminders.Any())
        {
            var evt = summary.EventReminders.First();
            lines.Add($"• \"{evt.EventName}\" starts {evt.TimeUntil}");
        }
        
        if (lines.Count == 2) // Only header + empty line
        {
            lines.Add("• The world was quiet while you were away");
        }
        
        return string.Join("\n", lines);
    }
    
    private string FormatTimeAgo(DateTimeOffset time)
    {
        var diff = DateTimeOffset.UtcNow - time;
        
        if (diff.TotalMinutes < 1)
            return "just now";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}h ago";
        return $"{(int)diff.TotalDays}d ago";
    }
    
    private string FormatTimeUntil(DateTimeOffset time)
    {
        var diff = time - DateTimeOffset.UtcNow;
        
        if (diff.TotalMinutes < 60)
            return $"in {(int)diff.TotalMinutes}m";
        if (diff.TotalHours < 24)
            return $"in {(int)diff.TotalHours}h";
        return $"in {(int)diff.TotalDays}d";
    }
    
    private string TruncateContent(string content, int maxLength)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
            return content;
        
        var truncated = content.Substring(0, maxLength);
        var lastSpace = truncated.LastIndexOf(' ');
        
        if (lastSpace > maxLength * 0.7)
            truncated = truncated.Substring(0, lastSpace);
        
        return truncated + "...";
    }
}

/// <summary>
/// Complete catchup summary for player
/// </summary>
public class CatchupSummary
{
    public string PlayerId { get; set; } = "";
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string NarrativeSummary { get; set; } = "";
    public SummaryStats Stats { get; set; } = new();
    public List<PostSummary> PostSummaries { get; set; } = new();
    public List<CommentNotification> CommentNotifications { get; set; } = new();
    public List<FollowUpdate> FollowerUpdates { get; set; } = new();
    public List<RelationshipUpdate> RelationshipUpdates { get; set; } = new();
    public List<RumorUpdate> RumorUpdates { get; set; } = new();
    public List<EventReminder> EventReminders { get; set; } = new();
}

/// <summary>
/// Summary statistics
/// </summary>
public class SummaryStats
{
    public int NewFollowers { get; set; }
    public int PostsFromFollowed { get; set; }
    public int CommentsOnYourPosts { get; set; }
    public int NewRumors { get; set; }
    public int FollowerCount { get; set; }
}

/// <summary>
/// Summary of a post
/// </summary>
public class PostSummary
{
    public string PostId { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string Preview { get; set; } = "";
    public string Content { get; set; } = "";
    public int Engagement { get; set; }
    public string TimeAgo { get; set; } = "";
}

/// <summary>
/// Notification about a comment
/// </summary>
public class CommentNotification
{
    public string CommentId { get; set; } = "";
    public string PostId { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string Content { get; set; } = "";
    public string TimeAgo { get; set; } = "";
}

/// <summary>
/// Update about a follower
/// </summary>
public class FollowUpdate
{
    public string NpcId { get; set; } = "";
    public string NpcName { get; set; } = "";
    public string Action { get; set; } = "";
    public string TimeAgo { get; set; } = "";
}

/// <summary>
/// Update about a relationship
/// </summary>
public class RelationshipUpdate
{
    public string NpcId { get; set; } = "";
    public string NpcName { get; set; } = "";
    public string Change { get; set; } = "";
    public string TimeAgo { get; set; } = "";
}

/// <summary>
/// Update about a rumor
/// </summary>
public class RumorUpdate
{
    public string RumorId { get; set; } = "";
    public string AboutNpcId { get; set; } = "";
    public string AboutNpcName { get; set; } = "";
    public string Description { get; set; } = "";
    public double Confidence { get; set; }
    public string TimeAgo { get; set; } = "";
}

/// <summary>
/// Reminder about an upcoming event
/// </summary>
public class EventReminder
{
    public string EventId { get; set; } = "";
    public string EventName { get; set; } = "";
    public DateTimeOffset StartTime { get; set; }
    public string TimeUntil { get; set; } = "";
}
