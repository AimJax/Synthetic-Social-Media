# Feed System

## Synthetic Social World - Personalized Feed Ranking

---

## Core Principles

1. **Not Simple Chronological Order**: Feed is NOT `ORDER BY CreatedAt DESC`
2. **Multi-Factor Ranking**: Recency, relationships, interests, engagement, personalization
3. **Player Adaptation**: Player behavior affects future feed
4. **Content Distribution**: Mix of high/low quality, interesting/boring content
5. **Performance**: < 100ms for first page

---

## Feed Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      FEED REQUEST                                │
│  Player requests feed (initial or pagination)                    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                   CANDIDATE GENERATION                            │
│  - Fetch recent posts from followed NPCs                        │
│  - Fetch community posts                                        │
│  - Fetch trending posts                                         │
│  - Fetch recommended posts (cold start)                         │
│  - Apply initial filters                                        │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     SCORING ENGINE                               │
│  Score each candidate using multi-factor algorithm               │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    RANKING & FILTERING                           │
│  - Sort by score descending                                     │
│  - Apply diversity constraints                                   │
│  - Remove seen content                                          │
│  - Paginate                                                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      FEED DELIVERY                               │
│  Return ranked, paginated feed to client                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## Feed Candidate Sources

### 1. Following Feed
```sql
SELECT p.* FROM Posts p
JOIN Follows f ON p.AuthorId = f.FollowedId
WHERE f.FollowerId = @playerId
  AND p.IsDeleted = 0
  AND p.CreatedAt > @cutoffTime
```

### 2. Community Feed
```sql
SELECT p.* FROM Posts p
JOIN CommunityMembers cm ON p.CommunityId = cm.CommunityId
WHERE cm.NpcId = @playerId
  AND p.IsDeleted = 0
```

### 3. Trending Posts
```sql
SELECT p.* FROM Posts p
WHERE p.EngagementScore > @threshold
  AND p.CreatedAt > @cutoffTime
  AND p.IsDeleted = 0
ORDER BY p.EngagementScore DESC
LIMIT 100
```

### 4. Recommended Posts (Discovery)
```sql
-- Based on player interests and similar users
SELECT p.* FROM Posts p
WHERE p.AuthorId NOT IN (SELECT FollowedId FROM Follows WHERE FollowerId = @playerId)
  AND p.AuthorId NOT IN (SELECT FollowedId FROM Follows WHERE FollowerId = @playerId AND Status = 'blocked')
  AND p.CreatedAt > @cutoffTime
  AND p.IsDeleted = 0
ORDER BY p.InterestRelevanceScore + p.AuthorPopularityScore DESC
```

---

## Scoring Algorithm

### Base Score Components

```csharp
public class FeedScoringService
{
    public double CalculateScore(Post post, Player player, FeedContext context)
    {
        double score = 0.0;
        
        // 1. Recency (exponential decay)
        score += CalculateRecencyScore(post.CreatedAt, context.WorldTime) * 0.25;
        
        // 2. Relationship factor
        score += CalculateRelationshipScore(post.AuthorId, player.Id) * 0.20;
        
        // 3. Interest relevance
        score += CalculateInterestScore(post, player) * 0.15;
        
        // 4. Engagement score
        score += CalculateEngagementScore(post) * 0.15;
        
        // 5. Author popularity
        score += CalculatePopularityScore(post.Author) * 0.10;
        
        // 6. Controversy factor
        score += CalculateControversyScore(post) * 0.05;
        
        // 7. Community relevance
        score += CalculateCommunityScore(post, player) * 0.05;
        
        // 8. Previous interaction
        score += CalculateInteractionScore(post, player) * 0.05;
        
        return score;
    }
    
    private double CalculateRecencyScore(DateTimeOffset postTime, DateTimeOffset currentTime)
    {
        var hoursOld = (currentTime - postTime).TotalHours;
        
        // Exponential decay
        return Math.Exp(-hoursOld / 24); // Half-life ~24 hours
    }
    
    private double CalculateRelationshipScore(string authorId, string playerId)
    {
        var relationship = _relationshipService.GetRelationship(playerId, authorId);
        
        if (relationship == null)
            return 0.0;
        
        // Combine positive and negative dimensions
        double score = relationship.Affinity * 0.3;
        score += relationship.Trust * 0.2;
        score += relationship.Respect * 0.1;
        score -= relationship.Hostility * 0.3;
        score -= relationship.Jealousy * 0.1;
        
        return Math.Clamp(score, -1.0, 1.0);
    }
    
    private double CalculateInterestScore(Post post, Player player)
    {
        if (post.Topics == null || !post.Topics.Any())
            return 0.0;
        
        double relevance = 0.0;
        foreach (var topic in post.Topics)
        {
            var playerInterest = player.GetInterestWeight(topic);
            relevance += playerInterest;
        }
        
        return relevance / post.Topics.Count;
    }
    
    private double CalculateEngagementScore(Post post)
    {
        // Engagement ratio (likes + comments + shares / views)
        var totalEngagement = post.LikeCount + post.DislikeCount + post.CommentCount + post.ShareCount;
        
        if (post.ViewCount == 0)
            return 0.0;
        
        var engagementRate = totalEngagement / post.ViewCount;
        
        // Normalize: high engagement is good, but very high is suspicious
        return Math.Min(engagementRate, 0.5);
    }
    
    private double CalculatePopularityScore(NPC author)
    {
        // Logarithmic scale to prevent mega-influencers from dominating
        return Math.Log10(author.Popularity + 1) / 3.0;
    }
    
    private double CalculateControversyScore(Post post)
    {
        // Controversial posts (high like+dislike ratio close to 1:1) get boost
        var totalVotes = post.LikeCount + post.DislikeCount;
        
        if (totalVotes < 10) // Need minimum engagement
            return 0.0;
        
        var likeRatio = post.LikeCount / totalVotes;
        var controversy = 1.0 - Math.Abs(0.5 - likeRatio) * 2; // 0 = balanced, 1 = not controversial
        
        // Controversial posts get moderate boost
        return controversy * 0.3;
    }
}
```

---

## Player Personalization

### Player Interest Profile
```csharp
public class PlayerInterestProfile
{
    public Dictionary<string, double> TopicWeights { get; set; }  // Topic → Weight
    public HashSet<string> EngagedAuthors { get; set; }
    public HashSet<string> IgnoredAuthors { get; set; }
    public List<string> PreferredCommunities { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
```

### Profile Update Triggers
```csharp
public class PlayerProfileService
{
    public void UpdateProfileFromAction(PlayerAction action)
    {
        switch (action.Type)
        {
            case ActionType.Like:
                IncreaseInterestInTopics(action.Target.Topics, 0.1);
                IncreaseEngagementWithAuthor(action.Target.AuthorId, 0.1);
                break;
                
            case ActionType.Dislike:
                DecreaseInterestInTopics(action.Target.Topics, 0.05);
                break;
                
            case ActionType.Comment:
                IncreaseInterestInTopics(action.Target.Topics, 0.2);
                IncreaseEngagementWithAuthor(action.Target.AuthorId, 0.2);
                break;
                
            case ActionType.Share:
                IncreaseInterestInTopics(action.Target.Topics, 0.15);
                IncreaseEngagementWithAuthor(action.Target.AuthorId, 0.15);
                break;
                
            case ActionType.Follow:
                IncreaseEngagementWithAuthor(action.Target.AuthorId, 0.5);
                break;
                
            case ActionType.Ignore:
            case ActionType.Hide:
                DecreaseEngagementWithAuthor(action.Target.AuthorId, 0.3);
                break;
        }
        
        _playerProfile.InterestDecay(action.Timestamp);
        Persist(_playerProfile);
    }
}
```

---

## Content Distribution

### Realistic Feed Composition
A healthy feed should contain:

| Content Type | Percentage | Examples |
|-------------|-----------|----------|
| High-effort posts | 10-15% | Long content, thoughtful posts |
| Medium-effort | 25-30% | Regular updates, photos |
| Low-effort | 30-35% | Quick takes, reactions, memes |
| Engagement bait | 5-10% | Questions, polls, CTAs |
| Ads/simulated | 0-5% | If implemented |
| Controversial | 5-10% | Debates, hot takes |
| Personal updates | 10-15% | Life updates, check-ins |

### Diversity Constraints
```csharp
public class FeedDiversityService
{
    public List<Post> ApplyDiversity(List<Post> rankedPosts, int targetCount = 50)
    {
        var result = new List<Post>();
        var authorCount = new Dictionary<string, int>();
        var topicCount = new Dictionary<string, int>();
        
        foreach (var post in rankedPosts)
        {
            if (result.Count >= targetCount)
                break;
            
            // Limit same author
            var authorPosts = authorCount.GetValueOrDefault(post.AuthorId, 0);
            if (authorPosts >= 3 && !IsHighPriority(post))
                continue;
            
            // Limit same topic
            foreach (var topic in post.Topics ?? Enumerable.Empty<string>())
            {
                var topicPosts = topicCount.GetValueOrDefault(topic, 0);
                if (topicPosts >= 5)
                    continue;
            }
            
            result.Add(post);
            authorCount[post.AuthorId] = authorPosts + 1;
            foreach (var topic in post.Topics ?? Enumerable.Empty<string>())
                topicCount[topic] = topicCount.GetValueOrDefault(topic, 0) + 1;
        }
        
        return result;
    }
}
```

---

## Feed Caching

### Cache Layers
```csharp
public class FeedCacheService
{
    // Layer 1: In-memory cache for hot data
    private readonly IMemoryCache _memoryCache;
    
    // Layer 2: Redis/distributed cache (future)
    private readonly IDistributedCache _distributedCache;
    
    // Cache keys
    private const string PlayerFeedKey = "feed:{playerId}:page:{page}";
    private const string PlayerFeedVersionsKey = "feed:{playerId}:versions";
    
    public async Task<FeedResponse> GetFeed(string playerId, int page, int pageSize)
    {
        var cacheKey = string.Format(PlayerFeedKey, playerId, page);
        
        // Try memory cache first
        if (_memoryCache.TryGetValue(cacheKey, out var cached))
            return (FeedResponse)cached;
        
        // Compute feed
        var feed = await ComputeFeed(playerId, page, pageSize);
        
        // Cache with TTL
        _memoryCache.Set(cacheKey, feed, TimeSpan.FromMinutes(1));
        
        return feed;
    }
    
    public void InvalidateFeed(string playerId)
    {
        // Invalidate all pages for player
        var pattern = $"feed:{playerId}:*";
        _memoryCache.RemoveByPattern(pattern);
    }
}
```

---

## Infinite Scroll / Pagination

### Cursor-Based Pagination
```csharp
public class FeedPaginationService
{
    public async Task<FeedPage> GetFeedPage(
        string playerId,
        string cursor,
        int pageSize = 20)
    {
        var context = new FeedContext
        {
            PlayerId = playerId,
            WorldTime = _worldClock.CurrentTime,
            PlayerProfile = await _profileService.GetProfile(playerId)
        };
        
        // Parse cursor for resume point
        DateTimeOffset? resumeFrom = null;
        double? resumeScore = null;
        
        if (!string.IsNullOrEmpty(cursor))
        {
            (resumeFrom, resumeScore) = ParseCursor(cursor);
        }
        
        // Generate candidates (already scored and ranked)
        var candidates = await _candidateService.GetCandidates(context, resumeFrom);
        
        // Filter already-seen posts
        var unseen = candidates
            .Where(c => resumeScore == null || c.Score < resumeScore)
            .Where(c => !_seenService.HasSeen(playerId, c.PostId))
            .Take(pageSize + 1) // +1 for next cursor
            .ToList();
        
        // Build response
        var hasMore = unseen.Count > pageSize;
        var page = unseen.Take(pageSize).ToList();
        
        var nextCursor = hasMore 
            ? CreateCursor(page.Last())
            : null;
        
        return new FeedPage
        {
            Posts = page.Select(c => c.ToDto()).ToList(),
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}
```

---

## Virality Handling

### Virality Detection
```csharp
public class ViralityService
{
    private const double ViralThreshold = 0.8;
    
    public bool IsViral(Post post)
    {
        // Calculate virality score
        var velocity = CalculateEngagementVelocity(post);
        var spread = CalculateSpreadRate(post);
        var networkReach = EstimateNetworkReach(post.AuthorId);
        
        var viralityScore = velocity * 0.4 + spread * 0.3 + networkReach * 0.3;
        
        return viralityScore > ViralThreshold;
    }
    
    private double CalculateEngagementVelocity(Post post)
    {
        var hoursOld = (_worldClock.CurrentTime - post.CreatedAt).TotalHours;
        if (hoursOld < 0.1) hoursOld = 0.1; // Prevent division issues
        
        var totalEngagement = post.LikeCount + post.CommentCount + post.ShareCount;
        return totalEngagement / hoursOld; // Engagement per hour
    }
}
```

### Virality Constraints
- Viral posts get boosted but capped
- Prevent runaway feedback loops
- Apply dampening at thresholds

---

## "While You Were Away" Summary

### Summary Generation
```csharp
public class CatchUpSummaryService
{
    public async Task<CatchUpSummary> GenerateSummary(
        string playerId,
        TimeSpan offlineDuration)
    {
        var events = await _eventService.GetImportantEvents(playerId, offlineDuration);
        
        var summary = new CatchUpSummary
        {
            Duration = offlineDuration,
            Events = new List<SummaryEvent>()
        };
        
        // Group by type
        var grouped = events.GroupBy(e => e.Type);
        
        foreach (var group in grouped)
        {
            switch (group.Key)
            {
                case "follower_gain":
                    summary.Events.Add(new SummaryEvent
                    {
                        Type = "follower_gain",
                        Count = group.Sum(e => e.Count),
                        Description = GenerateFollowerDescription(group)
                    });
                    break;
                    
                case "engagement":
                    summary.Events.Add(new SummaryEvent
                    {
                        Type = "engagement",
                        Likes = group.Where(e => e.SubType == "like").Sum(e => e.Count),
                        Comments = group.Where(e => e.SubType == "comment").Sum(e => e.Count),
                        Description = GenerateEngagementDescription(group)
                    });
                    break;
                    
                case "dm_received":
                    summary.Events.Add(new SummaryEvent
                    {
                        Type = "dm_received",
                        Count = group.Count(),
                        NPCs = group.Select(e => e.NpcId).Distinct().ToList(),
                        Description = $"{group.Count()} new messages"
                    });
                    break;
                    
                case "public_drama":
                    summary.Events.Add(new SummaryEvent
                    {
                        Type = "public_drama",
                        Description = GenerateDramaDescription(group),
                        Severity = "medium" // or "high"
                    });
                    break;
            }
        }
        
        return summary;
    }
}
```

### Example Summary
```
While you were away (6 hours):

• Sarah gained 83 followers
• Alex's community became popular
• Mike and Jessica had a public argument
• Your post received 41 new likes
• Sarah sent you a DM
• A rumor about you appeared in /Gaming
```

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [SOCIAL_GRAPH.md](./SOCIAL_GRAPH.md) - Relationship system
- [API.md](./API.md) - REST and WebSocket endpoints
