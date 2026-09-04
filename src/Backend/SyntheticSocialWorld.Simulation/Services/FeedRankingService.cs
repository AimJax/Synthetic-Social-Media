using SyntheticSocialWorld.Domain.Entities;

namespace SyntheticSocialWorld.Simulation.Services;

/// <summary>
/// Multi-factor feed ranking service implementing the scoring algorithm from FEED_SYSTEM.md
/// 
/// Score = Recency × 0.25 + Relationship × 0.20 + Interest × 0.15 + 
///         Engagement × 0.15 + Popularity × 0.10 + Controversy × 0.05 + 
///         Community × 0.05 + PreviousInteraction × 0.05
/// </summary>
public class FeedRankingService
{
    // Scoring weights from FEED_SYSTEM.md
    private const double RecencyWeight = 0.25;
    private const double RelationshipWeight = 0.20;
    private const double InterestWeight = 0.15;
    private const double EngagementWeight = 0.15;
    private const double PopularityWeight = 0.10;
    private const double ControversyWeight = 0.05;
    private const double CommunityWeight = 0.05;
    private const double InteractionWeight = 0.05;

    // Half-life for recency decay: ~24 hours
    private const double RecencyHalfLifeHours = 24.0;

    // Maximum post age to consider (7 days)
    private const double MaxPostAgeHours = 168.0;

    /// <summary>
    /// Calculate the composite score for a post for a specific user
    /// </summary>
    public double CalculateScore(
        Post post,
        string playerId,
        PlayerFeedContext context)
    {
        double score = 0.0;

        // 1. Recency Score (0.25 weight)
        score += CalculateRecencyScore(post.CreatedAt, context.WorldTime) * RecencyWeight;

        // 2. Relationship Score (0.20 weight)
        score += CalculateRelationshipScore(post.AuthorId, playerId, context) * RelationshipWeight;

        // 3. Interest Score (0.15 weight)
        score += CalculateInterestScore(post, playerId, context) * InterestWeight;

        // 4. Engagement Score (0.15 weight)
        score += CalculateEngagementScore(post) * EngagementWeight;

        // 5. Author Popularity Score (0.10 weight)
        score += CalculatePopularityScore(post.Author) * PopularityWeight;

        // 6. Controversy Score (0.05 weight)
        score += CalculateControversyScore(post) * ControversyWeight;

        // 7. Community Score (0.05 weight)
        score += CalculateCommunityScore(post, playerId, context) * CommunityWeight;

        // 8. Previous Interaction Score (0.05 weight)
        score += CalculateInteractionScore(post, playerId, context) * InteractionWeight;

        return Math.Clamp(score, 0.0, 1.0);
    }

    /// <summary>
    /// Calculate recency score using exponential decay
    /// Half-life of ~24 hours
    /// </summary>
    private double CalculateRecencyScore(DateTimeOffset postTime, DateTimeOffset currentTime)
    {
        var hoursOld = (currentTime - postTime).TotalHours;
        
        // Clamp to max age
        if (hoursOld > MaxPostAgeHours)
            return 0.0;
        
        // Exponential decay: e^(-hours / halfLife)
        return Math.Exp(-hoursOld / RecencyHalfLifeHours);
    }

    /// <summary>
    /// Calculate relationship score based on affinity, trust, respect, and hostility
    /// </summary>
    private double CalculateRelationshipScore(
        string authorId, 
        string playerId, 
        PlayerFeedContext context)
    {
        if (!context.Relationships.TryGetValue(authorId, out var relationship))
            return 0.0;

        // Combine positive and negative dimensions
        double score = relationship.Affinity * 0.3;
        score += relationship.Trust * 0.2;
        score += relationship.Respect * 0.1;
        score -= relationship.Hostility * 0.3;
        score -= relationship.Jealousy * 0.1;

        return Math.Clamp(score, -1.0, 1.0);
    }

    /// <summary>
    /// Calculate interest relevance based on post topics and player interests
    /// </summary>
    private double CalculateInterestScore(
        Post post, 
        string playerId, 
        PlayerFeedContext context)
    {
        // If no topic-based scoring, use author interests as proxy
        if (!context.AuthorInterests.TryGetValue(post.AuthorId, out var interests) || !interests.Any())
        {
            // Fallback: random but higher for close relationships
            if (context.Relationships.TryGetValue(post.AuthorId, out var rel))
            {
                return Math.Max(0, rel.Affinity + 0.5) * 0.3;
            }
            return 0.15; // Base interest
        }

        // Score based on topic match with player interests
        double totalRelevance = 0.0;
        int topicCount = 0;

        foreach (var interest in interests)
        {
            // Check if post mentions this topic (simple keyword matching)
            if (ContainsTopic(post.Content, interest.Topic))
            {
                totalRelevance += interest.Weight;
                topicCount++;
            }
        }

        if (topicCount == 0)
            return 0.15; // Base interest for no topic match

        return Math.Clamp(totalRelevance / Math.Max(topicCount, 1), 0.0, 1.0);
    }

    /// <summary>
    /// Calculate engagement score based on likes, comments, shares relative to views
    /// </summary>
    private double CalculateEngagementScore(Post post)
    {
        var totalEngagement = post.LikeCount + post.CommentCount + post.ShareCount;
        
        if (post.ViewCount == 0)
        {
            // No views yet - use absolute engagement as signal
            return Math.Min(totalEngagement / 50.0, 0.5);
        }
        
        var engagementRate = totalEngagement / (double)post.ViewCount;
        
        // High engagement is good, but very high (>50%) might be suspicious (bot-like)
        return Math.Min(engagementRate, 0.5);
    }

    /// <summary>
    /// Calculate author popularity using logarithmic scale
    /// Prevents mega-influencers from dominating
    /// </summary>
    private double CalculatePopularityScore(NPC? author)
    {
        if (author == null)
            return 0.0;
        
        // Logarithmic scale: log10(popularity + 1) / 3
        // This means popularity of 999 becomes ~3.0, normalized to 0-1
        var rawScore = Math.Log10(author.Popularity + 1) / 3.0;
        
        return Math.Clamp(rawScore, 0.0, 1.0);
    }

    /// <summary>
    /// Calculate controversy score
    /// Controversial posts (high like+dislike ratio close to 1:1) get a boost
    /// </summary>
    private double CalculateControversyScore(Post post)
    {
        var totalVotes = post.LikeCount + post.DislikeCount;
        
        // Need minimum engagement to be considered
        if (totalVotes < 10)
            return 0.0;
        
        var likeRatio = (double)post.LikeCount / totalVotes;
        
        // 0.5 = balanced (controversial), 0 or 1 = not controversial
        var controversy = 1.0 - Math.Abs(0.5 - likeRatio) * 2;
        
        // Controversial posts get moderate boost
        return controversy * 0.3;
    }

    /// <summary>
    /// Calculate community relevance score
    /// </summary>
    private double CalculateCommunityScore(
        Post post, 
        string playerId, 
        PlayerFeedContext context)
    {
        if (post.CommunityId == null)
            return 0.0;
        
        // Check if player is a member of this community
        if (context.CommunityMemberships.Contains(post.CommunityId))
            return 1.0;
        
        // Check community activity level
        if (context.CommunityActivity.TryGetValue(post.CommunityId, out var activity))
        {
            return Math.Min(activity / 100.0, 1.0);
        }
        
        return 0.0;
    }

    /// <summary>
    /// Calculate previous interaction score
    /// Higher if player has interacted with this author before
    /// </summary>
    private double CalculateInteractionScore(
        Post post, 
        string playerId, 
        PlayerFeedContext context)
    {
        if (!context.InteractionHistory.TryGetValue(post.AuthorId, out var history))
            return 0.0;
        
        // More recent and frequent interactions = higher score
        var recencyBonus = history.LastInteraction.HasValue
            ? Math.Exp(-(DateTimeOffset.UtcNow - history.LastInteraction.Value).TotalDays / 7.0)
            : 0.0;
        
        var frequencyBonus = Math.Min(history.InteractionCount / 20.0, 1.0);
        
        return (recencyBonus * 0.6 + frequencyBonus * 0.4) * 0.5;
    }

    /// <summary>
    /// Simple topic matching (case-insensitive)
    /// </summary>
    private bool ContainsTopic(string content, string topic)
    {
        return content.Contains(topic, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Rank posts using multi-factor scoring
    /// </summary>
    public List<Post> RankPosts(IEnumerable<Post> posts, string playerId, PlayerFeedContext context)
    {
        return posts
            .Select(p => new { Post = p, Score = CalculateScore(p, playerId, context) })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Post)
            .ToList();
    }

    /// <summary>
    /// Apply diversity constraints to prevent same-author domination
    /// </summary>
    public List<Post> ApplyDiversity(List<Post> rankedPosts, int maxSameAuthor = 3, int maxSameTopic = 5)
    {
        var result = new List<Post>();
        var authorCount = new Dictionary<string, int>();
        var topicCount = new Dictionary<string, int>();
        const int maxPosts = 50;

        foreach (var post in rankedPosts)
        {
            if (result.Count >= maxPosts)
                break;

            // Limit same author
            var authorPosts = authorCount.GetValueOrDefault(post.AuthorId, 0);
            if (authorPosts >= maxSameAuthor && !IsHighPriority(post))
                continue;

            // Count topics in post
            // Simple: treat first word as topic for now
            var firstWord = post.Content.Split(' ').FirstOrDefault()?.ToLowerInvariant() ?? "";
            if (!string.IsNullOrEmpty(firstWord))
            {
                var topicPosts = topicCount.GetValueOrDefault(firstWord, 0);
                if (topicPosts >= maxSameTopic)
                    continue;
                topicCount[firstWord] = topicPosts + 1;
            }

            result.Add(post);
            authorCount[post.AuthorId] = authorPosts + 1;
        }

        return result;
    }

    /// <summary>
    /// High priority posts bypass diversity limits
    /// </summary>
    private bool IsHighPriority(Post post)
    {
        // High engagement or recent high importance
        return post.LikeCount + post.CommentCount > 50 || post.ImportanceScore > 0.7;
    }
}

/// <summary>
/// Context for calculating feed scores for a player
/// </summary>
public class PlayerFeedContext
{
    public DateTimeOffset WorldTime { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Relationships from player to other NPCs
    /// Key: NPC ID, Value: Relationship dimensions
    /// </summary>
    public Dictionary<string, RelationshipDimensions> Relationships { get; set; } = new();
    
    /// <summary>
    /// Author interests for scoring
    /// Key: NPC ID, Value: List of interests
    /// </summary>
    public Dictionary<string, List<AuthorInterest>> AuthorInterests { get; set; } = new();
    
    /// <summary>
    /// Communities the player is a member of
    /// </summary>
    public HashSet<string> CommunityMemberships { get; set; } = new();
    
    /// <summary>
    /// Community activity levels
    /// </summary>
    public Dictionary<string, double> CommunityActivity { get; set; } = new();
    
    /// <summary>
    /// Player's interaction history with authors
    /// </summary>
    public Dictionary<string, InteractionHistory> InteractionHistory { get; set; } = new();
    
    /// <summary>
    /// Posts the player has already seen
    /// </summary>
    public HashSet<string> SeenPostIds { get; set; } = new();
}

/// <summary>
/// Simplified relationship dimensions for feed scoring
/// </summary>
public class RelationshipDimensions
{
    public double Affinity { get; set; }
    public double Trust { get; set; }
    public double Respect { get; set; }
    public double Hostility { get; set; }
    public double Jealousy { get; set; }
}

/// <summary>
/// Author interest for topic matching
/// </summary>
public class AuthorInterest
{
    public string Topic { get; set; } = "";
    public double Weight { get; set; }
}

/// <summary>
/// Interaction history for previous interaction scoring
/// </summary>
public class InteractionHistory
{
    public DateTimeOffset? LastInteraction { get; set; }
    public int InteractionCount { get; set; }
}
