# Memory System Architecture

## Synthetic Social World - NPC Memory and Knowledge Management

---

## Core Principles

1. **Structured Memory**: Not a giant conversation transcript
2. **Selective Retrieval**: Never send all memories to LLM
3. **Budget Enforcement**: ~512 tokens for memory context
4. **Model Independence**: Store "NPC believes X", not "Qwen believes X"
5. **Memory Lifecycle**: Create, retrieve, decay, consolidate, forget

---

## Memory Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                       MEMORY STORAGE                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │  Episodic   │  │  Semantic   │  │   Social    │             │
│  │   Memory    │  │   Beliefs   │  │   Memory    │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
│  ┌─────────────┐  ┌─────────────┐                              │
│  │   Rumors    │  │ Knowledge   │                              │
│  │             │  │  Graph      │                              │
│  └─────────────┘  └─────────────┘                              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    MEMORY PROCESSING                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │  Relevance  │  │  Retrieval  │  │   Decay     │             │
│  │  Scoring    │  │   Slicing   │  │   Engine    │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    CONTEXT BUILDER                               │
│  Combined with NPC state + current situation                    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    LLM CONTEXT                                   │
│  Compact packet (~512 tokens max)                              │
└─────────────────────────────────────────────────────────────────┘
```

---

## Memory Types

### 1. Episodic Memory
**Purpose**: Record specific events experienced by the NPC

```csharp
public class EpisodicMemory
{
    public string Id { get; set; }
    public string OwnerId { get; set; }  // NPC who remembers
    public string EventType { get; set; }  // "post_created", "argument", "compliment"
    public string Description { get; set; }  // "The player publicly insulted me"
    public List<string> Participants { get; set; }  // Other NPCs involved
    public double Importance { get; set; }  // 0.0 - 1.0
    public string Emotion { get; set; }  // "anger", "joy", "sadness"
    public DateTimeOffset Timestamp { get; set; }
    public string Source { get; set; }  // "direct", "told", "observed"
    public double Confidence { get; set; }  // 0.0 - 1.0
    public DateTimeOffset CreatedAt { get; set; }
}
```

**Example Storage**:
```
NPC: Sarah
Event: "The player publicly insulted me"
Participants: ["player", "alex"]
Importance: 0.8
Emotion: "anger"
Source: "direct"
```

### 2. Semantic Belief
**Purpose**: Store NPC's beliefs and opinions about entities/topics

```csharp
public class SemanticBelief
{
    public string Id { get; set; }
    public string OwnerId { get; set; }
    public string Subject { get; set; }  // Entity or topic
    public string Belief { get; set; }  // "The player is arrogant"
    public double Confidence { get; set; }  // 0.0 - 1.0
    public List<string> SupportingEvidence { get; set; }
    public List<string> ConflictingEvidence { get; set; }
    public string Source { get; set; }  // "direct", "inference", "hearsay"
    public DateTimeOffset Timestamp { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
```

**Example**:
```
Belief: "The player is arrogant"
Confidence: 0.75
SupportingEvidence: [
    "Player mocked my community",
    "Player ignored my question",
    "Player talked down to Mike"
]
ConflictingEvidence: [
    "Player helped me once"
]
Source: "direct"
```

### 3. Social Memory
**Purpose**: Record social interactions and relationships

```csharp
public class SocialMemory
{
    public string Id { get; set; }
    public string OwnerId { get; set; }
    public string Description { get; set; }  // "Alex defended Sarah during argument"
    public List<string> Participants { get; set; }
    public string RelationshipType { get; set; }  // "support", "betrayal", "collaboration"
    public double Importance { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

### 4. Rumor
**Purpose**: Track gossip and information spread

```csharp
public class Rumor
{
    public string Id { get; set; }
    public string OriginatorId { get; set; }  // Who started the rumor
    public string Subject { get; set; }  // Who/what the rumor is about
    public string Content { get; set; }  // "The player is dishonest"
    public double Confidence { get; set; }  // Starts at origin confidence
    public List<RumorSpread> SourceChain { get; set; }  // Who spread it
    public int SpreadCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class RumorSpread
{
    public string SpreaderId { get; set; }
    public DateTimeOffset SpreadAt { get; set; }
    public double ConfidenceAtSpread { get; set; }
}
```

---

## Knowledge Graph

**Purpose**: Track what NPCs know (and how they know it)

```csharp
public class KnowledgeEntry
{
    public string Id { get; set; }
    public string NpcId { get; set; }
    public string EntityType { get; set; }  // "post", "comment", "event", "npc", "community"
    public string EntityId { get; set; }
    public string KnowledgeType { get; set; }  // "observed", "told", "read", "inferred", "learned"
    public double Confidence { get; set; }
    public DateTimeOffset AcquiredAt { get; set; }
    public string SourceId { get; set; }  // NPC who told them (if applicable)
}
```

### Knowledge Types
| Type | Description | Confidence |
|------|-------------|------------|
| Observed | Directly witnessed | 1.0 |
| Read | Saw in feed/post | 0.9 |
| Told | Informed by another NPC | Variable |
| Inferred | Deduced from context | 0.5-0.8 |
| Learned | Through community/event | Variable |

### Example
```
Sarah knows: "The player insulted me"
Type: observed
Confidence: 1.0

Mike does NOT automatically know.

If Sarah tells Mike: "The player is an asshole"
Mike's knowledge:
- Type: told
- Source: Sarah
- Confidence: Based on Sarah's trust level (~0.45)
```

---

## Memory Retrieval System

### Retrieval Algorithm
```csharp
public class MemoryRetriever
{
    private const int DefaultTokenBudget = 512;
    
    public MemoryContext Retrieve(
        string npcId,
        RetrievalQuery query,
        int tokenBudget = DefaultTokenBudget)
    {
        var memories = new List<MemorySlice>();
        
        // 1. Get memories relevant to target person (if any)
        if (query.TargetPersonId != null)
        {
            var personMemories = _repository.GetMemoriesForPerson(npcId, query.TargetPersonId);
            memories.AddRange(RankByRelevance(personMemories, query));
        }
        
        // 2. Get memories relevant to current topic
        if (query.Topics != null && query.Topics.Any())
        {
            var topicMemories = _repository.GetMemoriesByTopics(npcId, query.Topics);
            memories.AddRange(RankByRelevance(topicMemories, query));
        }
        
        // 3. Get emotionally relevant memories
        if (query.EmotionalRelevance != null)
        {
            var emotionalMemories = _repository.GetMemoriesByEmotion(npcId, query.EmotionalRelevance);
            memories.AddRange(RankByRelevance(emotionalMemories, query));
        }
        
        // 4. Get recent high-importance memories
        var recentImportant = _repository.GetRecentImportantMemories(npcId, query.RecencyThreshold);
        memories.AddRange(recentImportant);
        
        // 5. Deduplicate and rank
        var unique = memories.DistinctBy(m => m.Id);
        var ranked = RankByCompositeScore(unique, query);
        
        // 6. Slice to token budget
        return SliceToBudget(ranked, tokenBudget);
    }
    
    private MemoryContext SliceToBudget(IEnumerable<MemorySlice> memories, int tokenBudget)
    {
        var result = new MemoryContext();
        var currentTokens = 0;
        
        foreach (var memory in memories)
        {
            var memoryTokens = EstimateTokens(memory);
            if (currentTokens + memoryTokens > tokenBudget)
                break;
            
            result.Memories.Add(memory);
            currentTokens += memoryTokens;
        }
        
        return result;
    }
}
```

### Retrieval Query
```csharp
public class RetrievalQuery
{
    public string TargetPersonId { get; set; }
    public List<string> Topics { get; set; }
    public string EmotionalRelevance { get; set; }
    public TimeSpan RecencyThreshold { get; set; } = TimeSpan.FromDays(7);
    public double MinimumImportance { get; set; } = 0.3;
    public int MaxResults { get; set; } = 20;
}
```

---

## Memory Context for LLM

### Compact Context Packet
```json
{
  "relevant_memories": [
    {
      "type": "episodic",
      "description": "Player mocked Sarah's community",
      "importance": 0.7,
      "emotion": "anger",
      "recency": "2 days ago"
    },
    {
      "type": "belief",
      "subject": "player",
      "belief": "is arrogant",
      "confidence": 0.75
    }
  ],
  "relationship_with_target": {
    "affinity": 0.2,
    "trust": -0.3,
    "hostility": 0.8
  },
  "total_token_estimate": "~450"
}
```

---

## Memory Importance Scoring

### Factors
```csharp
public double CalculateMemoryImportance(
    MemoryCreationContext context)
{
    double importance = 0.0;
    
    // Player involvement
    if (context.InvolvesPlayer)
        importance += 0.4;
    
    // Emotional intensity
    importance += context.EmotionalIntensity * 0.3;
    
    // Audience size
    importance += Math.Min(context.AudienceSize / 1000, 1.0) * 0.2;
    
    // Relationship impact
    importance += context.RelationshipImpact * 0.2;
    
    // Novelty
    if (!context.IsNovel)
        importance *= 0.5;
    
    // Controversy
    if (context.IsControversial)
        importance += 0.15;
    
    // Relationship-defining events
    if (context.IsRelationshipDefining)
        importance = Math.Max(importance, 0.8);
    
    return Math.Clamp(importance, 0.0, 1.0);
}
```

### Importance Levels
| Score | Level | Examples |
|-------|-------|----------|
| 0.01 | Trivial | Random like |
| 0.05 | Minor | Generic comment |
| 0.10 | Low | Routine follow |
| 0.30 | Moderate | Interesting post |
| 0.50 | Significant | Meaningful conversation |
| 0.70 | Major | Public argument |
| 0.85 | Critical | Relationship-defining event |
| 1.00 | Maximum | Direct player interaction |

---

## Memory Decay

### Decay Rules
Not all memories deserve equal permanence.

**Decayable (importance < 0.5)**:
- Can fade over time
- May compress or become less detailed
- May be forgotten entirely

**Durable (importance >= 0.5)**:
- Persist indefinitely
- Retain emotional weight
- Never randomly erased for storage

```csharp
public class MemoryDecayService
{
    public void ProcessDecay(string npcId)
    {
        var decayableMemories = _repository.GetDecayableMemories(npcId);
        
        foreach (var memory in decayableMemories)
        {
            var daysSinceCreation = (DateTimeOffset.UtcNow - memory.CreatedAt).TotalDays;
            var decayRate = GetDecayRate(memory);
            var decayAmount = daysSinceCreation * decayRate * memory.Importance;
            
            memory.Importance -= decayAmount;
            memory.Confidence -= decayAmount * 0.5;
            
            if (memory.Importance < 0.05)
            {
                // Compress to summary or delete
                _repository.CompressOrDelete(memory);
            }
            else
            {
                _repository.Update(memory);
            }
        }
    }
    
    private double GetDecayRate(EpisodicMemory memory)
    {
        return memory.Type switch
        {
            "daily_interaction" => 0.1,
            "social_event" => 0.05,
            "significant_event" => 0.02,
            "relationship_event" => 0.01,
            _ => 0.1
        };
    }
}
```

---

## Memory Creation Triggers

### Automatic Creation
```csharp
// On every significant event
public async Task OnEventProcessed(DomainEvent evt)
{
    var importance = CalculateMemoryImportance(evt);
    
    if (importance >= 0.1)  // Minimum threshold
    {
        await CreateMemory(evt, importance);
    }
}

// On player interaction
public async Task OnPlayerInteraction(PlayerInteraction interaction)
{
    var importance = CalculatePlayerInteractionImportance(interaction);
    await CreateMemory(interaction, importance);
}
```

### Memory Creation Rules
| Event | Auto-Create | Importance Threshold |
|-------|-------------|---------------------|
| Player DM | Yes | 0.1 |
| Player comment | Yes | 0.1 |
| Player post | Yes | 0.2 |
| NPC post | If visible | 0.1 |
| Public argument | Yes | 0.4 |
| Relationship change | Yes | 0.3 |
| Community event | If member | 0.2 |
| Rumor heard | Yes | 0.2 |

---

## Belief Update System

### Belief Modification
```csharp
public class BeliefUpdateService
{
    public void UpdateBelief(string npcId, BeliefUpdate update)
    {
        var existingBelief = _repository.GetBelief(npcId, update.Subject);
        
        if (existingBelief == null)
        {
            // Create new belief
            CreateBelief(npcId, update);
        }
        else
        {
            // Update existing belief
            UpdateExistingBelief(existingBelief, update);
        }
    }
    
    private void UpdateExistingBelief(SemanticBelief belief, BeliefUpdate update)
    {
        var evidenceWeight = CalculateEvidenceWeight(update.Source, update.Confidence);
        
        if (update.SupportsBelief)
        {
            belief.SupportingEvidence.Add(update.Evidence);
            belief.Confidence = Math.Min(belief.Confidence + evidenceWeight * 0.1, 1.0);
        }
        else
        {
            belief.ConflictingEvidence.Add(update.Evidence);
            belief.Confidence = Math.Max(belief.Confidence - evidenceWeight * 0.1, 0.0);
        }
        
        belief.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
```

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [SIMULATION.md](./SIMULATION.md) - NPC behavior system
- [AI_SYSTEM.md](./AI_SYSTEM.md) - LLM integration
- [SOCIAL_GRAPH.md](./SOCIAL_GRAPH.md) - Relationship system
