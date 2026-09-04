# Architecture Decision Records

## Synthetic Social World - Significant Technical Decisions

---

## ADR-001: ASP.NET Core for Backend

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Need a backend server for the social simulation that supports:
- REST API endpoints
- WebSocket connections
- Background services
- Dependency injection
- Async/await throughout
- Structured logging

**Decision**: Use ASP.NET Core 8

**Alternatives Considered**:
- Node.js/Express: Familiar but less type safety
- Python/FastAPI: Good but less mature DI
- Go: Performance good but less productivity features

**Consequences**:
- POSITIVE: First-class async/await, dependency injection, WebSocket support
- POSITIVE: Strong typing with C#, excellent tooling
- POSITIVE: Entity Framework Core for database abstraction
- NEGATIVE: Larger memory footprint than Go
- NEGATIVE: Cold start time on serverless (not applicable here)

---

## ADR-002: SQLite as Initial Database

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Need a persistent database for world state that:
- Is simple to set up and maintain
- Supports WAL mode for concurrent reads
- Can be migrated to PostgreSQL later
- Handles moderate write volume with serialization

**Decision**: Use SQLite with WAL mode, Entity Framework Core

**Alternatives Considered**:
- PostgreSQL: Overkill for initial development
- MySQL: Not as good PostgreSQL compatibility
- NoSQL: Would require rebuilding domain for SQL

**Consequences**:
- POSITIVE: Zero configuration, portable, file-based
- POSITIVE: WAL mode allows concurrent reads
- POSITIVE: EF Core allows PostgreSQL migration
- NEGATIVE: Single-writer bottleneck (mitigated with write queue)
- NEGATIVE: Not suitable for true distributed deployment (acceptable for v1)

---

## ADR-003: Modular Monolith Architecture

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Need to structure the backend with clear separation but without premature microservices complexity.

**Decision**: Single ASP.NET Core solution with logical projects:
- Domain (no external dependencies)
- Infrastructure (database, AI providers)
- Simulation (NPC behavior, world state)
- Api (controllers, WebSocket handlers)

**Alternatives Considered**:
- Microservices: Premature for single-server deployment
- Single project: Loses logical boundaries
- Vertical slices: More complex than needed for v1

**Consequences**:
- POSITIVE: Clear separation of concerns
- POSITIVE: Easy to extract services later if needed
- POSITIVE: Simple deployment
- NEGATIVE: Some cross-project dependencies to manage
- NEGATIVE: Slower build times than single project

---

## ADR-004: LLM as Expression Layer Only

**Date**: 2025-09-03

**Status**: Accepted

**Context**: The simulation must work without AI, and AI must never be authoritative over world state.

**Decision**: 
1. All authoritative state owned by deterministic simulation code
2. LLM called only for natural language generation
3. Engine validates and applies LLM-proposed actions
4. Tiered system: Tier 1 (deterministic), Tier 2 (utility), Tier 3 (LLM)

**Alternatives Considered**:
- LLM as simulation engine: Dangerous, non-deterministic, expensive
- LLM for everything: Terrible performance, expensive
- No LLM: Missing natural language richness

**Consequences**:
- POSITIVE: Simulation survives AI failures
- POSITIVE: Cost predictable and controllable
- POSITIVE: Deterministic behavior for testing
- NEGATIVE: Must maintain both simulation logic and LLM integration
- NEGATIVE: Some AI-generated content may feel less contextual

---

## ADR-005: Ollama + Qwen3-4B for Local AI

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Need local AI inference that:
- Runs on the PC/server (not phone)
- Is affordable (no per-token costs)
- Provides reasonable quality
- Can be swapped for different models

**Decision**: Use Ollama with Qwen3-4B-Instruct-2507

**Alternatives Considered**:
- OpenAI API: Expensive, requires internet, privacy concerns
- Claude API: Same issues as OpenAI
- Larger local models: VRAM requirements too high
- No AI: Missing natural language quality

**Consequences**:
- POSITIVE: Runs locally, no API costs
- POSITIVE: Privacy preserved
- POSITIVE: Model can be swapped
- POSITIVE: Ollama provides simple API
- NEGATIVE: Hardware requirements (4B fits in ~4GB VRAM)
- NEGATIVE: Quality vs larger cloud models
- NEGATIVE: Must download and manage model

---

## ADR-006: Event-Driven Simulation

**Date**: 2025-09-03

**Status**: Accepted

**Context**: World state changes must be tracked, persisted, and can trigger downstream effects.

**Decision**: Use domain events for all significant state changes:
- PostCreated, CommentCreated, RelationshipChanged, etc.
- Events stored in database for audit/history
- Event handlers for side effects (notifications, AI jobs)
- Channel<DomainEvent> for serialized write pipeline

**Alternatives Considered**:
- Direct method calls: Harder to track, no history
- Message bus: Overkill for single process
- No events: Difficult to extend

**Consequences**:
- POSITIVE: Complete audit trail
- POSITIVE: Decoupled handlers
- POSITIVE: Easy to add new event types
- NEGATIVE: More complexity than direct calls
- NEGATIVE: Event ordering matters

---

## ADR-007: Scheduled Actions Over Continuous Ticking

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Processing every NPC every tick would be O(N²) and wasteful.

**Decision**: 
1. Each NPC has a next scheduled action time
2. Scheduler processes actions when due
3. After execution, NPC schedules next action
4. Background scheduler wakes to process due actions

**Alternatives Considered**:
- Continuous ticking: Wastes CPU on inactive NPCs
- Random delay: No good distribution
- Event-driven only: Hard to ensure coverage

**Consequences**:
- POSITIVE: O(active NPCs) not O(all NPCs)
- POSITIVE: Natural activity distribution
- POSITIVE: Easy to implement pause
- NEGATIVE: Requires good scheduling algorithm
- NEGATIVE: May miss spontaneous interactions

---

## ADR-008: AI Priority Queue

**Date**: 2025-09-03

**Status**: Accepted

**Context**: AI requests must not starve player interactions.

**Decision**: Implement priority queue with levels:
- 100: Direct player interaction
- 95: Player DM
- 90: Player reply
- 80: Major relationship event
- 70: Major public drama
- 60: Important NPC conversation
- 40: Meaningful background content
- 20: Ordinary NPC content
- 10: Trivial background chatter

**Alternatives Considered**:
- FIFO queue: Player gets same priority as background
- Separate queues: More complex scheduling
- No queue: Disaster

**Consequences**:
- POSITIVE: Player always gets best response
- POSITIVE: Background work happens when possible
- POSATIVE: Easy to tune priorities
- NEGATIVE: Low priority jobs may wait long
- NEGATIVE: Queue monitoring needed

---

## ADR-009: Compact LLM Context (~512 tokens)

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Sending entire NPC state to LLM would be expensive and slow.

**Decision**: 
1. Retrieve only relevant memories (target person, topic, emotion)
2. Build compact context packet
3. Include NPC state summary, relationship, current situation
4. Target ~512 tokens for memory/retrieved content
5. Add compact system prompt, current state, conversation

**Alternatives Considered**:
- Full context: Expensive, slow, exceeds context window
- No context: Loses personality consistency
- Large context: Requires larger models

**Consequences**:
- POSITIVE: Fast inference
- POSITIVE: Predictable costs
- POSITIVE: Works with 4B model
- NEGATIVE: May miss some context
- NEGATIVE: Memory retrieval must be good

---

## ADR-010: Android Native with Jetpack Compose

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Need a native Android client for the social app.

**Decision**: 
- Kotlin with Jetpack Compose
- MVVM + Clean Architecture
- Hilt for dependency injection
- Retrofit for REST, OkHttp WebSocket for real-time
- StateFlow/Flow for reactive state
- Paging 3 for lists

**Alternatives Considered**:
- Flutter: Extra complexity, less native feel
- React Native: Less native performance
- WebView: Terrible performance and UX

**Consequences**:
- POSITIVE: Native performance
- POSITIVE: Material Design 3
- POSITIVE: Excellent tooling
- POSITIVE: Large ecosystem
- NEGATIVE: iOS requires separate codebase
- NEGATIVE: Build times can be slow

---

## ADR-011: Multi-Dimensional Relationships

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Simple "friendship" relationship is unrealistic for social simulation.

**Decision**: Implement directional, multi-dimensional relationships:
- Affinity, Trust, Respect, Attraction
- Hostility, Jealousy, Fear
- Admiration, Resentment, Familiarity
- Each NPC has own relationship TO every other NPC

**Alternatives Considered**:
- Single number: Too simplistic
- Fixed types (friend/enemy): Too rigid
- No relationships: Breaks simulation

**Consequences**:
- POSITIVE: Rich social dynamics
- POSITIVE: Realistic NPC behavior
- POSITIVE: Supports romance, rivalry, complex dynamics
- NEGATIVE: More complex to compute
- NEGATIVE: More storage required

---

## ADR-012: Offline World Simulation

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Player should return after hours to find changed world, but not wait for hundreds of AI generations.

**Decision**:
1. On disconnect, record timestamp
2. Calculate elapsed time
3. Execute deterministic simulation for elapsed time
4. Extract high-importance events only
5. Generate "while you were away" summary
6. Queue LLM jobs only for high-value events
7. Restore world state immediately for player

**Alternatives Considered**:
- Full simulation catch-up: Takes too long
- No offline simulation: World doesn't feel alive
- Cloud simulation: Complex infrastructure

**Consequences**:
- POSITIVE: Player returns quickly
- POSITIVE: World continues
- POSITIVE: AI budget controlled
- NEGATIVE: Some detail lost
- NEGATIVE: Requires good event extraction

---

## ADR-013: Tiered Behavioral LOD

**Date**: 2025-09-03

**Status**: Accepted

**Context**: Cannot afford equal computational attention for all entities.

**Decision**: Implement two LOD systems:
1. **Behavioral LOD**: 
   - Tier 1: Deterministic (sleep, work, like)
   - Tier 2: Utility decision (action selection)
   - Tier 3: LLM expression (high-value interactions)

2. **Social LOD**:
   - HOT: Player, interactors, trending, conflicts
   - WARM: Active NPCs, popular posts
   - COLD: Inactive NPCs, background noise

**Alternatives Considered**:
- Uniform processing: Too expensive
- Random sampling: Loses consistency

**Consequences**:
- POSITIVE: Scalable to thousands of NPCs
- POSITIVE: Resources where they matter
- POSITIVE: Natural behavior variation
- NEGATIVE: Some behavior less detailed
- NEGATIVE: LOD boundaries may cause artifacts

---

## Future ADRs to Create

When significant decisions are made, add here with format:
- Title
- Date
- Status (Proposed/Accepted/Rejected/Deprecated/Superseded)
- Context
- Decision
- Alternatives Considered
- Consequences

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [ROADMAP.md](./ROADMAP.md) - Implementation phases
