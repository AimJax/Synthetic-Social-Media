# Architecture Overview

## Synthetic Social World - System Architecture

### Project Mission
Build a persistent AI social network simulation where AI-controlled NPCs with rich internal states (personalities, memories, relationships, goals) interact with each other and a human player through a social media platform interface.

The goal is **maximum social believability per unit of computation**.

---

## Core Philosophy

```
WORLD → produces EVENTS
EVENTS → modify STATE
STATE → changes NPC DECISIONS
DECISIONS → produce SOCIAL ACTIONS
SOCIAL ACTIONS → create MEMORIES
MEMORIES → influence FUTURE DECISIONS
RELATIONSHIPS → modify SOCIAL INTERPRETATION
THE FEED → determines INFORMATION EXPOSURE
INFORMATION → propagates through SOCIAL NETWORKS
IMPORTANT EVENTS → invoke LLM EXPRESSION
LLM → produces LANGUAGE
LANGUAGE → becomes a SOCIAL EVENT
SOCIAL EVENTS → alter the WORLD
WORLD → continues without the PLAYER
PLAYER → returns and experiences the CONSEQUENCES
```

---

## Architectural Principles

### 1. Engine vs Expression Decoupling
- **Simulation Engine** owns all authoritative world state
- **LLM** provides natural language expression only
- The LLM NEVER directly owns state, writes to database, or mutates canonical state
- Engine validates and applies LLM-proposed actions

### 2. Deterministic Rules First
- Core simulation mechanics MUST work without LLM inference
- LLM failure reduces linguistic richness but does NOT destroy simulation continuity
- OLLAMA can be offline, overloaded, unavailable, or timing out

### 3. Tiered Behavioral LOD (Level of Detail)

| Tier | Description | LLM Required |
|------|-------------|--------------|
| Tier 1 | Deterministic/Background | ZERO |
| Tier 2 | Utility Decision System | ZERO unless NL generation |
| Tier 3 | LLM Expression | FULL |

### 4. Social LOD
Entities are treated differently based on importance:
- **HOT**: Player, direct interactors, trending content, major conflicts
- **WARM**: Active NPCs, popular posts, meaningful relationships
- **COLD**: Inactive NPCs, dormant communities, background noise

### 5. Domain / Infrastructure Separation
- Domain has NO dependencies on SQLite, HTTP, Android, WebSockets, or Ollama
- Simulation code should work identically regardless of persistence backend

---

## System Layers

```
┌─────────────────────────────────────────────────────────────┐
│                     ANDROID CLIENT                           │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────┐    │
│  │   UI    │  │  View   │  │ Network │  │   State     │    │
│  │ Compose │  │  Model  │  │  Layer  │  │   Manager    │    │
│  └─────────┘  └─────────┘  └─────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │ REST + WebSocket
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     ASP.NET CORE API                        │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────┐    │
│  │   REST  │  │   WS    │  │  Auth   │  │  Validation │    │
│  │Controller│ │ Handler │  │ Filter  │  │  Middleware │    │
│  └─────────┘  └─────────┘  └─────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    SIMULATION CORE                          │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────┐    │
│  │  World  │  │   NPC   │  │  Feed   │  │  Scheduler  │    │
│  │  Clock  │  │ System  │  │ Engine  │  │   Service   │    │
│  └─────────┘  └─────────┘  └─────────┘  └─────────────┘    │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────┐    │
│  │Relationship│ │ Memory │  │Community│  │    Event   │    │
│  │  System │  │ System  │  │ System  │  │   System    │    │
│  └─────────┘  └─────────┘  └─────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    AI ORCHESTRATION                         │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────┐    │
│  │AI Queue │  │ Priority│  │ Context │  │   Output    │    │
│  │Manager  │  │ System  │  │ Builder │  │  Validator   │    │
│  └─────────┘  └─────────┘  └─────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    AI PROVIDER                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │              IAIProvider Interface                   │   │
│  └─────────────────────────────────────────────────────┘   │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────┐    │
│  │ Ollama  │  │  Local  │  │ Remote  │  │    Mock     │    │
│  │Provider │  │Provider │  │Provider │  │   Provider  │    │
│  └─────────┘  └─────────┘  └─────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    PERSISTENCE LAYER                        │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────┐    │
│  │Write    │  │Repository│ │ Entity  │  │  Migration  │    │
│  │Pipeline │  │ Pattern │  │Framework│  │   Manager   │    │
│  └─────────┘  └─────────┘  └─────────┘  └─────────────┘    │
│                         │                                   │
│                         ▼                                   │
│              ┌─────────────────────┐                        │
│              │    SQLite (WAL)     │                        │
│              │   PostgreSQL-Ready  │                        │
│              └─────────────────────┘                        │
└─────────────────────────────────────────────────────────────┘
```

---

## Module Breakdown

### Backend Modules

| Module | Responsibility |
|--------|----------------|
| **Domain** | Core entities, value objects, domain events, interfaces |
| **Infrastructure** | SQLite, repositories, AI providers, external services |
| **Simulation** | NPC behavior, scheduler, world clock, social systems |
| **API** | REST controllers, WebSocket handlers, authentication |

### Domain Entities

```
World
├── WorldClock (authoritative persistent time)
├── NPCs[]
│   ├── Identity (name, handle, avatar)
│   ├── Personality (multi-dimensional traits)
│   ├── Interests[]
│   ├── Goals[]
│   ├── Mood (dynamic emotional state)
│   ├── Relationships[] (directional, multi-dimensional)
│   ├── Memories[]
│   │   ├── EpisodicMemory
│   │   ├── SemanticBelief
│   │   ├── SocialMemory
│   │   └── Rumor
│   ├── Knowledge (what NPC knows)
│   ├── ActivityProfile
│   └── Schedule (next actions)
├── Communities[]
│   ├── name, topic, rules, culture
│   ├── Members[]
│   └── Activity
├── Posts[]
├── Comments[]
├── Events[]
├── Messages[]
├── Notifications[]
├── Feed (personalized ranking)
└── SocialGraph
```

---

## Communication Patterns

### Client ↔ Server
- **REST API**: CRUD operations, player actions
- **WebSocket**: Realtime events, push notifications

### Server Internally
- **Domain Events**: State changes trigger events
- **Channel<DomainEvent>**: Serialized write pipeline
- **Background Services**: Hosted services for simulation ticks

---

## Data Flow

### Player Action Flow
```
1. Player performs action (like, post, comment, DM)
2. Android sends request to API
3. API validates request
4. Domain event created
5. Event handler processes:
   - Updates state
   - Creates notifications
   - Triggers NPC reactions
   - Queues AI jobs if needed
6. Domain event written to persistence
7. WebSocket pushes update to relevant clients
```

### NPC Decision Flow
```
1. Scheduler triggers NPC action time
2. NPC evaluates available actions using utility scoring
3. Personality + mood + relationships + goals modify scores
4. Best action selected (Tier 1/2) or AI job queued (Tier 3)
5. Action executed
6. Domain event created
7. State updated
8. Memory created if significant
```

---

## Technology Choices

| Component | Technology | Rationale |
|-----------|------------|-----------|
| Backend | ASP.NET Core 8 | Modern, async-first, DI, WebSocket support |
| Database | SQLite | Simple, portable, WAL mode, PostgreSQL migration path |
| ORM | Entity Framework Core | Standard .NET ORM, migrations, LINQ |
| AI Runtime | Ollama + Qwen3-4B | Local inference, privacy, no API costs |
| Android | Kotlin + Compose | Modern UI, reactive, Google standard |
| Networking | Retrofit + OkHttp | REST; Stomp protocol for WebSocket |
| State | StateFlow/Flow | Kotlin coroutines native |

---

## Non-Functional Requirements

### Performance
- Simulation must handle 1000+ NPCs eventually
- AI queue must prioritize player interactions
- Feed ranking must be < 100ms for first page
- Database writes serialized to prevent SQLITE_BUSY

### Reliability
- LLM failure must not halt simulation
- Database must survive server restart
- Offline progression must be resumable

### Observability
- Structured logging for all major events
- Metrics for AI latency, queue depth, simulation throughput
- Debug tools for NPC inspection

---

## Security Considerations

- Validate ALL input (server authority)
- Parameterized SQL queries
- No direct SQLite exposure
- No Ollama exposure to clients
- NPC private state never sent to other NPCs
- Admin functions protected

---

## Future Migration Path

### SQLite → PostgreSQL
Architecture is designed to allow this migration without rebuilding domain model.

### Qwen 4B → Larger Model
Model-agnostic prompt versioning allows model swapping.

### Single Server → Distributed
Modular monolith allows future extraction if needed.

---

## Related Documents

- [DATABASE.md](./DATABASE.md) - Schema design
- [SIMULATION.md](./SIMULATION.md) - NPC behavior system
- [AI_SYSTEM.md](./AI_SYSTEM.md) - Ollama integration
- [MEMORY_SYSTEM.md](./MEMORY_SYSTEM.md) - Memory architecture
- [SOCIAL_GRAPH.md](./SOCIAL_GRAPH.md) - Relationship system
- [FEED_SYSTEM.md](./FEED_SYSTEM.md) - Feed ranking
- [API.md](./API.md) - REST and WebSocket endpoints
- [ANDROID.md](./ANDROID.md) - Client architecture
