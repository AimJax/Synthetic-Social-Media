# Simulation System

## Synthetic Social World - NPC Behavior and World Simulation

---

## Core Simulation Principles

1. **Deterministic First**: Core mechanics MUST work without LLM
2. **Scheduled Actions**: NPCs do not tick continuously; they have scheduled actions
3. **Utility-Based Decisions**: NPCs evaluate actions using weighted utility functions
4. **Event-Driven**: State changes produce domain events
5. **Survivable**: Simulation must survive LLM failure, server restart, extended offline periods

---

## World Clock

### Purpose
Authoritative persistent time source that survives restarts and disconnections.

### Characteristics
- Stored as persistent timestamp in database
- Independent of client frame rate or server uptime
- Supports speed multipliers (1x, 10x, 100x, 1000x for development)
- Can be paused/resumed

### Implementation
```csharp
public class WorldClock
{
    public DateTimeOffset CurrentTime { get; private set; }
    public double SpeedMultiplier { get; set; } = 1.0;
    public bool IsPaused { get; set; }
    
    // Advances based on elapsed real time * speed multiplier
    // Persisted on each significant state change
}
```

---

## Scheduler System

### Philosophy
Do NOT continuously tick every NPC. Use scheduled future actions.

### Design
```csharp
public class ScheduledAction
{
    public string NpcId { get; set; }
    public ActionType Type { get; set; }
    public string TargetId { get; set; }
    public DateTimeOffset ScheduledFor { get; set; }
    public int Priority { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}

// Scheduler processes actions when due
// Actions are stored in database for persistence
// When action is due: process → execute → optionally reschedule
```

### Scheduling Strategies

| Activity Type | Default Interval | Variation |
|--------------|------------------|-----------|
| Lurker | 4-8 hours | Random |
| Casual User | 2-4 hours | Random |
| Active User | 30-90 minutes | Random |
| Highly Active | 10-30 minutes | Random |
| Influencer | 5-15 minutes | Random |

### Action Types
- Post (Tier 1-3 based on importance)
- Comment (Tier 1-3 based on importance)
- Like/Dislike (Tier 1 only)
- Follow/Unfollow (Tier 1-2)
- Send Message (Tier 2-3)
- Join/Leave Community (Tier 1)
- Attend Event (Tier 1)
- Create Event (Tier 2)
- Browse Feed (Tier 1)

---

## NPC Activity Profiles

### Activity Levels
NPCs are assigned activity profiles based on archetype:

| Archetype | Activity Level | Post Frequency | Engagement Rate |
|-----------|---------------|----------------|-----------------|
| Lurker | 0.1-0.2 | Very Rare | Low |
| Casual User | 0.3-0.4 | Rare | Moderate |
| Active User | 0.5-0.6 | Regular | High |
| Highly Active | 0.7-0.8 | Frequent | Very High |
| Influencer | 0.8-1.0 | Very Frequent | Very High |

---

## Tiered Behavioral LOD

### Tier 1: Deterministic / Background
**LLM Required: ZERO**

Used for:
- Sleep, work, browse
- Like/dislike
- Routine follow/unfollow
- Basic community activity
- Generic engagement

Example implementations:
- Like: `engagementService.LikePost(npcId, postId)`
- Follow: `socialService.Follow(npcId, targetId)`
- Browse: `feedService.GetFeedForNpc(npcId, page, count)`

### Tier 2: Utility Decision System
**LLM Required: ZERO unless NL generation needed**

NPCs evaluate potential actions using utility functions:

```
UtilityScore = Σ(factor_weight × factor_value × personality_modifier × mood_modifier)
```

**Input Factors:**
- Personality traits
- Current mood
- Interests (topic relevance)
- Goals (goal alignment)
- Relationships (social context)
- Social pressure
- Popularity
- Novelty
- Controversy
- Recent events
- Reputation
- Community context
- Time of day
- Activity schedule

**Output Actions:**
- Post (with topic selection)
- Comment (with target selection)
- Reply (with content type)
- DM (with recipient selection)
- Like/Dislike (with target selection)
- Follow/Unfollow
- Join/Leave Community
- Attend Event
- Create Event

**Example Utility Calculation:**
```csharp
public double CalculatePostUtility(Npc npc, Topic topic)
{
    double baseUtility = 0.5;
    
    // Personality modifiers
    baseUtility += npc.Personality.Extroversion * 0.3;
    baseUtility += npc.Personality.Confidence * 0.2;
    baseUtility -= npc.Personality.Introversion * 0.3;
    
    // Interest modifier
    baseUtility += npc.GetInterestWeight(topic) * 0.4;
    
    // Goal alignment
    baseUtility += npc.GetGoalAlignment(topic) * 0.2;
    
    // Mood modifier
    baseUtility *= npc.Mood.Excitement > 0.7 ? 1.5 : 1.0;
    baseUtility *= npc.Mood.Sadness > 0.7 ? 0.5 : 1.0;
    
    // Time since last post
    var hoursSincePost = (WorldClock.CurrentTime - npc.LastPostTime).TotalHours;
    baseUtility += Math.Min(hoursSincePost / 24, 1.0) * 0.3;
    
    // Social pressure
    if (topic.IsTrending)
        baseUtility += 0.2;
    
    return Math.Clamp(baseUtility, 0.0, 1.0);
}
```

### Tier 3: LLM Expression
**LLM Required: FULL**

Used for:
- Direct player DM
- Direct player reply
- Emotionally significant conversation
- Major argument
- Relationship-defining moment
- Romantic confession
- Important accusation
- Major community conflict
- High-value NPC conversation

**Prompt Context (compact):**
```
NPC: Sarah
Current mood: Annoyed
Personality: Aggression 0.61, Sarcasm 0.82, Humor 0.75
Relationship with Alex: Hostility 0.81, Trust -0.70
Recent memories:
- Alex mocked Sarah's community
- Alex insulted Sarah yesterday
Current event: Alex posted "Some people shouldn't run communities lol."
Task: Generate a reply that Sarah would post.
```

**Output Schema (max 4 root fields):**
```json
{
  "action": "reply",
  "tone": "hostile",
  "emotion": "annoyed",
  "text": "..."
}
```

---

## NPC Decision Flow

```
1. SCHEDULER triggers NPC at scheduled time
        ↓
2. NPC evaluates available action TYPES using Tier 2 utility
        ↓
3. Best action TYPE selected (post, comment, like, etc.)
        ↓
4. Is this a HIGH-VALUE interaction?
   ├── YES → Queue LLM job (Tier 3) → Wait for response
   │         ↓
   │    Validate output → Execute action
   │
   └── NO → Execute Tier 1 deterministic action
            ↓
       Execute immediately
        ↓
5. Create DOMAIN EVENT
        ↓
6. Persist state change
        ↓
7. Schedule NEXT action (based on activity profile)
```

---

## Social LOD (Level of Detail)

### HOT Entities
**Highest simulation detail:**
- Player
- NPCs directly interacting with player
- Active conversations
- Trending content
- Major conflicts
- Influential NPCs
- Important events

**Treatment:**
- Full Tier 2-3 processing
- Real-time updates
- Detailed memory encoding

### WARM Entities
**Moderate detail:**
- Active NPCs
- Popular posts
- Communities with recent activity
- Meaningful relationships

**Treatment:**
- Standard Tier 1-2 processing
- Periodic updates
- Standard memory encoding

### COLD Entities
**Minimal detail:**
- Inactive NPCs
- Dormant communities
- Low-engagement posts
- Background social noise

**Treatment:**
- Tier 1 only (aggregated)
- Batch processing
- Compressed memory encoding
- Statistical updates only

---

## Mood System

### Emotional Dimensions
- Happiness (0.0 - 1.0)
- Sadness (0.0 - 1.0)
- Anger (0.0 - 1.0)
- Excitement (0.0 - 1.0)
- Anxiety (0.0 - 1.0)
- Embarrassment (0.0 - 1.0)
- Affection (0.0 - 1.0)
- Jealousy (0.0 - 1.0)
- Loneliness (0.0 - 1.0)
- Confidence (0.0 - 1.0)

### Mood Dynamics
- Mood changes gradually over time
- Events cause mood shifts
- Personality modifies response strength
- Moods decay or transform naturally
- Primary mood is the dominant current state

### Mood Influence on Behavior
```csharp
public double GetMoodMultiplier(Mood mood, BehaviorType behavior)
{
    return behavior switch
    {
        BehaviorType.Aggressive => 1.0 + mood.Anger * 0.5,
        BehaviorType.Social => 1.0 + mood.Happiness * 0.3 - mood.Sadness * 0.2,
        BehaviorType.Romantic => 1.0 + mood.Affection * 0.4,
        BehaviorType.Withdrawn => 1.0 + mood.Loneliness * 0.3 + mood.Sadness * 0.2,
        BehaviorType.Bold => 1.0 + mood.Confidence * 0.4 + mood.Excitement * 0.2,
        _ => 1.0
    };
}
```

---

## Goal System

### Goal Types
- Gain followers
- Become influential
- Make friends
- Find romance
- Preserve relationship
- Become important in community
- Create community
- Organize events
- Seek attention
- Express opinions
- Avoid conflict
- Maintain reputation

### Goal Properties
```csharp
public class NpcGoal
{
    public GoalType Type { get; set; }
    public double Priority { get; set; } // 0.0 - 1.0
    public double Progress { get; set; } // 0.0 - 1.0
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
```

### Goal Influence on Decisions
Goals affect utility scoring for actions:
- Goal: "Find romance" → Higher utility for romantic interactions
- Goal: "Gain followers" → Higher utility for popular posts
- Goal: "Avoid conflict" → Lower utility for arguments

---

## Two-Speed Simulation

### Online Mode (Player Present)
- Higher NPC responsiveness
- Player-related interactions prioritized
- More detailed social updates
- Real-time WebSocket events
- Faster simulation tick rate

### Offline Mode (Player Absent)
- Accelerated or aggregated progression
- Low-detail background simulation
- Important event extraction
- Deferred narrative generation
- No real-time WebSocket events

### Offline Progression Algorithm
```
1. Calculate elapsed offline time
2. For each active NPC:
   a. Calculate action count based on activity profile
   b. Execute deterministic actions (Tier 1)
   c. Execute utility-driven actions (Tier 2) without LLM
   d. Aggregate low-value interactions
3. Extract HIGH-IMPORTANCE events
4. Generate "while you were away" summary
5. Queue LLM jobs for HIGH-VALUE events only
6. Restore world state for player
```

---

## Social Contagion

Information propagates through the social network:

```
NPC A posts accusation
    ↓
NPC B comments
    ↓
NPC C shares
    ↓
NPC D disagrees
    ↓
NPC E sees the share
    ↓
NPC F learns through community
```

### Propagation Rules
- Not all NPCs see all content
- Visibility depends on following relationships
- Community membership affects reach
- Engagement affects further spread
- Information can transform as it propagates
- Confidence may decrease with distance from source

---

## Conflict System

### Escalation Factors
- Personality (aggression, impulsiveness)
- Relationship state (hostility, trust)
- Interaction history
- Emotional state
- Audience size
- Topic sensitivity
- Stakes involved

### Conflict States
- Latent (no visible conflict)
- Surface (minor disagreement)
- Active (visible argument)
- Escalated (intense conflict)
- Resolving (de-escalation)
- Resolved (conflict ended)
- Mutated (new conflict from old)

### Conflict Outcomes
- Fade away naturally
- Escalate to major drama
- Resolve with winner/loser
- Mutate into different conflict
- Restart later

---

## Population Distribution

### Initial Distribution (20 NPCs)
- 4 Lurkers
- 6 Casual Users
- 5 Active Users
- 3 Highly Active
- 2 Influencers

### Archetype Distribution
- 2-3 Lurkers
- 3-4 Casual Users
- 3-4 Influencers
- 2-3 Comedians
- 1-2 Debate Addicts
- 1 Moderator
- 2-3 Romantics
- 1-2 Gamers
- 1-2 Community Fanatics
- 1-2 Social Butterflies
- 1-2 Introverts
- 1-2 Attention Seekers
- 1-2 Activists
- 1-2 Hobbyists
- 1-2 News Addicts

---

## Performance Considerations

### O(N²) Avoidance
Do NOT evaluate every NPC against every NPC each tick.

### Graph Neighborhood Processing
Only process:
- Followers
- Following
- Friends
- Enemies
- Community members
- Romantic interests
- Recent interaction partners

### Scaling Strategy
- 20 NPCs: Full simulation
- 50 NPCs: Full simulation with batching
- 100 NPCs: Full simulation with LOD
- 250+ NPCs: LOD + aggressive batching
- 500+ NPCs: Aggressive LOD + statistical aggregation
- 1000+ NPCs: Statistical simulation + targeted detail

---

## Action Rate Controls

Prevent pathological behavior with configurable limits:

| Action | Base Limit | Personality Modifier | Max |
|--------|------------|---------------------|-----|
| Posts per day | 10 | × Extroversion | 30 |
| Comments per day | 30 | × Sociability | 100 |
| DMs per day | 20 | varies | 50 |
| Follows per day | 15 | varies | 40 |
| Likes per day | 100 | varies | 200 |

---

## Debug Tools

### Developer Commands
- `npc.spawn` - Create new NPC
- `npc.inspect <id>` - View NPC state
- `world.advance <minutes>` - Advance time
- `world.speed <1|10|100|1000>` - Set speed
- `world.pause` / `world.resume`
- `memory.inject <npcId>` - Add memory
- `queue.clear` - Clear AI queue
- `queue.inspect` - View queue state

### NPC Inspection Output
```
NPC: Sarah
├── Identity: @sarah_developer
├── Personality: Aggression 0.61, Sarcasm 0.82, ...
├── Mood: Annoyed (primary), Anger 0.7, ...
├── Goals: [Gain followers 0.8, Find romance 0.6]
├── Relationships:
│   ├── Alex: Hostility 0.81, Trust -0.70
│   ├── Mike: Affinity 0.65, Trust 0.50
├── Memories: 47 total (12 important)
├── Beliefs: "Player is arrogant" (0.75 confidence)
├── Current Activity: Browsing feed
├── Next Scheduled: Post at 14:32:15
├── Recent Actions:
│   ├── 14:15 - Liked post
│   ├── 14:10 - Commented on Alex's post
│   ├── 14:05 - Viewed profile
└── AI Jobs: 0 queued
```

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [MEMORY_SYSTEM.md](./MEMORY_SYSTEM.md) - Memory architecture
- [AI_SYSTEM.md](./AI_SYSTEM.md) - LLM integration
- [SOCIAL_GRAPH.md](./SOCIAL_GRAPH.md) - Relationship system
