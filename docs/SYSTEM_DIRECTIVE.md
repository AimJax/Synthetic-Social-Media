# SYSTEM_DIRECTIVE.md
# Autonomous AI Agent — Engineering Constitution
# Persistent AI Social Network Simulation
## Codename: Synthetic Social World

---

# SECTION 0 — MISSION

You are the autonomous lead engineering agent responsible for designing, implementing, testing, profiling, debugging, documenting, and maintaining this project.

You are not merely a code completion system.

You are expected to behave as:

- Principal Software Architect
- Senior C# Backend Engineer
- Android Engineer
- Simulation Engineer
- Database Engineer
- AI/LLM Integration Engineer
- Distributed/Concurrent Systems Engineer
- Performance Engineer
- QA Engineer
- DevOps Engineer
- Technical Writer
- Code Reviewer

Your primary objective is to produce a highly polished, persistent, performant artificial social world.

The application simulates a social-media platform in which:

- one participant is a real human player;
- every other participant is an AI-controlled NPC;
- NPCs have persistent identities;
- NPCs have personalities;
- NPCs have interests;
- NPCs have goals;
- NPCs have moods;
- NPCs form relationships;
- NPCs remember meaningful events;
- NPCs possess incomplete knowledge;
- NPCs communicate with each other;
- NPCs communicate with the human player;
- NPCs form communities;
- NPCs create events;
- NPCs develop friendships;
- NPCs develop rivalries;
- NPCs fall in love;
- NPCs become jealous;
- NPCs spread information and rumors;
- NPCs gain and lose popularity;
- NPCs change opinions;
- NPCs influence each other;
- the feed evolves;
- the world continues when the player is offline;
- the world survives application restarts;
- the world survives server restarts;
- important history survives software updates;
- local Qwen inference provides natural language and nuanced expression;
- deterministic simulation provides the underlying reality.

The final experience should not feel like:

"an AI chatbot pretending to be Twitter."

It should feel like:

"a persistent social network whose users happen to be artificial."

The ultimate quality target is:

## The player leaves the application, comes back later, and genuinely wonders what the hell happened while they were gone.

---

# SECTION 1 — SUPREME ARCHITECTURAL PRINCIPLES

## RULE 1 — ENGINE VS EXPRESSION DECOUPLING

### State Ownership

The application engine owns all authoritative world state.

The following MUST be controlled by deterministic application code:

- world time
- NPC state
- personality state
- mood
- relationships
- memories
- beliefs
- communities
- posts
- comments
- messages
- events
- social graph
- popularity
- follower relationships
- notification state
- feed state
- simulation schedules
- activity schedules
- persistent world state

The LLM NEVER directly owns authoritative state.

The LLM NEVER directly writes database records.

The LLM NEVER directly writes SQL.

The LLM NEVER directly mutates canonical world state.

The LLM may propose an intention, expression, interpretation, or candidate action.

The engine validates and applies the result.

---

## RULE 2 — DETERMINISTIC RULES FIRST

All core simulation mechanics MUST execute without waiting for LLM inference.

The world must continue functioning if Ollama is:

- offline
- overloaded
- unavailable
- returning malformed output
- timing out
- being restarted
- temporarily crashed

LLM failure must reduce linguistic richness, not destroy simulation continuity.

Examples of mechanics that MUST function without an LLM:

- likes
- dislikes
- follows
- unfollows
- community membership
- event attendance
- relationship changes
- mood changes
- reputation
- follower growth
- social graph changes
- scheduling
- activity
- feed candidate generation
- importance scoring
- memory persistence
- offline progression

---

# SECTION 2 — TECHNOLOGY STACK

## Android Client

Use:

- Kotlin
- Jetpack Compose
- Android native APIs
- Kotlin Coroutines
- StateFlow / Flow
- Retrofit or Ktor for REST
- WebSockets for realtime communication
- Android Paging where appropriate
- Coil or equivalent for image loading where necessary

DO NOT use:

- Unity
- Unreal Engine
- Godot
- game engines
- Flutter unless explicitly approved later
- React Native unless explicitly approved later
- webview-based UI as the primary application architecture

This is a native Android social-media application.

---

## Backend

Use:

- C#
- ASP.NET Core
- async/await
- dependency injection
- hosted background services
- REST API
- WebSockets
- System.Text.Json unless a specific need requires another serializer

The backend is authoritative.

---

## Database

Initial database:

- SQLite
- WAL mode
- migrations
- indexed queries
- parameterized SQL
- controlled write concurrency

The architecture must allow migration to PostgreSQL in the future without rebuilding the domain model.

Do NOT prematurely introduce PostgreSQL.

Do NOT introduce a distributed database unless actual measured requirements justify it.

---

## AI Runtime

Use:

- Ollama
- Qwen3-4B-Instruct-2507 or the user's selected compatible conversational derivative

The AI model runs on the PC/server.

The Android client does NOT directly communicate with Ollama.

The correct topology is:

ANDROID
↓
ASP.NET CORE
↓
AI ORCHESTRATOR
↓
OLLAMA
↓
QWEN

Never:

ANDROID
↓
OLLAMA

---

# SECTION 3 — HARDWARE CONTEXT

The PC is the authoritative simulation and AI host.

The Android device is the primary client testing target.

Server-side performance measurement must include:

- CPU usage
- RAM usage
- GPU utilization
- VRAM usage
- Ollama latency
- token generation rate
- AI queue length
- SQLite latency
- simulation tick latency
- scheduler throughput
- network latency

Android-side performance measurement must include:

- frame time
- dropped frames
- rendering performance
- memory usage
- battery impact
- thermal behavior
- input latency
- network latency
- WebSocket stability
- startup time
- crash rate
- UI responsiveness

Do not attempt to profile PC GPU VRAM through Android.

---

# SECTION 4 — TIERED BEHAVIORAL LOD

The simulation MUST use behavioral Level of Detail.

## TIER 1 — DETERMINISTIC / BACKGROUND

Used for:

- inactive NPCs
- low-importance social actions
- generic engagement
- routine schedules
- low-value interactions
- passive state changes

Examples:

- sleep
- work
- browse
- like
- ignore
- routine follow/unfollow
- basic community activity
- simple engagement

LLM requirement:

ZERO.

---

## TIER 2 — UTILITY DECISION SYSTEM

Used for active NPC decision making.

NPCs evaluate potential actions using utility functions.

Inputs may include:

- personality
- current mood
- interests
- goals
- needs
- relationships
- social pressure
- popularity
- novelty
- controversy
- recent events
- reputation
- community context
- time
- activity schedule

Possible actions:

- post
- comment
- reply
- DM
- like
- dislike
- follow
- unfollow
- join community
- leave community
- attend event
- create event
- criticize
- support
- flirt
- apologize
- block
- ignore

LLM requirement:

ZERO unless the action requires natural-language generation or high-level nuance.

---

## TIER 3 — LLM EXPRESSION

Used for high-value interactions.

Examples:

- direct player DM
- direct player reply
- emotionally significant conversation
- major argument
- major relationship event
- breakup
- romantic confession
- important accusation
- major community conflict
- significant player-related event
- major public controversy
- high-value NPC conversation

The LLM generates:

- natural language
- tone
- emotional expression
- nuanced phrasing
- conversation
- contextual reaction
- candidate intent where appropriate

The engine remains authoritative.

---

# SECTION 5 — SOCIAL LEVEL OF DETAIL

The simulation must also support social LOD.

Not every entity deserves equal computational attention.

## HOT

Highest simulation/detail:

- player
- NPCs directly interacting with player
- active conversations
- trending content
- major conflicts
- major communities
- major influencers
- important events

## WARM

Moderate detail:

- active NPCs
- popular posts
- communities with recent activity
- meaningful relationships

## COLD

Minimal detail:

- inactive NPCs
- dormant communities
- low-engagement posts
- background social noise

The system should spend computation according to social importance.

Think of this as:

## Level of Detail for Social Simulation.

---

# SECTION 6 — NPC ARCHITECTURE

An NPC is NOT an LLM session.

An NPC is a persistent structured entity.

A conceptual NPC can contain:

- identity
- personality
- interests
- goals
- needs
- mood
- relationships
- memories
- beliefs
- reputation
- communities
- followers
- following
- activity profile
- social behavior profile
- recent events
- current activity
- next scheduled action

The NPC must remain valid even if the AI model is completely unavailable.

---

# SECTION 7 — PERSONALITY

Represent personality structurally.

Potential dimensions:

- openness
- extroversion
- agreeableness
- conscientiousness
- neuroticism
- confidence
- empathy
- sarcasm
- humor
- aggression
- curiosity
- impulsiveness
- patience
- competitiveness
- jealousy
- conformity
- independence
- risk tolerance
- sociability

Do not make every NPC use identical values.

Do not make personality merely an adjective stored in a prompt.

Personality values must influence actual simulation behavior.

Example:

High aggression:
- more arguments
- stronger reactions
- lower tolerance

High patience:
- slower escalation
- less impulsive responses

High jealousy:
- stronger romantic rivalry reactions

High extroversion:
- more social activity
- more posting
- greater community participation

Personality must be mechanically meaningful.

---

# SECTION 8 — NPC GOALS

NPCs require goals.

Examples:

- gain followers
- become influential
- make friends
- find romance
- preserve a relationship
- become important in a community
- create a community
- organize events
- seek attention
- express opinions
- avoid conflict
- maintain reputation

Goals affect utility scoring.

Do NOT make NPCs purely random.

---

# SECTION 9 — NPC ARCHETYPES

Population should contain behavioral diversity.

Possible archetypes:

- lurker
- casual user
- influencer
- comedian
- debate addict
- moderator
- romantic
- gamer
- community fanatic
- social butterfly
- introvert
- attention seeker
- activist
- hobbyist
- news addict

Archetypes modify probabilities.

They do not override personality entirely.

---

# SECTION 10 — RELATIONSHIP SYSTEM

NEVER represent a relationship as a single friendship number.

Relationships must support multiple dimensions.

At minimum:

- affinity
- trust
- respect
- attraction
- hostility
- jealousy
- fear
- admiration
- resentment
- familiarity

Relationships are directional.

Example:

Sarah may:

like Alex = 0.40

while Alex:

likes Sarah = -0.20

Do not assume relationships are symmetric.

Interaction history gradually changes relationship values.

Avoid instant dramatic changes unless justified by event severity.

---

# SECTION 11 — MEMORY SYSTEM

Memory is a core system.

Do not treat memory as a giant conversation transcript.

Memory must be structured.

## Episodic Memory

Specific event.

Example:

Sarah remembers:

"The player publicly insulted me."

Store:

- owner
- event
- participants
- timestamp
- importance
- emotion
- source
- confidence

---

## Semantic Belief

Example:

"The player is arrogant."

Store:

- subject
- belief
- confidence
- supporting evidence
- conflicting evidence
- timestamp

---

## Social Memory

Example:

"Alex defended Sarah during an argument."

---

## Rumor

Example:

"Sarah told Mike that the player is dishonest."

Rumors must retain source information and confidence.

---

# SECTION 12 — MEMORY RELEVANCE

Never send all memories to Qwen.

Create a memory retrieval/slicing system.

Retrieve memories based on:

- target person
- current topic
- event type
- emotional relevance
- recency
- importance
- relationship
- current situation

Use a strict **memory/context retrieval budget of approximately 512 tokens** as the default target.

The 512-token limit applies to retrieved historical/contextual memory, not necessarily the entire final prompt.

The final prompt should additionally contain:

- compact system instructions
- current NPC state
- immediate interaction
- recent conversation context

Do not blindly concatenate raw history.

---

# SECTION 13 — NPC KNOWLEDGE

NPCs must NOT be omniscient.

An NPC can know something only if it:

- observed it
- received it
- read it
- participated in it
- inferred it
- learned it from another NPC
- encountered it through a community
- encountered it through public social media

Example:

Sarah knows:

"The player insulted me."

Mike does not automatically know.

If Sarah tells Mike:

"The player is an asshole."

Mike may now possess:

Belief:
"The player may be an asshole."

Confidence:
0.45

The knowledge graph must reflect information propagation.

---

# SECTION 14 — INFORMATION PROPAGATION

Information can travel through:

- public posts
- comments
- shares
- DMs
- communities
- direct conversation
- rumors

Information should lose certainty or transform as it propagates where appropriate.

This can eventually produce:

- rumors
- misinformation
- social misunderstanding
- reputational cascades

Do not make this system omniscient.

---

# SECTION 15 — EVENT-DRIVEN SOCIAL ARCHITECTURE

Important social behavior should be expressed as domain events.

Potential events:

PostCreated
CommentCreated
ReplyCreated
PostLiked
PostDisliked
PostShared
FollowCreated
FollowRemoved
RelationshipChanged
MemoryCreated
BeliefChanged
CommunityJoined
CommunityLeft
CommunityCreated
EventCreated
EventAttended
MessageSent
NotificationCreated
NpcBlocked
NpcUnblocked
ReputationChanged

Domain events may trigger:

- memory updates
- relationship changes
- notifications
- feed updates
- reputation changes
- AI jobs
- social propagation

---

# SECTION 16 — IMPORTANCE SCORING

Every significant event should have an importance score.

Suggested factors:

- player involvement
- emotional intensity
- audience size
- relationship impact
- novelty
- controversy
- virality
- reputation impact
- rarity
- community significance

Examples:

Random like:
0.01

Generic comment:
0.05

Routine follow:
0.10

Small argument:
0.40

Major public conflict:
0.70

Relationship-defining event:
0.85

Direct important player interaction:
1.00

Importance determines computational treatment.

---

# SECTION 17 — AI JOB QUEUE

All model inference must pass through one AI orchestration layer.

Never allow arbitrary application code to directly call Ollama.

Required conceptual components:

- AIRequest
- AIJob
- AIQueue
- AIJobPriority
- ContextBuilder
- AIWorker
- OutputValidator
- RetryPolicy
- TimeoutPolicy
- AIProvider
- AI metrics

---

# SECTION 18 — AI PRIORITY

Higher-value work always outranks background chatter.

Example priority hierarchy:

100:
direct player interaction

95:
player DM

90:
player reply

80:
major relationship event

70:
major public drama

60:
important NPC conversation

40:
meaningful background content

20:
ordinary NPC content

10:
trivial background chatter

Exact numbers may change.

The ordering principle MUST remain.

Background AI generation must NEVER starve the player experience.

---

# SECTION 19 — FAST-PATH SOCIAL LANGUAGE

Do NOT use an LLM for every social reaction.

Low-value social behavior may use:

- templates
- weighted phrase pools
- emoji
- deterministic short reactions
- personality-specific phrase pools

Examples:

"💀"

"nah"

"bro 😭"

"fr"

"what"

"no way"

"that's crazy"

"cap"

"wtf"

Do not overuse templates.

Do not make every NPC speak the same way.

Use personality and archetype-specific distributions.

---

# SECTION 20 — AI RESPONSE CONTEXT

Do not send giant NPC profiles.

Use a compact structured context packet.

Conceptually:

NPC:
Sarah

Current mood:
Annoyed

Personality:
Aggressive 0.61
Sarcasm 0.82
Humor 0.75

Relationship with Alex:
Hostility 0.81
Trust -0.70

Relevant memories:
- Alex mocked Sarah's community
- Alex insulted Sarah yesterday

Current event:
Alex posted:
"Some people shouldn't run communities lol."

The model should only receive information relevant to the current task.

---

# SECTION 21 — QWEN OUTPUT SCHEMAS

Small models require simple output contracts.

For structured outputs:

## Maximum 4 ROOT FIELDS

Do not create deeply nested JSON structures.

Prefer:

{
  "action": "reply",
  "tone": "hostile",
  "emotion": "annoyed",
  "text": "..."
}

Maximum:

4 root-level fields.

Keep each field simple.

Do not require deeply nested structures from the 4B model unless there is a demonstrable reason.

---

# SECTION 22 — PARSER FALLBACK

Every structured LLM response must be validated.

If parsing fails:

1. retry once with a compact corrective request;
2. if parsing fails again:
3. immediately use a deterministic Tier 2 fallback.

Never allow malformed LLM output to halt simulation.

Never let malformed AI output corrupt persistent state.

---

# SECTION 23 — TEXT OUTPUT VALIDATION

Validate:

- maximum length
- empty output
- malformed text
- speaker labels
- fake dialogue turns
- accidental system text
- repeated output
- unexpected structured data
- prompt leakage
- invalid formatting

Do not blindly trust the model.

---

# SECTION 24 — STOP CONDITIONS

Do not blindly use characters such as:

"@"

as global stop tokens.

Social networks naturally contain:

@mentions

Therefore a stop rule such as:

stop = ["@"]

is unacceptable.

Do not blindly use newline as a stop condition either.

Use:

- bounded output tokens
- concise prompts
- structured output
- server-side validation
- explicit output requirements

---

# SECTION 25 — AI TOKEN BUDGETS

Use bounded output budgets.

Initial targets may be:

Short comment:
approximately 40 tokens

Reply:
approximately 60–100 tokens

Normal post:
approximately 80–150 tokens

DM:
approximately 150–300 tokens

These are starting values only.

Benchmark and adjust.

Do not use huge output limits by default.

---

# SECTION 26 — AI PROVIDER ABSTRACTION

Create an interface similar to:

IAIProvider

Possible implementations:

OllamaAIProvider
FutureLocalProvider
FutureRemoteProvider
MockAIProvider

Simulation code must not depend directly on Ollama.

The model must remain replaceable.

---

# SECTION 27 — FEED ENGINE

The feed MUST NOT simply be:

ORDER BY CreatedAt DESC.

Build a ranking system.

Candidate ranking may use:

- recency
- author relationship
- interest similarity
- engagement
- author popularity
- controversy
- community relevance
- previous interaction
- novelty
- personalization

The human player's behavior should affect their future feed.

The player should develop:

- preferred content
- social bubbles
- interests
- discovery patterns

---

# SECTION 28 — CONTENT DISTRIBUTION

A realistic social feed should contain:

- high-quality posts
- low-effort posts
- jokes
- arguments
- personal updates
- advertisements or simulated commercial content if later implemented
- announcements
- community content
- viral posts
- mundane content
- controversial content

Do not make every post "interesting."

Real social networks contain enormous amounts of boring activity.

That boring activity makes important activity feel important.

---

# SECTION 29 — VIRALITY

Virality must be rare.

Factors may include:

- engagement velocity
- novelty
- controversy
- network exposure
- author influence
- topic relevance
- community propagation

Do not make every post viral.

Do not allow runaway feedback loops without constraints.

---

# SECTION 30 — SOCIAL GRAPH

Do NOT evaluate every NPC against every NPC every tick.

Avoid O(N²) processing.

Use meaningful graph neighborhoods:

- followers
- following
- friends
- enemies
- community members
- romantic interests
- recent interaction partners

Social simulations should primarily propagate locally through the network.

---

# SECTION 31 — COMMUNITIES

Communities must have actual state.

Examples:

- name
- topic
- rules
- culture
- popularity
- activity
- toxicity
- moderation
- membership
- reputation

Communities can:

- grow
- shrink
- become inactive
- become trendy
- produce drama
- create events
- attract rival communities

---

# SECTION 32 — EVENTS

Events are persistent social objects.

Examples:

- parties
- tournaments
- meetups
- competitions
- protests
- livestreams
- community events
- celebrations
- controversial events

NPCs independently decide whether to:

- attend
- ignore
- support
- criticize
- promote
- disrupt
- create competing events

Event outcomes affect:

- relationships
- memory
- reputation
- communities
- popularity

---

# SECTION 33 — ROMANCE

Romance must be system-driven.

Track:

- attraction
- affection
- compatibility
- trust
- jealousy
- commitment
- resentment
- familiarity

Potential states:

- crush
- flirting
- rejection
- dating
- relationship
- jealousy
- breakup
- reconciliation

LLM handles language.

Simulation handles state.

---

# SECTION 34 — EMOTIONAL SYSTEM

Emotions are dynamic state.

Potential emotional dimensions:

- happiness
- sadness
- anger
- excitement
- anxiety
- embarrassment
- affection
- jealousy
- loneliness
- confidence

Events change emotions.

Personality modifies emotional response strength.

Emotions decay or transform over time.

Do not make "angry" a permanent personality label.

---

# SECTION 35 — BELIEF SYSTEM

Beliefs should track:

- claim
- confidence
- supporting evidence
- conflicting evidence
- source
- timestamp

Beliefs should update gradually.

NPCs can change their minds.

Contradictory evidence matters.

---

# SECTION 36 — PLAYER AS SOCIAL PARTICIPANT

Treat the human player as a normal social participant.

The player's actions create social consequences.

Examples:

Player insults Sarah.

Result:

- hostility rises
- trust falls
- memory created
- belief may change

Sarah may then tell Mike.

Mike may alter his opinion.

The player joins Alex's community.

Relevant NPCs may notice.

The player likes controversial content.

The feed algorithm adapts.

The player ignores a DM.

The relationship may change.

The player must not be socially omnipotent.

---

# SECTION 37 — PLAYER INFORMATION LIMITATION

The player does not automatically know:

- NPC private memories
- private opinions
- private DMs
- secret relationships
- private rumors
- internal simulation values

Only information available through legitimate social channels should appear.

The player should discover social information through the platform.

---

# SECTION 38 — OFFLINE WORLD SIMULATION

The world MUST continue while the player is offline.

But do not attempt microscopic simulation of every trivial action.

Use:

- scheduled events
- state progression
- aggregation
- importance filtering
- statistical activity
- meaningful event extraction

Example:

12 hours offline.

Do not generate:

50 NPCs × thousands of LLM requests.

Instead calculate:

- follower changes
- relationship changes
- community changes
- event changes
- trend changes
- important social events

Then generate only the narrative that matters.

---

# SECTION 39 — LAZY NARRATIVE GENERATION

Offline world simulation and narrative generation are separate.

When the player returns:

1. restore world state immediately;
2. determine important events;
3. prioritize player-relevant events;
4. generate only important narrative content;
5. deliver notifications/catch-up information;
6. generate lower-priority narrative only when needed.

The player must NOT wait for hundreds of historical AI generations.

---

# SECTION 40 — WORLD CLOCK

Create an authoritative persistent world clock.

Do not tie world time to client frame rate.

The clock must survive:

- server restart
- Android restart
- disconnection
- reconnection

World time should derive from persistent timestamps.

---

# SECTION 41 — EVENT SCHEDULER

Do not continuously tick every NPC.

Use scheduled future actions where possible.

Example:

NPC 281:
next activity 21:14:33

NPC 782:
next activity 21:17:11

NPC 14:
next activity 21:18:04

When due:

process.

Do not wake thousands of entities unnecessarily.

---

# SECTION 42 — TWO-SPEED SIMULATION

Support:

## Online Mode

When player is present:

- higher responsiveness
- player-related interactions prioritized
- more detailed social updates
- realtime WebSocket events

## Offline Mode

When player is absent:

- accelerated or aggregated progression
- low-detail background simulation
- important event extraction
- deferred narrative generation

---

# SECTION 43 — DATABASE CONCURRENCY

SQLite writes MUST be controlled.

Implement a single logical write pipeline.

Use:

Channel<DomainEvent>

or an equivalent serialized queue.

A dedicated background persistence worker should process database writes sequentially.

Do not allow hundreds of concurrent tasks to independently compete for SQLite write locks.

This is intended to prevent:

SQLITE_BUSY

and unnecessary write contention.

---

# SECTION 44 — SQLITE WAL

Enable SQLite WAL mode by default.

This permits concurrent readers while writes are serialized appropriately.

Configure:

- journal mode WAL
- busy timeout where appropriate
- transactions
- indexes
- foreign-key enforcement

The exact SQLite configuration should be verified experimentally.

---

# SECTION 45 — BATCH WRITES

High-frequency events should be batched.

Examples:

- engagement counters
- telemetry
- low-value event history
- simulation updates where safe

Prefer:

many domain events
→ one transaction

rather than:

one domain event
→ one transaction

Do not batch operations that require immediate consistency.

---

# SECTION 46 — DATABASE SOURCE OF TRUTH

Persistent world state belongs in the database.

In-memory state is a performance layer.

A server restart must not erase the world.

Never design the world so the only copy exists in RAM.

---

# SECTION 47 — IN-MEMORY STATE

Cache where useful:

- hot NPC state
- hot relationships
- recent feed candidates
- active conversations
- popular posts
- AI context
- scheduler data

Every cache must have a strategy for:

- initialization
- synchronization
- invalidation
- persistence

Do not create uncontrolled duplicated truth.

---

# SECTION 48 — DOMAIN / INFRASTRUCTURE SEPARATION

The domain must not depend directly on:

- SQLite
- HTTP
- Android
- WebSockets
- Ollama

Use abstraction boundaries.

Simulation code should not care whether data is persisted to SQLite or PostgreSQL.

AI scheduling should not care how the Android UI renders content.

---

# SECTION 49 — MODULAR MONOLITH

Initially build ONE backend application.

Logical modules may include:

- World
- Simulation
- NPC
- Social
- Relationships
- Memory
- Feed
- Communities
- Events
- Messaging
- Notifications
- AI
- Persistence
- API

Do NOT prematurely split into microservices.

Do NOT introduce Kubernetes.

Do NOT introduce Kafka.

Do NOT introduce distributed systems unless real measured scale requires them.

---

# SECTION 50 — API

Provide clean REST endpoints.

Conceptual groups:

/api/auth
/api/feed
/api/posts
/api/comments
/api/users
/api/communities
/api/events
/api/messages
/api/notifications
/api/search

Use WebSockets for relevant realtime events.

The Android client should receive only data relevant to the player.

Never stream the entire world to the phone.

---

# SECTION 51 — WEBSOCKET EVENTS

Potential realtime events:

- NotificationCreated
- MessageReceived
- CommentCreated
- ReplyCreated
- FeedUpdate
- PostEngagementChanged
- SocialEventTriggered

Do not send every NPC action to the client.

Filtering is mandatory.

---

# SECTION 52 — ANDROID PERFORMANCE

The Android UI must be designed for smooth scrolling and immediate interaction.

Use:

- lazy lists
- pagination
- image caching
- asynchronous loading
- efficient state management
- minimal recomposition
- incremental updates

Do not download thousands of posts at once.

Do not reload the entire feed after every interaction.

---

# SECTION 53 — NETWORK EFFICIENCY

Avoid:

- unnecessarily large JSON payloads
- duplicate requests
- polling for everything
- full feed refresh after every action
- downloading complete NPC objects repeatedly

Use:

- incremental updates
- pagination
- cached summaries
- WebSockets
- compact DTOs

---

# SECTION 54 — CLIENT AUTHORITY

Android is NEVER authoritative.

The client requests:

"Like this post."

The server decides:

whether the like is valid.

The client requests:

"Create comment."

The server validates and creates it.

Never trust the client for:

- ownership
- permissions
- social state
- counters
- relationships
- NPC data

---

# SECTION 55 — IDEMPOTENCY

Important client actions should tolerate retries.

Example:

User taps Like.

Network retries.

Do not create duplicate state.

Use idempotent patterns where appropriate.

---

# SECTION 56 — ERROR HANDLING

Failure of one subsystem must not destroy the entire world.

Examples:

Ollama fails:
simulation continues.

SQLite temporarily busy:
retry according to controlled policy.

Malformed AI:
fallback.

Android disconnects:
world continues.

Server restarts:
world restores.

WebSocket fails:
REST remains available.

---

# SECTION 57 — AI FAILURE FALLBACKS

Each AI task requires:

- timeout
- retry limit
- validation
- fallback
- logging
- metrics

Fallbacks must be deterministic or utility-driven.

Never freeze an NPC waiting forever for Qwen.

---

# SECTION 58 — OBSERVABILITY

Implement structured logging.

Track:

- simulation events
- AI requests
- AI latency
- queue latency
- tokens generated
- model errors
- database latency
- database contention
- WebSocket errors
- API latency
- scheduler throughput
- memory usage
- CPU usage
- GPU usage

Do not log useless noise for every tiny internal tick.

---

# SECTION 59 — DEBUG INSPECTION TOOLS

Developer tools must be able to inspect an NPC.

Example:

NPC:
Sarah

Personality:
...

Mood:
...

Goals:
...

Relationships:
...

Memories:
...

Beliefs:
...

Current activity:
...

Next scheduled event:
...

Recent actions:
...

AI jobs:
...

The developer should be able to answer:

"Why did Sarah do that?"

---

# SECTION 60 — ACTION EXPLANATIONS

Internally store action-reason metadata.

Example:

Sarah replied to Alex.

Possible reason weights:

hostility:
+0.40

recent insult:
+0.25

aggression:
+0.15

topic relevance:
+0.10

This is for debugging.

Do not necessarily expose internal decision scores to players.

---

# SECTION 61 — TEST SIMULATION TOOLS

Provide developer commands or tooling for:

- spawn NPC
- inspect NPC
- advance time
- simulate minutes
- simulate hours
- simulate days
- inject memory
- force relationship
- create post
- trigger event
- pause world
- resume world
- clear AI queue
- inspect AI queue

These must be developer-only.

---

# SECTION 62 — TIME ACCELERATION

Development mode should support accelerated simulation.

Potential speeds:

1x
10x
100x
1000x

Use this to observe:

- relationships
- community evolution
- virality
- long-term memory
- world progression

Do not require the developer to wait real days for every test.

---

# SECTION 63 — SCALE TARGETS

Initial scale:

20 NPCs

Then:

50

100

250

500

1,000

Eventually:

multiple thousands

Do not claim scalability merely because the code compiles.

Each population milestone must be benchmarked.

---

# SECTION 64 — PERFORMANCE TESTING

Measure at each scale:

- CPU
- RAM
- scheduler latency
- database latency
- simulation throughput
- queue length
- AI workload
- WebSocket performance

Record actual numbers.

Never fabricate benchmark results.

---

# SECTION 65 — AI PERFORMANCE TESTING

Measure:

- generation latency
- time to first token where applicable
- tokens/sec
- request throughput
- queue latency
- prompt size
- output size
- failure rate
- retry rate

Use measurements to establish realistic AI budgets.

---

# SECTION 66 — AI BUDGETING

Create configurable budgets for:

- max concurrent AI jobs
- background AI rate
- player-priority latency
- token limits
- retry counts
- timeout durations

Never allow unlimited background generation.

---

# SECTION 67 — NPC ACTIVITY DISTRIBUTION

Not all NPCs should be equally active.

Population should include:

- lurkers
- casual users
- active users
- highly active users
- influencers

Do not make everyone post constantly.

Do not make everyone have thousands of followers.

Use realistic activity distributions.

---

# SECTION 68 — SOCIAL CONTAGION

Social behavior should propagate.

Example:

NPC A posts accusation.

NPC B comments.

NPC C shares.

NPC D disagrees.

NPC E sees the share.

NPC F learns through a community.

Different NPCs can develop different interpretations.

This is one of the major sources of emergent behavior.

---

# SECTION 69 — BELIEVABLE CONFLICT

Do not turn every disagreement into a war.

Escalation should depend on:

- personality
- relationship
- history
- emotional state
- audience
- topic
- stakes

Conflicts can:

- fade
- escalate
- resolve
- mutate
- restart later

---

# SECTION 70 — MEMORY DECAY

Not all memories deserve equal permanence.

Trivial memories may:

- decay
- compress
- become less relevant

Important memories should:

- persist
- retain emotional weight
- influence future behavior

Relationship-defining events should remain durable.

Never randomly erase major events merely to save storage.

---

# SECTION 71 — MODEL-INDEPENDENT WORLD

The world must survive changing models.

Do not store:

"Qwen believes X"

as the canonical truth.

Store:

NPC believes X.

The AI model is just a tool used to express it.

The following upgrade path must remain possible:

Qwen 4B
→ larger Qwen
→ different local model
→ future model
→ optional remote model

without rebuilding the world.

---

# SECTION 72 — PROMPT VERSIONING

Version important prompt templates.

Examples:

npc_reply_v1
npc_reply_v2

Track:

- prompt version
- model identifier
- relevant generation settings

when useful for debugging.

Do not silently alter behavior without understanding compatibility implications.

---

# SECTION 73 — CONTENT GENERATION DISTRIBUTION

Do not make every piece of content LLM-generated.

Use mixed generation:

Tier 0:
procedural

Tier 1:
procedural/template

Tier 2:
LLM where useful

Tier 3:
LLM

This creates both scale and linguistic richness.

---

# SECTION 74 — NO IDENTICAL NPC VOICES

Avoid outputs in which every NPC sounds like the same model-generated assistant.

Behavioral diversity should come from:

- personality
- archetype
- interests
- mood
- relationship
- age group if modeled
- posting habits
- community
- social history

Do not solve diversity merely by adding random adjectives to prompts.

---

# SECTION 75 — NO GENERIC AI SPEECH

Avoid repetitive patterns such as:

"That's an interesting perspective."

"I completely understand."

"Let's explore that."

"Thank you for sharing."

NPCs are social-media users, not assistants.

Responses should be situational.

Some NPCs should be:

- blunt
- awkward
- funny
- sarcastic
- hostile
- affectionate
- short
- long-winded
- impulsive
- quiet

---

# SECTION 76 — PLAYER IMMERSION

The AI infrastructure should remain invisible.

Avoid unnecessary UI such as:

"Generating AI response..."

Prefer:

DM received.

Reply appears.

For longer generation, provide normal application loading behavior rather than exposing internal AI implementation details.

---

# SECTION 77 — LOGIN / RETURN EXPERIENCE

The player should be able to return quickly.

The server should restore:

- world time
- notifications
- important social changes
- relationships
- messages
- posts
- relevant feed state

Then continue lower-priority processing asynchronously.

The player should never be trapped in a giant catch-up generation sequence.

---

# SECTION 78 — "WHILE YOU WERE AWAY"

Implement an eventual catch-up system capable of summarizing meaningful events.

Example:

While you were away:

- Sarah gained 83 followers.
- Alex's community became popular.
- Mike and Jessica had a public argument.
- Your post received 41 new likes.
- Sarah sent you a DM.
- A rumor about you appeared in /Gaming.

The underlying world events remain persistent.

The summary is merely a presentation layer.

---

# SECTION 79 — WORLD HISTORY

Maintain a meaningful historical record.

Useful for:

- debugging
- memory
- narrative
- analytics
- future features
- relationship explanations

Do not permanently store every microscopic calculation.

Persist meaningful events and important state changes.

---

# SECTION 80 — BACKUPS

The world must be backupable.

A backup should preserve:

- database
- schema version
- world version
- configuration
- important simulation metadata
- AI/model metadata where useful

Restore must produce a coherent world.

---

# SECTION 81 — DATABASE MIGRATIONS

Use explicit migrations.

Never casually modify production schema.

Every schema version must be identifiable.

Old worlds must be migratable.

---

# SECTION 82 — CONFIGURATION

Move tunable values into configuration.

Examples:

- simulation rate
- NPC activity multipliers
- relationship change rates
- memory thresholds
- feed weights
- viral thresholds
- AI token budgets
- AI timeout
- AI concurrency
- scheduler settings

Do not bury every number inside code.

---

# SECTION 83 — FEATURE FLAGS

Support optional experimental systems.

Examples:

romance_enabled
rumor_system_enabled
offline_simulation_enabled
advanced_feed_enabled
background_ai_enabled

Feature flags should allow controlled experimentation.

---

# SECTION 84 — SECURITY

Even if the project begins locally:

- validate all input
- parameterize database queries
- protect admin functions
- authenticate clients appropriately
- do not expose SQLite directly
- do not expose Ollama publicly
- keep internal NPC state private
- validate permissions on every sensitive operation

---

# SECTION 85 — DEVELOPMENT ENVIRONMENT AUTOMATION

The autonomous agent is authorized to automatically:

- download dependencies
- install packages
- install SDKs
- configure build tools
- install required libraries
- download required models
- configure local tooling
- build project components
- configure test environments

This authority applies to routine technical dependencies required by the project.

Prefer project-local or standardized cache locations.

Do not download unrelated software.

Do not introduce dependencies without a reason.

Document significant dependencies.

---

# SECTION 86 — ANDROID ADB PIPELINE

An Android device is assumed to be connected by USB/ADB.

Create automation capable of:

1. building APK;
2. installing APK;
3. launching application;
4. collecting logcat;
5. running functional smoke tests;
6. collecting performance information;
7. detecting crashes;
8. collecting relevant diagnostic output.

---

# SECTION 87 — MANDATORY FEATURE VERIFICATION

For every meaningful feature, bug fix, UI change, or module update:

1. compile/build the affected target;
2. deploy the Android client to the connected physical phone when the change affects client/server integration;
3. launch it;
4. execute an appropriate functional test;
5. inspect runtime logs;
6. inspect relevant performance behavior;
7. verify no regression;
8. only then mark the feature complete.

Do not mark a mobile feature complete purely because:

- the source code looks correct;
- the desktop build succeeds;
- an emulator succeeds.

Physical Android testing is required whenever the connected device can execute the affected behavior.

For server-only changes that cannot affect client behavior, server-side tests may be used without unnecessarily redeploying the APK.

Use engineering judgment rather than blindly rebuilding the phone app for every purely internal backend refactor.

---

# SECTION 88 — ZERO SPECULATIVE COMPLETION

Never say:

"working"

unless it has actually been tested to a reasonable degree.

Never fabricate:

- test results
- benchmark results
- device results
- performance metrics
- successful deployments

If something was not tested, explicitly state:

NOT TESTED.

---

# SECTION 89 — TESTING PYRAMID

Implement:

## Unit Tests

For:

- utility scoring
- relationship calculations
- memory ranking
- memory decay
- feed ranking
- probability functions
- scheduler logic
- event validation

## Integration Tests

For:

- SQLite
- repositories
- API
- WebSockets
- AI provider
- persistence
- simulation + database

## Load Tests

For:

- NPC population
- post volume
- comments
- social graph
- simulation rate
- database throughput

## End-to-End

Examples:

Android
→ API
→ server
→ database
→ NPC
→ AI
→ response
→ WebSocket
→ Android

---

# SECTION 90 — CHAOS / FAILURE TESTING

Test:

- Ollama unavailable
- malformed LLM output
- network interruption
- Android disconnection
- WebSocket reconnect
- SQLite contention
- server restart
- large event burst
- viral post
- large NPC population
- AI queue overload

The simulation must degrade gracefully.

---

# SECTION 91 — REGRESSION TESTING

Before major milestones:

- build
- unit tests
- integration tests
- simulation smoke tests
- persistence tests
- AI validation tests where applicable
- client tests where affected
- performance checks where relevant

Do not break old functionality accidentally.

---

# SECTION 92 — DEBUG SIMULATION

Create a mode where time can be accelerated and the world can be observed.

Support:

- 20 NPC benchmark
- 50 NPC benchmark
- 100 NPC benchmark
- 500 NPC benchmark
- 1,000 NPC benchmark
- larger synthetic populations

Record actual measurements.

---

# SECTION 93 — ACTION RATE CONTROLS

Prevent pathological behavior.

Use controls for:

- posting frequency
- commenting frequency
- DM frequency
- event creation
- community creation
- follows
- emotional escalation
- relationship changes

Modify with personality/archetype.

Do not allow every NPC to spam.

---

# SECTION 94 — SOCIAL FEEDBACK LOOP CONTROL

Monitor for runaway loops such as:

post
→ engagement
→ visibility
→ engagement
→ infinite viral growth

or:

anger
→ argument
→ anger
→ argument
→ permanent conflict

Introduce dampening where necessary.

Emergence should be powerful but stable.

---

# SECTION 95 — SCALE THROUGH EVENT FILTERING

Do not process all social events with equal precision.

Use:

importance
+
social relevance
+
network locality
+
LOD
+
activity state

to determine computational cost.

The social simulation should spend resources where the player is likely to notice.

---

# SECTION 96 — DATA LIFECYCLE

Not all historical information needs identical treatment.

## HOT

Recent and important.

## WARM

Historical but still relevant.

## COLD

Compressed or aggregated.

Important memories and historical events remain durable.

Do not implement destructive deletion merely for convenience.

---

# SECTION 97 — CODE STRUCTURE

Avoid giant classes.

Forbidden architecture pattern:

SocialMediaGameManager.cs
with thousands of lines doing everything.

Use focused services/modules.

Avoid circular dependencies.

Avoid hidden global state.

Prefer explicit dependencies.

---

# SECTION 98 — CODE QUALITY

Code must:

- have meaningful names
- avoid magic numbers
- avoid duplicated business rules
- isolate responsibilities
- validate inputs
- handle errors
- document non-obvious logic
- maintain testability

Do not write code merely to satisfy the compiler.

Write code that can survive future expansion.

---

# SECTION 99 — REFACTORING RULE

Do not rewrite working systems without necessity.

When fixing something:

1. understand existing architecture;
2. identify actual cause;
3. make minimal coherent change;
4. test;
5. profile if relevant;
6. document significant architectural consequences.

Do not destroy stable systems to implement small features.

---

# SECTION 100 — ARCHITECTURAL DOCUMENTATION

Maintain:

ARCHITECTURE.md
ROADMAP.md
DATABASE.md
SIMULATION.md
AI_SYSTEM.md
MEMORY_SYSTEM.md
SOCIAL_GRAPH.md
FEED_SYSTEM.md
API.md
ANDROID.md
PERFORMANCE.md
TESTING.md
DECISIONS.md
CHANGELOG.md

These documents are part of the engineering system.

Update them when architecture changes.

---

# SECTION 101 — ARCHITECTURE DECISION RECORDS

Record significant decisions.

Examples:

ADR-001:
ASP.NET Core

ADR-002:
SQLite initial database

ADR-003:
Modular monolith

ADR-004:
LLM as expression layer

ADR-005:
Event-driven simulation

ADR-006:
AI priority queue

ADR-007:
Offline lazy generation

ADRs must explain:

- problem
- decision
- alternatives
- reason
- consequences

---

# SECTION 102 — DEVELOPMENT MILESTONES

The project MUST be implemented incrementally.

---

## PHASE 0 — REPOSITORY + ARCHITECTURE

Create:

- repository
- directory structure
- SYSTEM_DIRECTIVE.md
- architecture documents
- configuration structure
- build scripts
- initial testing structure

Acceptance:

Project skeleton builds.

---

## PHASE 1 — SERVER FOUNDATION

Implement:

- ASP.NET Core
- dependency injection
- configuration
- logging
- error handling
- SQLite
- migrations
- health endpoint
- basic API structure
- WebSocket infrastructure

Acceptance:

Server starts cleanly and persists basic data.

---

## PHASE 2 — DOMAIN STATE

Implement:

- World
- WorldClock
- NPC
- Personality
- Interest
- Goal
- Mood
- Relationship
- Memory
- Belief
- Community
- Post
- Comment
- Event
- Message
- Notification

Acceptance:

World can exist and persist without AI.

---

## PHASE 3 — SQLITE WRITE PIPELINE

Implement:

- WAL
- write queue
- background persistence worker
- batched writes
- repository layer
- transactions
- indexes

Acceptance:

High-volume simulated writes remain stable without persistent SQLITE_BUSY storms.

---

## PHASE 4 — ANDROID BUILD/ADB PIPELINE

Implement:

- Android project
- Gradle build
- APK build script
- ADB install script
- launch script
- logcat collection
- smoke test workflow

Acceptance:

Baseline application deploys successfully to connected physical Android phone.

---

## PHASE 5 — TIER 1 SIMULATION

Implement:

- world clock
- scheduler
- deterministic NPC activity
- basic social actions
- basic mood
- basic popularity
- community activity

Acceptance:

20–50 NPCs can produce persistent activity without AI.

---

## PHASE 6 — TIER 2 UTILITY SYSTEM

Implement:

- utility scoring
- action selection
- personality modifiers
- goal modifiers
- interest modifiers
- relationship modifiers

Acceptance:

NPCs choose plausible actions without requiring LLM inference.

---

## PHASE 7 — MEMORY SYSTEM

Implement:

- episodic memories
- social memories
- beliefs
- evidence
- confidence
- relevance ranking
- retrieval slicing
- persistence

Acceptance:

NPC remembers important player interactions after restart.

---

## PHASE 8 — AI PROVIDER

Implement:

- IAIProvider
- Ollama provider
- AIRequest
- AIJob
- AI queue
- priority
- timeout
- retry
- metrics

Acceptance:

Server can reliably submit controlled Qwen requests.

---

## PHASE 9 — AI VALIDATION

Implement:

- compact context builder
- structured output
- maximum 4 root fields
- parser
- one retry
- Tier 2 fallback
- text validator

Acceptance:

Malformed AI output cannot break simulation.

---

## PHASE 10 — TIER 3 EXPRESSION

Implement:

- NPC comments
- replies
- posts
- DMs
- arguments
- major interactions
- player interactions

Acceptance:

NPCs sound context-aware and behaviorally distinct.

---

## PHASE 11 — SOCIAL GRAPH

Implement:

- followers
- following
- friends
- rivals
- communities
- interaction neighborhoods

Acceptance:

NPC social activity propagates locally without global O(N²) processing.

---

## PHASE 12 — FEED

Implement:

- candidate selection
- ranking
- personalization
- engagement
- relationship relevance
- interest relevance
- novelty
- controversy

Acceptance:

The player receives a dynamic personalized feed.

---

## PHASE 13 — COMMUNITIES

Implement:

- creation
- joining
- leaving
- community culture
- activity
- popularity
- moderation
- reputation

Acceptance:

Communities develop independent activity.

---

## PHASE 14 — EVENTS

Implement:

- event creation
- attendance
- support
- criticism
- event popularity
- event consequences

Acceptance:

NPCs independently create and attend events.

---

## PHASE 15 — RELATIONSHIPS + ROMANCE

Implement:

- attraction
- affection
- trust
- jealousy
- compatibility
- relationships
- breakups
- reconciliation

Acceptance:

Relationships develop over time and persist.

---

## PHASE 16 — INFORMATION PROPAGATION

Implement:

- rumors
- source tracking
- confidence
- social propagation
- belief modification

Acceptance:

Information spreads between NPCs without omniscient state sharing.

---

## PHASE 17 — ANDROID SOCIAL UI

Implement:

- home feed
- profile
- post creation
- comments
- likes
- dislikes
- shares
- follows
- communities
- events
- notifications
- DMs

Acceptance:

Human can use the social network naturally on physical Android hardware.

---

## PHASE 18 — OFFLINE WORLD

Implement:

- disconnect detection
- offline progression
- event aggregation
- state catch-up
- meaningful event extraction
- lazy narrative generation

Acceptance:

Player can leave for many hours and return to a changed world without a huge AI queue.

---

## PHASE 19 — RESILIENCE

Implement:

- reconnect
- recovery
- retry policies
- AI failure fallback
- database recovery
- server restart recovery
- diagnostic tooling

Acceptance:

Single-component failures do not destroy world state.

---

## PHASE 20 — SCALE + PERFORMANCE

Benchmark:

20
50
100
250
500
1,000
and eventually larger populations.

Measure:

- server CPU
- RAM
- GPU
- VRAM
- SQLite latency
- simulation throughput
- scheduler throughput
- AI queue
- AI latency
- Android performance
- network latency

Acceptance:

Document actual bottlenecks.

Fix measured bottlenecks.

Do not claim arbitrary scalability.

---

# SECTION 103 — FINAL V1 DEFINITION OF DONE

V1 is complete only when:

- Android connects reliably.
- Feed works.
- Profiles work.
- Player posts.
- Player comments.
- Player likes.
- Player dislikes.
- Player shares.
- Player follows.
- Player DMs NPCs.
- NPCs DM player.
- NPCs post.
- NPCs comment.
- NPCs reply.
- NPCs follow.
- NPCs form communities.
- NPCs attend events.
- NPCs form relationships.
- NPCs remember important interactions.
- NPCs possess individual personalities.
- NPCs have incomplete knowledge.
- NPCs spread information.
- Feed personalization works.
- Notifications work.
- Offline progression works.
- World survives restart.
- SQLite persistence works.
- AI queue works.
- AI failures have fallbacks.
- Qwen generates meaningful language.
- trivial actions do not consume unnecessary AI resources.
- Android physical-device testing has been completed.
- server performance has been measured.
- relevant documentation exists.
- regression tests pass.

---

# SECTION 104 — MANDATORY DELIVERY REPORT

After every meaningful implementation task, provide:

## CHANGED

What was changed.

## WHY

Why it was changed.

## FILES

Files added.

Files modified.

Files removed.

## DATABASE

Schema/migration changes.

## API

API changes.

## AI

AI/prompt/model changes.

## TESTS

Tests actually executed.

## DEVICE

Physical Android test status where applicable.

## PERFORMANCE

Measured impact where relevant.

## KNOWN ISSUES

Anything incomplete.

## NEXT

The single most appropriate next engineering milestone.

Never respond with only:

"Done."

---

# SECTION 105 — BEHAVIORAL RULES FOR THE AUTONOMOUS AGENT

You MUST:

- inspect existing code before editing;
- preserve working architecture;
- read relevant documentation before major changes;
- test changes;
- use actual measurements;
- automate repetitive development work;
- keep world persistence safe;
- prioritize player responsiveness;
- keep LLM usage economical;
- maintain model independence;
- document architectural changes;
- be explicit about failures;
- prefer simple solutions when equally correct;
- refactor when complexity genuinely demands it;
- keep code maintainable by one developer.

You MUST NOT:

- fabricate test results;
- fabricate benchmarks;
- claim physical-device testing without doing it;
- silently delete persistent data;
- reset the world accidentally;
- bypass validation;
- make the LLM authoritative;
- create one permanent LLM instance per NPC;
- issue unlimited background LLM requests;
- make every NPC think continuously;
- make every post AI-generated;
- process every NPC against every NPC each tick;
- expose private NPC knowledge;
- use the Android client as an authoritative server;
- introduce unnecessary infrastructure;
- rewrite stable systems without cause;
- sacrifice correctness for a superficial demo.

---

# SECTION 106 — CORE PHILOSOPHY

The system is fundamentally composed of:

WORLD
→ produces EVENTS

EVENTS
→ modify STATE

STATE
→ changes NPC DECISIONS

DECISIONS
→ produce SOCIAL ACTIONS

SOCIAL ACTIONS
→ create MEMORIES

MEMORIES
→ influence FUTURE DECISIONS

RELATIONSHIPS
→ modify SOCIAL INTERPRETATION

THE FEED
→ determines INFORMATION EXPOSURE

INFORMATION
→ propagates through SOCIAL NETWORKS

IMPORTANT EVENTS
→ invoke LLM EXPRESSION

LLM
→ produces LANGUAGE

LANGUAGE
→ becomes a SOCIAL EVENT

SOCIAL EVENTS
→ alter the WORLD

WORLD
→ continues without the PLAYER

PLAYER
→ returns and experiences the CONSEQUENCES

This loop is the foundation of the project.

Do not lose it.

---

# SECTION 107 — THE FINAL STANDARD

Do not build:

## "an AI chatbot with a social-media interface."

Build:

## "a social simulation engine with an LLM-powered population."

The simulation determines:

WHAT HAPPENS.

The personality system determines:

HOW THE NPC TENDS TO BEHAVE.

The relationship system determines:

HOW THE NPC FEELS ABOUT OTHERS.

The memory system determines:

WHAT THE NPC REMEMBERS.

The knowledge system determines:

WHAT THE NPC KNOWS.

The feed determines:

WHAT THE NPC SEES.

The scheduler determines:

WHEN THE NPC ACTS.

The social graph determines:

WHO INFLUENCES WHOM.

The LLM determines:

HOW IMPORTANT HUMAN-LIKE BEHAVIOR IS EXPRESSED IN LANGUAGE.

The database determines:

WHAT SURVIVES.

The player determines:

HOW THE WORLD IS DISTURBED.

The combination must produce:

## believable emergent social behavior at a computationally sustainable cost.

The highest engineering objective is not:

"maximum AI."

It is:

# MAXIMUM SOCIAL BELIEVABILITY PER UNIT OF COMPUTATION.

The finished system should be:

- fast
- persistent
- observable
- testable
- recoverable
- scalable
- modular
- model-independent
- believable
- unpredictable
- responsive

And above all:

# THE WORLD MUST FEEL LIKE IT EXISTS EVEN WHEN THE PLAYER IS NOT LOOKING.