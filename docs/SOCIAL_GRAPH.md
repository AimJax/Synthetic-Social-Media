# Social Graph System

## Synthetic Social World - Relationships, Communities, and Information Propagation

---

## Core Principles

1. **Relationships are Multi-Dimensional**: Not a single "friendship" number
2. **Directional**: Sarah's relationship with Alex ≠ Alex's relationship with Sarah
3. **Gradual Changes**: Avoid instant dramatic changes
4. **O(N²) Avoidance**: Process graph neighborhoods, not entire graph
5. **Incomplete Knowledge**: NPCs only know what they've observed or learned

---

## Relationship System

### Multi-Dimensional Relationship Model

```csharp
public class NPCRelationship
{
    public string Id { get; set; }
    public string SourceNpcId { get; set; }  // Who holds this relationship
    public string TargetNpcId { get; set; }  // Who the relationship is about
    
    // Multi-dimensional values (-1.0 to 1.0)
    public double Affinity { get; set; }      // General liking
    public double Trust { get; set; }          // Reliability, honesty
    public double Respect { get; set; }         // Admiration, competence
    public double Attraction { get; set; }      // Romantic interest
    public double Hostility { get; set; }       // Active antagonism
    public double Jealousy { get; set; }       // Envy, rivalry
    public double Fear { get; set; }            // Anxiety about target
    public double Admiration { get; set; }     // Respect and esteem
    public double Resentment { get; set; }     // Bitterness, grudges
    public double Familiarity { get; set; }    // How well they know each other
    
    public DateTimeOffset? LastInteractionAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
```

### Relationship Dimensions Explained

| Dimension | Range | Description |
|-----------|-------|-------------|
| Affinity | -1.0 to 1.0 | General positive/negative feeling |
| Trust | -1.0 to 1.0 | Reliability, honesty, vulnerability comfort |
| Respect | -1.0 to 1.0 | Admiration for abilities, character |
| Attraction | -1.0 to 1.0 | Romantic/physical interest |
| Hostility | 0.0 to 1.0 | Active antagonism, desire to harm |
| Jealousy | 0.0 to 1.0 | Envy over attention/resources |
| Fear | 0.0 to 1.0 | Anxiety about target's actions |
| Admiration | 0.0 to 1.0 | Positive esteem, wanting to emulate |
| Resentment | 0.0 to 1.0 | Stored bitterness from past events |
| Familiarity | 0.0 to 1.0 | Depth of mutual knowledge/interaction |

### Directional Example

```
Sarah's relationship with Alex:
├── Affinity: -0.2 (dislikes)
├── Trust: -0.7 (distrusts)
├── Respect: 0.4 (respects abilities)
├── Hostility: 0.8 (hostile)
└── Familiarity: 0.9 (knows well)

Alex's relationship with Sarah:
├── Affinity: 0.6 (likes)
├── Trust: 0.3 (somewhat trusts)
├── Respect: 0.2 (neutral)
├── Hostility: 0.1 (minimal)
└── Familiarity: 0.9 (knows well)
```

### Relationship Update Triggers

```csharp
public class RelationshipUpdateService
{
    public void UpdateRelationship(RelationshipChange change)
    {
        var relationship = GetOrCreateRelationship(change.SourceId, change.TargetId);
        
        foreach (var dimension in change.Dimensions)
        {
            var currentValue = GetDimensionValue(relationship, dimension.Type);
            var changeAmount = CalculateChangeAmount(dimension, relationship);
            
            // Gradual changes - no instant jumps unless justified
            if (Math.Abs(changeAmount) > 0.3 && !change.IsMajorEvent)
            {
                changeAmount *= 0.3; // Dampen dramatic changes
            }
            
            var newValue = Math.Clamp(currentValue + changeAmount, -1.0, 1.0);
            SetDimensionValue(relationship, dimension.Type, newValue);
        }
        
        relationship.LastInteractionAt = DateTimeOffset.UtcNow;
        relationship.UpdatedAt = DateTimeOffset.UtcNow;
        
        Persist(relationship);
    }
}
```

---

## Follow System

### Follow Relationships

```csharp
public class Follow
{
    public string Id { get; set; }
    public string FollowerId { get; set; }  // Who is following
    public string FollowedId { get; set; }  // Who is being followed
    public DateTimeOffset CreatedAt { get; set; }
}
```

### Follower Count
- Stored as denormalized counter on NPC entity
- Updated asynchronously via event processing
- Periodically reconciled against actual count

---

## Community System

### Community Entity

```csharp
public class Community
{
    public string Id { get; set; }
    public string WorldId { get; set; }
    public string Name { get; set; }
    public string Handle { get; set; }
    public string Topic { get; set; }
    public string Description { get; set; }
    public string Rules { get; set; }
    
    // Community culture (0.0 = toxic, 1.0 = healthy)
    public double CultureScore { get; set; } = 0.5;
    public double ToxicityLevel { get; set; } = 0.0;
    
    // Stats
    public double Popularity { get; set; }
    public int MemberCount { get; set; }
    
    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedById { get; set; }
}
```

### Community Member

```csharp
public class CommunityMember
{
    public string Id { get; set; }
    public string CommunityId { get; set; }
    public string NpcId { get; set; }
    public CommunityRole Role { get; set; }  // member, moderator, admin
    public DateTimeOffset JoinedAt { get; set; }
}
```

### Community States

| State | Description | Behavior |
|-------|-------------|----------|
| Dormant | Low activity | Minimal processing |
| Active | Regular activity | Standard processing |
| Growing | Increasing members | Enhanced attention |
| Declining | Losing members | Monitor for deletion |
| Trending | High visibility | HOT LOD processing |
| Toxic | High conflict | Moderation attention |

---

## Event System

### Event Entity

```csharp
public class Event
{
    public string Id { get; set; }
    public string CommunityId { get; set; }  // Optional
    public string OrganizerId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public EventType Type { get; set; }  // party, tournament, meetup, protest
    public string Location { get; set; }
    
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    
    public int AttendeeCount { get; set; }
    public int? MaxAttendees { get; set; }
    
    public double Popularity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

### Event States

| State | Description |
|-------|-------------|
| Upcoming | Scheduled for future |
| In Progress | Currently happening |
| Completed | Past event |
| Cancelled | Cancelled by organizer |

---

## Social Graph Architecture

### Graph Representation

```
                    NPC: Sarah
                         │
         ┌───────────────┼───────────────┐
         │               │               │
      Follows        Friends         In Community
         │               │               │
         ▼               ▼               ▼
    ┌────────┐     ┌────────┐     ┌─────────┐
    │  Alex  │────▶│  Mike  │◀────│ Gaming  │
    └────────┘     └────────┘     │ Community│
         │               │          └─────────┘
         │               │
         ▼               ▼
    Relationship    Relationship
    (hostile)       (friendly)
```

### Graph Neighborhood Processing

```csharp
public class GraphNeighborhoodService
{
    public IEnumerable<NPC> GetNeighbors(string npcId, int depth = 1)
    {
        var neighbors = new HashSet<string>();
        var frontier = new Queue<string>();
        frontier.Enqueue(npcId);
        
        while (frontier.Count > 0 && depth > 0)
        {
            var current = frontier.Dequeue();
            var currentNeighbors = GetDirectConnections(current);
            
            foreach (var neighbor in currentNeighbors)
            {
                if (neighbors.Add(neighbor))
                    frontier.Enqueue(neighbor);
            }
            
            if (frontier.Count == 0)
                depth--;
        }
        
        return neighbors.Select(id => _npcRepository.Get(id));
    }
    
    public IEnumerable<NPC> GetSocialNeighborhood(string npcId)
    {
        // Get followers, following, friends, community members
        return GetNeighbors(npcId, 2)
            .Where(n => IsStrongConnection(npcId, n.Id));
    }
}
```

### O(N²) Avoidance Strategies

1. **Neighborhood Processing**: Only process connected NPCs
2. **Importance Filtering**: Skip cold NPCs
3. **Activity Filtering**: Skip inactive NPCs
4. **Random Sampling**: For massive populations, sample instead of process
5. **Temporal Batching**: Group events by time window

---

## Information Propagation

### Propagation Channels

```
Public Post → Followers See → Comments → Shares → Community Members See
                │                                    │
                ▼                                    ▼
           Knowledge Entry                      Knowledge Entry
           (read type)                          (read type)
                │                                    │
                ▼                                    ▼
         NPC's Knowledge                    NPC's Knowledge
```

### Rumor Spread Algorithm

```csharp
public class RumorSpreadService
{
    public void SpreadRumor(Rumor rumor, string spreaderId)
    {
        var spreader = _npcRepository.Get(spreaderId);
        var followers = _followRepository.GetFollowers(spreaderId);
        
        foreach (var follower in followers)
        {
            // Calculate probability of spread
            var spreadProbability = CalculateSpreadProbability(
                rumor,
                spreader,
                follower);
            
            if (_random.NextDouble() < spreadProbability)
            {
                // Create knowledge entry for follower
                CreateKnowledgeEntry(
                    follower.Id,
                    rumor,
                    sourceId: spreaderId,
                    type: "told",
                    confidence: rumor.Confidence * GetTrustMultiplier(spreaderId, follower.Id));
                
                // Add to rumor source chain
                AddToRumorChain(rumor.Id, follower.Id);
                
                // Optionally relay to their followers
                if (rumor.SpreadCount < 5 && rumor.Confidence > 0.3)
                {
                    var subRumor = CreateRelayedRumor(rumor, follower.Id);
                    SpreadRumor(subRumor, follower.Id);
                }
            }
        }
    }
    
    private double CalculateSpreadProbability(Rumor rumor, NPC spreader, NPC receiver)
    {
        double probability = 0.3; // Base probability
        
        // Interest in topic
        probability += receiver.GetInterestIn(rumor.Topic) * 0.3;
        
        // Relationship with spreader
        probability += receiver.GetRelationship(spreader.Id).Familiarity * 0.2;
        
        // Already knows?
        if (receiver.KnowsAbout(rumor.Subject))
            probability *= 0.3;
        
        // Controversy factor
        if (rumor.IsControversial)
            probability += 0.2;
        
        return Math.Clamp(probability, 0.0, 0.9);
    }
}
```

### Information Decay

Information can transform or lose certainty as it propagates:

```csharp
public double CalculatePropagationConfidence(
    double originalConfidence,
    int hopsFromOrigin,
    double trustInChain)
{
    // Each hop potentially reduces confidence
    var hopDecay = Math.Pow(0.8, hopsFromOrigin);
    
    // Chain trust affects decay rate
    var trustModifier = 0.5 + (trustInChain * 0.5);
    
    return originalConfidence * hopDecay * trustModifier;
}
```

---

## Social Contagion

### Types of Contagion

1. **Opinion Contagion**: beliefs spread and influence others
2. **Behavior Contagion**: actions are imitated
3. **Emotional Contagion**: moods spread through interaction
4. **Status Contagion**: popularity/influence spreads

### Contagion Implementation

```csharp
public class SocialContagionService
{
    public void ProcessContagion(string npcId)
    {
        var npc = _npcRepository.Get(npcId);
        var neighbors = _graphService.GetSocialNeighborhood(npcId);
        
        foreach (var neighbor in neighbors)
        {
            // Opinion contagion
            ProcessOpinionContagion(npc, neighbor);
            
            // Behavior contagion
            ProcessBehaviorContagion(npc, neighbor);
            
            // Emotional contagion
            ProcessEmotionalContagion(npc, neighbor);
        }
    }
    
    private void ProcessOpinionContagion(NPC target, NPC source)
    {
        var relationship = target.GetRelationship(source.Id);
        var influenceStrength = CalculateInfluence(relationship);
        
        // Get source's beliefs
        var sourceBeliefs = _beliefRepository.GetBeliefs(source.Id);
        
        foreach (var belief in sourceBeliefs)
        {
            // Does target already have this belief?
            var existingBelief = target.GetBelief(belief.Subject);
            
            if (existingBelief == null)
            {
                // Maybe adopt new belief
                if (_random.NextDouble() < influenceStrength * belief.Confidence)
                {
                    target.AdoptBelief(
                        belief.Subject,
                        belief.Claim,
                        belief.Confidence * 0.8, // Slight reduction
                        source: source.Id);
                }
            }
        }
    }
}
```

---

## Popularity System

### Popularity Calculation

```csharp
public class PopularityService
{
    public double CalculatePopularity(string npcId)
    {
        var npc = _npcRepository.Get(npcId);
        
        // Base: follower count (logarithmic)
        double popularity = Math.Log10(npc.FollowerCount + 1) * 10;
        
        // Boost: recent engagement
        var recentEngagement = _engagementRepository.GetRecentEngagement(npcId);
        popularity += recentEngagement.TotalEngagement * 0.1;
        
        // Boost: trending posts
        if (_trendingService.HasTrendingPost(npcId))
            popularity *= 1.5;
        
        // Boost: community influence
        var communityInfluence = CalculateCommunityInfluence(npcId);
        popularity += communityInfluence * 5;
        
        // Damping: recent controversy
        if (HasRecentControversy(npcId))
            popularity *= 0.7;
        
        // Damping: inactivity
        var daysSinceActive = (DateTimeOffset.UtcNow - npc.LastActiveAt).TotalDays;
        if (daysSinceActive > 7)
            popularity *= Math.Max(0.5, 1.0 - (daysSinceActive * 0.05));
        
        return Math.Clamp(popularity, 0, 1000);
    }
}
```

---

## LOD-Based Processing

### HOT Processing (Highest Detail)
```csharp
public void ProcessHotEntities()
{
    // Player
    var player = _playerService.GetPlayer();
    ProcessNPC(player, LODLevel.Hot);
    
    // Direct interactors
    var interactors = _interactionService.GetRecentInteractors(player.Id);
    foreach (var npc in interactors)
        ProcessNPC(npc, LODLevel.Hot);
    
    // Trending content
    var trending = _trendingService.GetTrendingContent();
    foreach (var content in trending)
        ProcessContent(content, LODLevel.Hot);
    
    // Major conflicts
    var conflicts = _conflictService.GetActiveConflicts();
    foreach (var conflict in conflicts)
        ProcessConflict(conflict, LODLevel.Hot);
}
```

### WARM Processing (Moderate Detail)
```csharp
public void ProcessWarmEntities()
{
    var activeNpcs = _npcRepository.GetActiveNPCs();
    foreach (var npc in activeNpcs)
        ProcessNPC(npc, LODLevel.Warm);
    
    var popularPosts = _postRepository.GetPopularPosts();
    foreach (var post in popularPosts)
        ProcessPost(post, LODLevel.Warm);
}
```

### COLD Processing (Minimal Detail)
```csharp
public void ProcessColdEntities()
{
    var inactiveNpcs = _npcRepository.GetInactiveNPCs();
    foreach (var batch in inactiveNpcs.Chunk(100))
    {
        // Batch process
        ProcessBatchAggregate(batch, LODLevel.Cold);
    }
}
```

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [SIMULATION.md](./SIMULATION.md) - NPC behavior system
- [MEMORY_SYSTEM.md](./MEMORY_SYSTEM.md) - Memory architecture
- [FEED_SYSTEM.md](./FEED_SYSTEM.md) - Feed ranking
