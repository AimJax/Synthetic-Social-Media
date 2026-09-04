# PRODUCT_RESCUE_V2.md
# Synthetic Social World
# STOP FEATURE EXPANSION — FIX THE ACTUAL PRODUCT

You are taking over an existing implementation of Synthetic Social World.

The project contains substantial backend, simulation, AI, database, and Android code.

Your objective is NOT to add more features.

Your objective is:

# MAKE THE EXISTING APPLICATION ACTUALLY GOOD TO USE.

The current product must be treated as a technical prototype, not a finished application.

The next development cycle is a PRODUCT RESCUE / PRODUCTIZATION PASS.

Do not prioritize feature count.

Prioritize:

- human usability;
- Android performance;
- correct player identity;
- profile creation/editing;
- navigation;
- responsiveness;
- persistence;
- coherent social experience;
- scalable architecture.

---

# SECTION 1 — ABSOLUTE PRIORITY

Before adding new major features:

STOP.

Audit the actual product from the perspective of a human using a physical Android phone.

Do not trust:

- completion percentages;
- README claims;
- feature checklists;
- endpoint counts;
- service counts;
- documentation statements.

Verify actual runtime behavior.

The executable application is the source of truth.

---

# SECTION 2 — PLAYER IDENTITY IS BROKEN UNTIL PROVEN OTHERWISE

The human player MUST have a dedicated persistent identity.

The player is NOT an NPC.

The player is NOT "the first NPC returned by the API."

The player is NOT selected from the NPC browsing list.

The player identity must be explicitly represented.

Required conceptual architecture:

WORLD
├── PLAYER
│    └── persistent PlayerId
│
└── NPC POPULATION
     ├── NPC 1
     ├── NPC 2
     ├── NPC 3
     └── ...

The player must have:

- persistent ID
- handle
- display name
- bio
- avatar
- profile
- interests
- posts
- followers
- following
- communities
- messages
- notifications
- social reputation

The player must remain the same person after:

- app restart
- server restart
- reconnect
- database migration
- world reload

---

# SECTION 3 — CURRENT USER API

Implement a proper current-user abstraction.

Provide an endpoint conceptually equivalent to:

GET /api/me

The Android client must retrieve the authenticated/current player from this endpoint.

Do NOT infer the current player from:

GET /api/npcs

The NPC endpoint should continue excluding the player.

Android must have:

currentPlayer

as explicit state.

---

# SECTION 4 — AUTHENTICATION / SESSION IDENTITY

The client must retain a persistent authenticated player identity.

The server must know:

which player is performing each action.

Do NOT require the client to send arbitrary:

FollowerId
SenderId
AuthorId

for security-sensitive player actions without server validation.

The backend must derive authenticated player identity from the current session/authentication context where practical.

---

# SECTION 5 — PLAYER PROFILE CREATION

The application must support creating a new in-world player character.

First-world flow:

Create Character
→ choose display name
→ choose handle
→ choose avatar
→ write bio
→ select interests
→ create profile
→ enter social world

The player character becomes persistent.

The profile must never silently become one of the NPCs.

---

# SECTION 6 — PLAYER PROFILE EDITING

The human must be able to edit their profile.

Support:

- display name
- handle
- bio
- avatar
- interests where appropriate
- profile presentation

Do NOT allow arbitrary editing of simulation consequences such as:

- popularity
- relationship values
- trust
- hostility
- NPC memories
- NPC opinions

The player controls their identity.

The simulation controls consequences.

---

# SECTION 7 — PROFILE SCREEN

The player's Profile screen must actually represent the player.

It must not display a randomly selected NPC.

The player profile should show:

- avatar
- name
- handle
- bio
- followers
- following
- posts
- communities

Provide an obvious:

Edit Profile

action.

---

# SECTION 8 — NPC PROFILE VS PLAYER PROFILE

These must be separate concepts.

NPC profile:

view another simulated user.

Player profile:

manage the human's character.

Do not reuse one state variable to represent both.

---

# SECTION 9 — REMOVE SELECTED-NPC-AS-PLAYER ARCHITECTURE

Do not use:

selectedNpc

as a proxy for:

currentPlayer.

The Android client must distinguish:

currentPlayer
from
selectedNpc.

selectedNpc:
currently viewed NPC.

currentPlayer:
authenticated human character.

These are fundamentally different.

---

# SECTION 10 — PLAYER ACTION OWNERSHIP

When the player:

- likes;
- dislikes;
- posts;
- comments;
- replies;
- shares;
- follows;
- joins;
- attends;
- sends a DM;

the server must know that the action belongs to the authenticated human player.

Do not ask the UI to manually choose an arbitrary NPC ID for the action.

---

# SECTION 11 — FREEZE FEATURE EXPANSION

Until the application passes the product usability audit:

DO NOT add unrelated features.

Do not keep adding:

- random screens;
- random endpoints;
- placeholder services;
- additional simulation mechanics;

while the existing application is difficult to use.

The current features must become coherent first.

---

# SECTION 12 — ANDROID PERFORMANCE IS NOW P0

The Android application must be treated as a performance-critical product.

The goal is:

## FAST, SMOOTH, RESPONSIVE SOCIAL-MEDIA UX.

Do not accept:

- janky scrolling;
- lag after tapping buttons;
- whole-feed reloads;
- unnecessary recompositions;
- blocking network operations;
- full-screen loading for small actions;
- excessive animations;
- giant state objects;
- redundant API requests.

---

# SECTION 13 — PROFILE THE ACTUAL DEVICE

Use the connected physical Android phone.

Measure:

- frame time;
- jank;
- dropped frames;
- startup time;
- screen transition latency;
- memory;
- CPU;
- network latency;
- battery/thermal impact;
- crashes.

Do not guess why the application is slow.

Profile it.

Use Android profiling tools and runtime logs.

---

# SECTION 14 — COMPOSE PERFORMANCE

Audit all Compose screens.

Pay particular attention to:

- large composables;
- unnecessary recomposition;
- mutableStateOf of large collections;
- unstable models;
- unnecessary lambdas;
- expensive modifiers;
- per-item animations;
- image decoding;
- repeated network refreshes.

Use stable immutable UI state where appropriate.

Break the enormous MainScreen implementation into focused components and screen ViewModels.

---

# SECTION 15 — DO NOT USE ANIMATIONS AS DECORATION

Remove or minimize animations that provide little UX value.

Do not wrap every feed post in unnecessary enter animations.

Scrolling performance takes priority over decorative animation.

Animations must be:

- intentional;
- short;
- lightweight;
- measurable.

---

# SECTION 16 — NO WHOLE-FEED RELOAD AFTER LIKE

Current behavior where:

LIKE
→ HTTP request
→ reload entire feed

is unacceptable for the final product.

Implement:

LIKE
→ update visible item immediately
→ send server request asynchronously
→ reconcile response

Use optimistic UI where safe.

Rollback only if server rejects the operation.

---

# SECTION 17 — NO WHOLE-DATABASE RELOAD AFTER FOLLOW

Do not do:

FOLLOW
→ reload all initial data

Use targeted updates.

Update:

- follow button;
- follower count;
- relevant local state;

without refreshing unrelated screens.

---

# SECTION 18 — POST CREATION

Creating a post should:

- submit once;
- return created entity;
- insert it into local UI state;
- scroll/position appropriately;
- avoid reloading the entire feed.

---

# SECTION 19 — LOCAL CLIENT CACHE

Implement Room or an equivalent local persistence layer for the Android client.

Use it for:

- recent feed;
- cached profiles;
- conversations;
- notifications;
- basic user state.

The server remains authoritative.

The local database is a cache/read model.

---

# SECTION 20 — FAST APP START

The app should:

1. load cached user/profile;
2. display cached feed immediately;
3. start network refresh;
4. update UI when fresh data arrives.

Do not make the user stare at a blank loading screen on every launch.

---

# SECTION 21 — PARALLEL INITIAL DATA FETCH

Do not unnecessarily perform independent startup requests serially.

Use structured concurrency to fetch independent data concurrently.

Only sequence requests when there is an actual dependency.

---

# SECTION 22 — STATE ARCHITECTURE

Do not store the entire application state in one giant ViewModel.

Use focused state owners.

Examples:

HomeViewModel
ExploreViewModel
MessagesViewModel
ProfileViewModel
CommunityViewModel
EventViewModel
NotificationsViewModel

Global session state:

SessionState/currentPlayer

Global realtime state:

RealtimeManager

---

# SECTION 23 — SCREEN SEPARATION

The enormous MainScreen implementation must be decomposed.

Separate:

- navigation;
- home;
- feed;
- create;
- messages;
- profile;
- notifications;
- explore.

Shared components should live in reusable UI modules.

---

# SECTION 24 — FEED ARCHITECTURE

The feed should be an independently managed paginated stream.

Use:

- cursor pagination;
- cached pages;
- incremental loading;
- local state mutation.

Do not repeatedly replace the entire collection for every interaction.

---

# SECTION 25 — FEED ITEM STABILITY

A single post changing:

likes from 17 → 18

must not force every visible post to rebuild.

Update only the affected post state.

Use stable keys.

Use immutable models and targeted state updates.

---

# SECTION 26 — IMAGES

Audit avatar/image loading.

Use:

- image caching;
- appropriately sized assets;
- placeholders;
- asynchronous loading;
- memory-safe decoding.

Never load giant images for tiny avatars.

---

# SECTION 27 — NAVIGATION

Every major screen must have clear navigation.

Required core destinations:

- Home
- Explore
- Create
- Messages
- Notifications
- Profile

Secondary destinations:

- Post detail
- NPC profile
- Community
- Event
- Search

Android back navigation must always behave naturally.

---

# SECTION 28 — NO DEAD BUTTONS

Every visible button must:

- work;
- navigate;
- perform an action;
- or not exist.

No placeholder controls.

No commented-out click handlers in product UI.

For example, a notification button currently containing an empty click action is not acceptable.

Implement it or remove it.

---

# SECTION 29 — USER JOURNEY

The following journey must be completely functional:

Launch
→ see player identity
→ browse feed
→ open post
→ like
→ comment
→ open author
→ follow
→ search
→ open community
→ view event
→ open notifications
→ open DM
→ send message
→ receive NPC reply
→ create post
→ open own profile
→ edit profile

No dead ends.

---

# SECTION 30 — FIRST-LAUNCH CHARACTER SETUP

A fresh installation/world should guide the player through character setup before entering the main social feed.

The player should never accidentally enter the world as a random NPC.

---

# SECTION 31 — POPULATION ARCHITECTURE

The current 20 NPC population is an initial development population, NOT the final product population.

Design population management so it can scale:

20
→ 50
→ 100
→ 250
→ 500
→ 1,000
→ potentially 5,000–10,000

Do not blindly spawn 10,000 NPCs until the engine has been benchmarked.

---

# SECTION 32 — POPULATION MUST NOT REQUIRE ANDROID LOADING

Android must never receive the entire NPC population.

Explore must paginate/search.

The client should only retrieve the users required for the current screen.

---

# SECTION 33 — POPULATION GENERATION

Create a dedicated population generation system.

It must be able to create NPCs from:

- archetypes;
- personality distributions;
- interests;
- demographic data if modeled;
- activity profiles;
- social clusters.

Population generation should be deterministic from a seed where appropriate.

---

# SECTION 34 — INITIAL SOCIAL GRAPH

Do not create 1,000 strangers with no connections.

Population generation should create:

- social clusters;
- community clusters;
- friends;
- acquaintances;
- influencers;
- lurkers;
- rivals;
- interest communities.

Then the simulation evolves the graph.

---

# SECTION 35 — PRODUCT VS DEVELOPMENT POPULATION

Development:

20–50 NPCs.

Stress testing:

100–10,000 synthetic NPCs.

Actual early playable world:

configurable population.

Do not confuse test scale with product scale.

---

# SECTION 36 — SERVER PAGINATION

All large collections must be paginated.

Examples:

- NPCs;
- followers;
- following;
- posts;
- comments;
- messages;
- notifications;
- communities.

Do not load entire tables and paginate in application memory.

Push filtering, ordering, and pagination into SQLite queries where possible.

---

# SECTION 37 — DATABASE QUERY AUDIT

Audit every repository/controller query for:

- unnecessary ToListAsync;
- in-memory filtering;
- in-memory ordering;
- eager loading of unnecessary relationships;
- N+1 queries;
- repeated First calls;
- excessive payload size.

At large population scale, these patterns are unacceptable.

---

# SECTION 38 — DATABASE PROJECTION

For list endpoints:

project directly to DTOs where possible.

Do not load entire entity graphs when the client only needs:

id
name
avatar
counts
timestamp

---

# SECTION 39 — MESSAGE QUERY

Do not load an entire conversation into memory and then paginate it.

Use database-level:

ORDER BY
LIMIT
cursor

where possible.

---

# SECTION 40 — BACKEND RESPONSE SIZE

DTOs must contain only information needed by the screen.

Never send:

- full personality vectors;
- internal memories;
- private relationships;
- unnecessary metadata;

to normal client views.

---

# SECTION 41 — CURRENT PLAYER ENDPOINT

Implement proper player retrieval.

The Android client must be able to ask:

GET /api/me

and receive the player's public profile.

---

# SECTION 42 — PLAYER PROFILE MUTATIONS

Implement clear endpoints for:

PUT /api/me/profile

and avatar handling if supported.

Use server-side validation.

Do not expose generic NPC mutation endpoints as a substitute.

---

# SECTION 43 — PLAYER POSTS

The post author must come from authenticated player context.

The client must not fake an NPC ID.

---

# SECTION 44 — PLAYER COMMENTS

Same rule.

Comments made by the player must be associated with the player's permanent identity.

---

# SECTION 45 — PLAYER DMS

The sender must be the authenticated player.

The Android client should not be able to claim:

"I am NPC 12"

by simply changing an ID in JSON.

---

# SECTION 46 — SOCIAL CONSEQUENCES

Player actions must remain connected to simulation state.

Example:

Player insults Sarah
→ comment created
→ Sarah learns event
→ relationship changes
→ emotion changes
→ memory created
→ future behavior changes.

Test this end-to-end.

---

# SECTION 47 — PRODUCT IMMERSION

Do not expose internal simulation language to the normal player.

The normal UI should say:

"Alex"

not:

"NPC #427"

The player should perceive users as people within the fictional social network.

Developer information belongs in developer mode.

---

# SECTION 48 — DEVELOPER MODE

Maintain developer tools separately.

Developer tools may expose:

- NPC IDs;
- relationship scores;
- memories;
- utility;
- world time;
- AI queue;
- simulation speed.

Normal product UI must not.

---

# SECTION 49 — REALTIME

Implement WebSocket/realtime updates properly.

When relevant:

- NPC replies;
- DMs;
- notifications;
- new comments;
- important social activity;

should arrive without requiring manual refresh.

Do not rebuild the entire feed on every realtime event.

---

# SECTION 50 — REALTIME GRANULARITY

Send targeted updates.

Bad:

"refresh entire application"

Good:

"comment X was added to post Y."

The client updates only affected state.

---

# SECTION 51 — NOTIFICATIONS

Implement a real notification experience.

Unread badge.

Notification list.

Navigation to relevant content.

Read/unread state.

Grouping for high-volume low-value notifications.

---

# SECTION 52 — NPC RESPONSE EXPERIENCE

Player:

sends DM.

UI immediately shows outgoing message.

Server processes.

Qwen generates reply.

Reply arrives asynchronously.

Do not block the entire application waiting for Qwen.

---

# SECTION 53 — AI FAILURE

If Ollama is offline:

The application continues working.

NPC reply may fall back to deterministic behavior.

No app-wide loading state.

No crash.

---

# SECTION 54 — PRODUCT PERFORMANCE IS SEPARATE FROM AI PERFORMANCE

A slow Qwen response must not make:

- feed scrolling;
- navigation;
- profile browsing;
- liking;
- following;

feel slow.

Keep AI work isolated from UI state.

---

# SECTION 55 — ANDROID ERROR STATES

Create polished:

- loading;
- empty;
- error;
- reconnecting;
- offline;

states.

Do not expose exception text.

---

# SECTION 56 — EMPTY FEED

If feed has no content:

do not show a dead blank screen.

Provide useful next steps:

Explore people.
Explore communities.
Create a post.

---

# SECTION 57 — EMPTY MESSAGES

Use:

"Your inbox is quiet."

with a path to discover people.

---

# SECTION 58 — EMPTY NOTIFICATIONS

Use:

"You're all caught up."

Do not show a blank screen.

---

# SECTION 59 — PRODUCT LANGUAGE

Normal product UI must never require the player to know:

- AI provider;
- Qwen;
- LLM;
- simulation tick;
- utility score;
- NPC state.

---

# SECTION 60 — PERFORMANCE REGRESSION GATE

Every significant Android change must be tested on the physical device.

Measure before/after when the change is performance related.

Do not accept:

"Seems fine."

Use evidence.

---

# SECTION 61 — LONG FEED TEST

Test:

- 20 posts;
- 100 posts;
- 500 posts;
- long scrolling sessions.

Check:

- FPS/jank;
- memory;
- image cache;
- state growth.

---

# SECTION 62 — LONG SESSION TEST

Open app.

Navigate repeatedly.

Scroll.

Open posts.

Open profiles.

Return.

Open DMs.

Return.

Search.

Return.

Repeat.

Watch for memory growth and increasing latency.

---

# SECTION 63 — PRODUCT QA

Test as a real user.

Do not only use:

curl
Postman
database queries
unit tests

Those test machinery.

They do not test the product.

---

# SECTION 64 — REAL HUMAN WALKTHROUGH

Perform the complete application walkthrough on the actual Android phone.

Do not read documentation while doing it.

Pretend you do not know how the product works.

Any confusion becomes a task.

---

# SECTION 65 — NO EXCUSES FOR CONFUSING UX

Do not solve confusing UX by adding explanatory developer documentation.

Fix the UI.

---

# SECTION 66 — DESIGN SYSTEM

Create a consistent reusable UI system.

Shared components:

- PostCard
- UserAvatar
- UserHeader
- ActionBar
- CommentItem
- MessageBubble
- CommunityCard
- EventCard
- NotificationItem
- ProfileHeader
- Skeleton
- EmptyState
- ErrorState

---

# SECTION 67 — VISUAL POLISH

Audit:

- typography;
- spacing;
- alignment;
- colors;
- icons;
- cards;
- buttons;
- avatars;
- interaction states.

Every screen must look like it belongs to the same application.

---

# SECTION 68 — DO NOT OVERDESIGN

The app should feel like a polished social network.

Do not fill every screen with cards, gradients, animations, and giant buttons.

Prioritize information density and readability.

---

# SECTION 69 — PROFILE UX

Profile should feel like a real social profile.

Include:

- avatar;
- name;
- handle;
- bio;
- counts;
- posts;
- communities;
- follow/edit action.

Do not expose internal AI/simulation attributes.

---

# SECTION 70 — EXPLORE UX

Explore should actually help discovery.

Provide:

- trending;
- people;
- communities;
- events;
- search.

Not merely a raw NPC list.

---

# SECTION 71 — COMMUNITY UX

Community detail should show:

- identity;
- description;
- members;
- recent posts;
- activity;
- join/leave.

---

# SECTION 72 — EVENT UX

Event detail should show:

- title;
- host;
- time;
- attendees;
- description;
- response;
- status.

---

# SECTION 73 — SEARCH

Search should be fast and useful.

Use server-side filtering.

Do not download all NPCs and search them on Android.

---

# SECTION 74 — OFFLINE RETURN

When the player returns after being away:

show the actual world consequences.

Do not merely show:

"Welcome back."

Provide meaningful social changes.

---

# SECTION 75 — PLAYER'S WORLD

The player must feel ownership of the character.

At no point should the app unexpectedly replace the player's identity with:

- an NPC;
- a seeded test user;
- a hardcoded "Zoe";
- a random database entry.

Any development default identity must only exist in explicit development/test mode.

---

# SECTION 76 — TEST DATA

Development seed data must be clearly identifiable and removable.

Do not confuse seed player data with real player identity.

---

# SECTION 77 — WORLD INITIALIZATION

A newly created world should:

1. create player;
2. generate NPC population;
3. create social clusters;
4. seed communities;
5. seed content;
6. seed initial relationships;
7. establish simulation schedule;
8. enter live world.

The world must begin as a coherent social environment.

---

# SECTION 78 — POPULATION SCALING

Do not make the UI depend on a fixed value of 20.

Population size must be configuration-driven.

Examples:

Development:
20

Small world:
100

Medium:
500

Large:
1,000+

Stress:
10,000

---

# SECTION 79 — NO FIXED "20" THROUGHOUT THE SYSTEM

Search the entire codebase for:

20
20 NPCs
limit = 20
first NPC

and determine whether each occurrence is:

- deliberate pagination;
- test configuration;
- accidental product dependency.

Replace accidental fixed-population assumptions.

---

# SECTION 80 — PERFORMANCE ARCHITECTURE

The application must scale through:

- pagination;
- caching;
- LOD;
- scheduler;
- event filtering;
- social locality;
- targeted updates;
- asynchronous AI.

Never through:

"load everything and hope."

---

# SECTION 81 — ANDROID NETWORK EFFICIENCY

Avoid:

- duplicate requests;
- full refreshes;
- unnecessary polling;
- redundant endpoint calls;
- giant JSON payloads.

Use:

- cache;
- cursor pagination;
- WebSockets;
- local state updates.

---

# SECTION 82 — SERVER QUERY EFFICIENCY

No production list endpoint should:

1. load an enormous entity list;
2. perform most filtering in memory;
3. sort in memory;
4. then paginate.

Push work into SQLite wherever possible.

---

# SECTION 83 — PRODUCT READINESS CHECK

A feature is not complete because:

- endpoint works;
- database table exists;
- UI exists;
- method compiles.

It is complete only when:

A human can use it naturally.

---

# SECTION 84 — REQUIRED PLAYER TEST

Create a fresh world.

Perform:

1. Create player profile.
2. Enter world.
3. Browse feed.
4. Create post.
5. Like post.
6. Comment.
7. Follow NPC.
8. Open NPC profile.
9. Open own profile.
10. Edit own profile.
11. Send DM.
12. Receive NPC reply.
13. Open notification.
14. Join community.
15. Open event.
16. Close app.
17. Restart app.
18. Confirm identity remains unchanged.
19. Advance world.
20. Confirm social consequences persist.

This must succeed.

---

# SECTION 85 — REQUIRED PERFORMANCE TEST

On physical Android device:

Measure:

- cold launch;
- warm launch;
- feed opening;
- feed scrolling;
- profile opening;
- post interaction;
- navigation;
- DM opening.

Identify actual bottlenecks.

Fix them.

Repeat measurements.

---

# SECTION 86 — REQUIRED BACKEND TEST

With at least 100 NPCs:

- simulate activity;
- load feed;
- search users;
- open profiles;
- process social actions;
- inspect database latency.

Then repeat at:

500
1,000

before claiming scale readiness.

---

# SECTION 87 — DEVELOPMENT ORDER

Perform work in this order:

## PHASE A
Current-player identity.

## PHASE B
Player character creation.

## PHASE C
Player profile editing.

## PHASE D
Android architecture/performance refactor.

## PHASE E
Feed state/cache optimization.

## PHASE F
Navigation/product UX cleanup.

## PHASE G
Realtime updates.

## PHASE H
Backend query optimization.

## PHASE I
Population generation/scalability.

## PHASE J
Long-session/offline testing.

Do not reverse this order without a documented reason.

---

# SECTION 88 — COMPLETION STATUS

Replace simplistic:

"98% complete"

metrics.

Track separately:

- backend functionality;
- simulation depth;
- Android functionality;
- Android UX;
- Android performance;
- player identity;
- persistence;
- AI quality;
- scale readiness;
- release readiness.

A feature-rich but unusable app is NOT 98% complete.

---

# SECTION 89 — DOCUMENTATION HONESTY

Whenever documentation says:

COMPLETE

verify actual runtime behavior.

If only infrastructure exists:

mark:

PARTIAL.

If only endpoint/UI exists:

mark:

PARTIAL.

If behavior works end-to-end:

mark:

COMPLETE.

---

# SECTION 90 — STOP BUILDING DEMO FEATURES

Do not optimize for screenshots.

Do not optimize for:

"look, we have a Community screen."

Optimize for:

"a human can actually use Communities."

---

# SECTION 91 — STOP BUILDING CHECKLISTS

Do not build features merely because they appear in the roadmap.

Build them because they improve the actual product.

---

# SECTION 92 — FINAL PRODUCT STANDARD

The player should be able to:

create their own character,

enter a persistent social world,

browse a convincing feed,

interact naturally,

develop relationships,

send and receive messages,

discover communities,

participate in events,

leave the application,

return later,

and immediately know:

"This is my character, and this is the same world I left."

The application must feel:

FAST.
SMOOTH.
COHERENT.
RESPONSIVE.
PERSISTENT.
ALIVE.

---

# FINAL COMMAND

For this development phase:

## DO NOT ADD MORE FEATURES JUST TO INCREASE FEATURE COUNT.

## DO NOT CHASE A HIGHER COMPLETION PERCENTAGE.

## DO NOT CALL THE PROJECT PRODUCT READY BECAUSE IT COMPILES.

## FIX THE HUMAN EXPERIENCE.

The current objective is:

# MAKE ME WANT TO USE THIS APP.

Only after that objective is achieved should the project aggressively scale toward 1,000+ NPCs and additional simulation depth.

The product is successful when the technology disappears and the artificial society remains.