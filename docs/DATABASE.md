# Database Architecture

## Synthetic Social World - SQLite Schema Design

---

## Design Principles

1. **Source of Truth**: Persistent world state belongs in database, not RAM
2. **PostgreSQL-Ready**: Architecture allows future migration without domain rebuild
3. **WAL Mode**: Concurrent readers with serialized writers
4. **Controlled Concurrency**: Single write pipeline prevents SQLITE_BUSY
5. **Indexed Queries**: All foreign keys and query paths indexed
6. **Parameterized SQL**: All queries use parameters, never string concatenation
7. **Migrations**: Explicit versioning, never casual schema changes

---

## Schema Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        CORE ENTITIES                            │
├─────────────────────────────────────────────────────────────────┤
│  Worlds                                                         │
│  NPCs                                                           │
│  Posts                                                          │
│  Comments                                                       │
│  Messages                                                       │
│  Communities                                                    │
│  Events                                                         │
│  Notifications                                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     RELATIONSHIP ENTITIES                        │
├─────────────────────────────────────────────────────────────────┤
│  NPCRelationships (directional, multi-dimensional)              │
│  Follows                                                        │
│  CommunityMembers                                               │
│  EventAttendees                                                 │
│  PostLikes                                                      │
│  PostDislikes                                                   │
│  PostShares                                                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      MEMORY ENTITIES                             │
├─────────────────────────────────────────────────────────────────┤
│  EpisodicMemories                                               │
│  SemanticBeliefs                                                │
│  SocialMemories                                                 │
│  Rumors                                                         │
│  KnowledgeEntries                                               │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       STATE ENTITIES                             │
├─────────────────────────────────────────────────────────────────┤
│  NPCPersonalities                                               │
│  NPCInterests                                                   │
│  NPCGoals                                                       │
│  NPCMoods                                                       │
│  WorldClock                                                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       EVENT ENTITIES                             │
├─────────────────────────────────────────────────────────────────┤
│  DomainEvents                                                   │
│  ScheduledActions                                               │
│  SimulationLogs                                                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       SYSTEM ENTITIES                            │
├─────────────────────────────────────────────────────────────────┤
│  SchemaVersions                                                 │
│  WorldBackups                                                   │
│  FeatureFlags                                                   │
│  Configuration                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Entity Definitions

### Worlds
```sql
CREATE TABLE Worlds (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    LastProcessedAt TEXT NOT NULL,
    WorldSpeed REAL NOT NULL DEFAULT 1.0,
    IsPaused INTEGER NOT NULL DEFAULT 0,
    Version INTEGER NOT NULL DEFAULT 1
);
```

### NPCs
```sql
CREATE TABLE NPCs (
    Id TEXT PRIMARY KEY,
    WorldId TEXT NOT NULL REFERENCES Worlds(Id),
    Handle TEXT NOT NULL UNIQUE,
    DisplayName TEXT NOT NULL,
    Bio TEXT,
    AvatarUrl TEXT,
    CreatedAt TEXT NOT NULL,
    LastActiveAt TEXT NOT NULL,
    IsPlayer INTEGER NOT NULL DEFAULT 0,
    ActivityLevel REAL NOT NULL DEFAULT 0.5,
    Popularity REAL NOT NULL DEFAULT 0.0,
    FollowerCount INTEGER NOT NULL DEFAULT 0,
    FollowingCount INTEGER NOT NULL DEFAULT 0,
    Reputation REAL NOT NULL DEFAULT 0.0,
    INDEX IX_NPCs_WorldId (WorldId),
    INDEX IX_NPCs_Handle (Handle),
    INDEX IX_NPCs_LastActiveAt (LastActiveAt)
);
```

### Posts
```sql
CREATE TABLE Posts (
    Id TEXT PRIMARY KEY,
    AuthorId TEXT NOT NULL REFERENCES NPCs(Id),
    CommunityId TEXT REFERENCES Communities(Id),
    Content TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    LikeCount INTEGER NOT NULL DEFAULT 0,
    DislikeCount INTEGER NOT NULL DEFAULT 0,
    CommentCount INTEGER NOT NULL DEFAULT 0,
    ShareCount INTEGER NOT NULL DEFAULT 0,
    ViewCount INTEGER NOT NULL DEFAULT 0,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    ImportanceScore REAL NOT NULL DEFAULT 0.1,
    INDEX IX_Posts_AuthorId (AuthorId),
    INDEX IX_Posts_CommunityId (CommunityId),
    INDEX IX_Posts_CreatedAt (CreatedAt),
    INDEX IX_Posts_ImportanceScore (ImportanceScore)
);
```

### Comments
```sql
CREATE TABLE Comments (
    Id TEXT PRIMARY KEY,
    PostId TEXT NOT NULL REFERENCES Posts(Id),
    AuthorId TEXT NOT NULL REFERENCES NPCs(Id),
    ParentCommentId TEXT REFERENCES Comments(Id),
    Content TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    LikeCount INTEGER NOT NULL DEFAULT 0,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    INDEX IX_Comments_PostId (PostId),
    INDEX IX_Comments_AuthorId (AuthorId),
    INDEX IX_Comments_ParentCommentId (ParentCommentId)
);
```

### Messages
```sql
CREATE TABLE Messages (
    Id TEXT PRIMARY KEY,
    SenderId TEXT NOT NULL REFERENCES NPCs(Id),
    RecipientId TEXT NOT NULL REFERENCES NPCs(Id),
    Content TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    IsRead INTEGER NOT NULL DEFAULT 0,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    INDEX IX_Messages_SenderId (SenderId),
    INDEX IX_Messages_RecipientId (RecipientId),
    INDEX IX_Messages_CreatedAt (CreatedAt)
);
```

### Communities
```sql
CREATE TABLE Communities (
    Id TEXT PRIMARY KEY,
    WorldId TEXT NOT NULL REFERENCES Worlds(Id),
    Name TEXT NOT NULL,
    Handle TEXT NOT NULL UNIQUE,
    Topic TEXT,
    Description TEXT,
    Rules TEXT,
    CultureScore REAL NOT NULL DEFAULT 0.5,
    ToxicityLevel REAL NOT NULL DEFAULT 0.0,
    Popularity REAL NOT NULL DEFAULT 0.0,
    MemberCount INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    CreatedById TEXT REFERENCES NPCs(Id),
    INDEX IX_Communities_WorldId (WorldId),
    INDEX IX_Communities_Handle (Handle),
    INDEX IX_Communities_Popularity (Popularity)
);
```

### Events
```sql
CREATE TABLE Events (
    Id TEXT PRIMARY KEY,
    CommunityId TEXT REFERENCES Communities(Id),
    OrganizerId TEXT NOT NULL REFERENCES NPCs(Id),
    Title TEXT NOT NULL,
    Description TEXT,
    EventType TEXT NOT NULL,
    Location TEXT,
    StartTime TEXT NOT NULL,
    EndTime TEXT,
    AttendeeCount INTEGER NOT NULL DEFAULT 0,
    MaxAttendees INTEGER,
    Popularity REAL NOT NULL DEFAULT 0.0,
    CreatedAt TEXT NOT NULL,
    INDEX IX_Events_CommunityId (CommunityId),
    INDEX IX_Events_OrganizerId (OrganizerId),
    INDEX IX_Events_StartTime (StartTime)
);
```

### Notifications
```sql
CREATE TABLE Notifications (
    Id TEXT PRIMARY KEY,
    RecipientId TEXT NOT NULL REFERENCES NPCs(Id),
    Type TEXT NOT NULL,
    Title TEXT NOT NULL,
    Body TEXT,
    RelatedEntityId TEXT,
    RelatedEntityType TEXT,
    IsRead INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    INDEX IX_Notifications_RecipientId (RecipientId),
    INDEX IX_Notifications_IsRead (IsRead),
    INDEX IX_Notifications_CreatedAt (CreatedAt)
);
```

### NPCRelationships (Multi-dimensional, Directional)
```sql
CREATE TABLE NPCRelationships (
    Id TEXT PRIMARY KEY,
    SourceNpcId TEXT NOT NULL REFERENCES NPCs(Id),
    TargetNpcId TEXT NOT NULL REFERENCES NPCs(Id),
    Affinity REAL NOT NULL DEFAULT 0.0,
    Trust REAL NOT NULL DEFAULT 0.0,
    Respect REAL NOT NULL DEFAULT 0.0,
    Attraction REAL NOT NULL DEFAULT 0.0,
    Hostility REAL NOT NULL DEFAULT 0.0,
    Jealousy REAL NOT NULL DEFAULT 0.0,
    Fear REAL NOT NULL DEFAULT 0.0,
    Admiration REAL NOT NULL DEFAULT 0.0,
    Resentment REAL NOT NULL DEFAULT 0.0,
    Familiarity REAL NOT NULL DEFAULT 0.0,
    LastInteractionAt TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    UNIQUE(SourceNpcId, TargetNpcId),
    INDEX IX_Relationships_SourceNpcId (SourceNpcId),
    INDEX IX_Relationships_TargetNpcId (TargetNpcId)
);
```

### Follows
```sql
CREATE TABLE Follows (
    Id TEXT PRIMARY KEY,
    FollowerId TEXT NOT NULL REFERENCES NPCs(Id),
    FollowedId TEXT NOT NULL REFERENCES NPCs(Id),
    CreatedAt TEXT NOT NULL,
    UNIQUE(FollowerId, FollowedId),
    INDEX IX_Follows_FollowerId (FollowerId),
    INDEX IX_Follows_FollowedId (FollowedId)
);
```

### CommunityMembers
```sql
CREATE TABLE CommunityMembers (
    Id TEXT PRIMARY KEY,
    CommunityId TEXT NOT NULL REFERENCES Communities(Id),
    NpcId TEXT NOT NULL REFERENCES NPCs(Id),
    Role TEXT NOT NULL DEFAULT 'member',
    JoinedAt TEXT NOT NULL,
    UNIQUE(CommunityId, NpcId),
    INDEX IX_CommunityMembers_CommunityId (CommunityId),
    INDEX IX_CommunityMembers_NpcId (NpcId)
);
```

### EventAttendees
```sql
CREATE TABLE EventAttendees (
    Id TEXT PRIMARY KEY,
    EventId TEXT NOT NULL REFERENCES Events(Id),
    NpcId TEXT NOT NULL REFERENCES NPCs(Id),
    Status TEXT NOT NULL DEFAULT 'attending',
    CreatedAt TEXT NOT NULL,
    UNIQUE(EventId, NpcId),
    INDEX IX_EventAttendees_EventId (EventId),
    INDEX IX_EventAttendees_NpcId (NpcId)
);
```

### PostEngagement
```sql
CREATE TABLE PostEngagement (
    Id TEXT PRIMARY KEY,
    PostId TEXT NOT NULL REFERENCES Posts(Id),
    NpcId TEXT NOT NULL REFERENCES NPCs(Id),
    Type TEXT NOT NULL, -- 'like', 'dislike', 'share'
    CreatedAt TEXT NOT NULL,
    UNIQUE(PostId, NpcId, Type),
    INDEX IX_PostEngagement_PostId (PostId),
    INDEX IX_PostEngagement_NpcId (NpcId)
);
```

### NPCPersonalities (Multi-dimensional traits)
```sql
CREATE TABLE NPCPersonalities (
    Id TEXT PRIMARY KEY,
    NpcId TEXT NOT NULL UNIQUE REFERENCES NPCs(Id),
    Openness REAL NOT NULL DEFAULT 0.5,
    Extroversion REAL NOT NULL DEFAULT 0.5,
    Agreeableness REAL NOT NULL DEFAULT 0.5,
    Conscientiousness REAL NOT NULL DEFAULT 0.5,
    Neuroticism REAL NOT NULL DEFAULT 0.5,
    Confidence REAL NOT NULL DEFAULT 0.5,
    Empathy REAL NOT NULL DEFAULT 0.5,
    Sarcasm REAL NOT NULL DEFAULT 0.5,
    Humor REAL NOT NULL DEFAULT 0.5,
    Aggression REAL NOT NULL DEFAULT 0.5,
    Curiosity REAL NOT NULL DEFAULT 0.5,
    Impulsiveness REAL NOT NULL DEFAULT 0.5,
    Patience REAL NOT NULL DEFAULT 0.5,
    Competitiveness REAL NOT NULL DEFAULT 0.5,
    Jealousy REAL NOT NULL DEFAULT 0.5,
    Conformity REAL NOT NULL DEFAULT 0.5,
    Independence REAL NOT NULL DEFAULT 0.5,
    RiskTolerance REAL NOT NULL DEFAULT 0.5,
    Sociability REAL NOT NULL DEFAULT 0.5,
    UpdatedAt TEXT NOT NULL,
    INDEX IX_NPCPersonalities_NpcId (NpcId)
);
```

### NPCInterests
```sql
CREATE TABLE NPCInterests (
    Id TEXT PRIMARY KEY,
    NpcId TEXT NOT NULL REFERENCES NPCs(Id),
    Topic TEXT NOT NULL,
    Weight REAL NOT NULL DEFAULT 0.5,
    UNIQUE(NpcId, Topic),
    INDEX IX_NPCInterests_NpcId (NpcId),
    INDEX IX_NPCInterests_Topic (Topic)
);
```

### NPCGoals
```sql
CREATE TABLE NPCGoals (
    Id TEXT PRIMARY KEY,
    NpcId TEXT NOT NULL REFERENCES NPCs(Id),
    GoalType TEXT NOT NULL,
    Priority REAL NOT NULL DEFAULT 0.5,
    Progress REAL NOT NULL DEFAULT 0.0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    INDEX IX_NPCGoals_NpcId (NpcId)
);
```

### NPCMoods
```sql
CREATE TABLE NPCMoods (
    Id TEXT PRIMARY KEY,
    NpcId TEXT NOT NULL UNIQUE REFERENCES NPCs(Id),
    Happiness REAL NOT NULL DEFAULT 0.5,
    Sadness REAL NOT NULL DEFAULT 0.0,
    Anger REAL NOT NULL DEFAULT 0.0,
    Excitement REAL NOT NULL DEFAULT 0.0,
    Anxiety REAL NOT NULL DEFAULT 0.0,
    Embarrassment REAL NOT NULL DEFAULT 0.0,
    Affection REAL NOT NULL DEFAULT 0.0,
    Jealousy REAL NOT NULL DEFAULT 0.0,
    Loneliness REAL NOT NULL DEFAULT 0.0,
    Confidence REAL NOT NULL DEFAULT 0.5,
    PrimaryMood TEXT NOT NULL DEFAULT 'neutral',
    UpdatedAt TEXT NOT NULL,
    INDEX IX_NPCMoods_NpcId (NpcId)
);
```

### EpisodicMemories
```sql
CREATE TABLE EpisodicMemories (
    Id TEXT PRIMARY KEY,
    OwnerId TEXT NOT NULL REFERENCES NPCs(Id),
    EventType TEXT NOT NULL,
    Description TEXT NOT NULL,
    Participants TEXT, -- JSON array of NPC IDs
    Importance REAL NOT NULL DEFAULT 0.1,
    Emotion TEXT,
    Timestamp TEXT NOT NULL,
    Source TEXT NOT NULL,
    Confidence REAL NOT NULL DEFAULT 1.0,
    CreatedAt TEXT NOT NULL,
    INDEX IX_EpisodicMemories_OwnerId (OwnerId),
    INDEX IX_EpisodicMemories_Importance (Importance),
    INDEX IX_EpisodicMemories_Timestamp (Timestamp)
);
```

### SemanticBeliefs
```sql
CREATE TABLE SemanticBeliefs (
    Id TEXT PRIMARY KEY,
    OwnerId TEXT NOT NULL REFERENCES NPCs(Id),
    Subject TEXT NOT NULL,
    Belief TEXT NOT NULL,
    Confidence REAL NOT NULL DEFAULT 0.5,
    SupportingEvidence TEXT, -- JSON
    ConflictingEvidence TEXT, -- JSON
    Source TEXT NOT NULL,
    Timestamp TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    INDEX IX_SemanticBeliefs_OwnerId (OwnerId),
    INDEX IX_SemanticBeliefs_Subject (Subject)
);
```

### SocialMemories
```sql
CREATE TABLE SocialMemories (
    Id TEXT PRIMARY KEY,
    OwnerId TEXT NOT NULL REFERENCES NPCs(Id),
    Description TEXT NOT NULL,
    Participants TEXT, -- JSON array
    RelationshipType TEXT,
    Importance REAL NOT NULL DEFAULT 0.1,
    Timestamp TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    INDEX IX_SocialMemories_OwnerId (OwnerId),
    INDEX IX_SocialMemories_Timestamp (Timestamp)
);
```

### Rumors
```sql
CREATE TABLE Rumors (
    Id TEXT PRIMARY KEY,
    OriginatorId TEXT NOT NULL REFERENCES NPCs(Id),
    Subject TEXT NOT NULL,
    Content TEXT NOT NULL,
    Confidence REAL NOT NULL DEFAULT 0.5,
    SourceChain TEXT NOT NULL, -- JSON array of spreading NPCs
    SpreadCount INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    INDEX IX_Rumors_Subject (Subject),
    INDEX IX_Rumors_CreatedAt (CreatedAt)
);
```

### KnowledgeEntries (What NPC knows)
```sql
CREATE TABLE KnowledgeEntries (
    Id TEXT PRIMARY KEY,
    NpcId TEXT NOT NULL REFERENCES NPCs(Id),
    EntityType TEXT NOT NULL, -- 'post', 'comment', 'event', 'npc', 'community'
    EntityId TEXT NOT NULL,
    KnowledgeType TEXT NOT NULL, -- 'observed', 'told', 'read', 'inferred', 'learned'
    Confidence REAL NOT NULL DEFAULT 1.0,
    AcquiredAt TEXT NOT NULL,
    SourceId TEXT REFERENCES NPCs(Id),
    INDEX IX_Knowledge_NpcId (NpcId),
    INDEX IX_Knowledge_Entity (EntityType, EntityId)
);
```

### ScheduledActions
```sql
CREATE TABLE ScheduledActions (
    Id TEXT PRIMARY KEY,
    NpcId TEXT NOT NULL REFERENCES NPCs(Id),
    ActionType TEXT NOT NULL,
    TargetType TEXT,
    TargetId TEXT,
    ScheduledFor TEXT NOT NULL,
    Priority INTEGER NOT NULL DEFAULT 0,
    Parameters TEXT, -- JSON
    IsExecuted INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    INDEX IX_ScheduledActions_NpcId (NpcId),
    INDEX IX_ScheduledActions_ScheduledFor (ScheduledFor),
    INDEX IX_ScheduledActions_Priority (Priority)
);
```

### DomainEvents
```sql
CREATE TABLE DomainEvents (
    Id TEXT PRIMARY KEY,
    EventType TEXT NOT NULL,
    EntityType TEXT NOT NULL,
    EntityId TEXT NOT NULL,
    Payload TEXT NOT NULL, -- JSON
    WorldTime TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    IsProcessed INTEGER NOT NULL DEFAULT 0,
    INDEX IX_DomainEvents_CreatedAt (CreatedAt),
    INDEX IX_DomainEvents_IsProcessed (IsProcessed)
);
```

### SchemaVersions
```sql
CREATE TABLE SchemaVersions (
    Version INTEGER PRIMARY KEY,
    AppliedAt TEXT NOT NULL,
    Description TEXT
);
```

### WorldBackups
```sql
CREATE TABLE WorldBackups (
    Id TEXT PRIMARY KEY,
    WorldId TEXT NOT NULL,
    BackupType TEXT NOT NULL, -- 'full', 'incremental'
    FilePath TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    WorldVersion INTEGER NOT NULL,
    SchemaVersion INTEGER NOT NULL
);
```

### FeatureFlags
```sql
CREATE TABLE FeatureFlags (
    Key TEXT PRIMARY KEY,
    IsEnabled INTEGER NOT NULL DEFAULT 0,
    Description TEXT,
    UpdatedAt TEXT NOT NULL
);
```

---

## Write Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     APPLICATION CODE                            │
│  (Simulation, API Controllers, Event Handlers)                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     DOMAIN EVENTS                               │
│  PostCreated, CommentCreated, RelationshipChanged, etc.         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Channel<DomainEvent>                           │
│            Single serialized write queue                        │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                BACKGROUND PERSISTENCE WORKER                    │
│  - Dequeue events                                              │
│  - Batch by transaction type                                   │
│  - Execute within transaction                                   │
│  - Commit                                                       │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        SQLite WAL                               │
│  - Concurrent reads allowed                                     │
│  - Serialized writes                                            │
└─────────────────────────────────────────────────────────────────┘
```

---

## Batching Strategy

| Event Type | Batching | Batch Window |
|------------|----------|--------------|
| PostLike/Dislike | Yes | 100ms |
| ViewCount | Yes | 1s |
| Engagement counters | Yes | 5s |
| Memory creation | No | Immediate |
| Relationship change | No | Immediate |
| Post creation | No | Immediate |
| Notification | No | Immediate |

---

## Migration Strategy

1. Each schema change is a numbered migration
2. Migrations are applied in order
3. Old worlds are migratable
4. Rollback supported for development only
5. Schema version tracked in SchemaVersions table

Example migration naming:
- `001_InitialSchema.sql`
- `002_AddPersonalityTraits.sql`
- `003_AddKnowledgeSystem.sql`

---

## Index Strategy

### Primary Query Patterns
1. Feed retrieval: `Posts WHERE IsDeleted=0 ORDER BY CreatedAt DESC` (with ranking)
2. NPC lookup: `NPCs WHERE Id=?`
3. Relationship query: `NPCRelationships WHERE SourceNpcId=?`
4. Notification query: `Notifications WHERE RecipientId=? AND IsRead=0`
5. Scheduled actions: `ScheduledActions WHERE ScheduledFor <= ? AND IsExecuted=0 ORDER BY Priority DESC`

### All indexes created on:
- All foreign keys
- All timestamp fields used in ORDER BY
- All fields used in WHERE clauses
- Composite indexes for common query patterns

---

## Configuration

```json
{
  "Database": {
    "Provider": "sqlite",
    "ConnectionString": "Data Source=data/synthetic_social_world.db;Mode=ReadWriteCreate",
    "WALMode": true,
    "BusyTimeout": 5000,
    "CacheSize": 10000,
    "EnableForeignKeys": true
  }
}
```

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [SIMULATION.md](./SIMULATION.md) - NPC behavior system
