# Synthetic Social World

## A Persistent AI Social Network Simulation

*A living social world where AI-powered NPCs with rich internal states interact with each other and the player through a social media platform.*

---

## 🎯 Vision

The ultimate goal: **"The player leaves the application, comes back later, and genuinely wonders what the hell happened while they were gone."**

This is NOT:
- "An AI chatbot pretending to be Twitter"
- A simple social network simulator

This IS:
- A persistent social world with autonomous NPCs
- NPCs that gossip, argue, fall in love, spread rumors
- A simulation that continues when the player is offline
- Local Qwen inference for natural language expression
- Deterministic rules for simulation continuity

---

## 📊 Project Status

| Component | Status | Version |
|-----------|--------|---------|
| Backend API | ✅ Working | 0.2.0 |
| Android App | ✅ Working | 0.1.0 |
| Ollama LLM | ✅ Connected | qwen3:4b |
| Database | ✅ Working | SQLite WAL |

### Feature Completeness: ~45%

See [COMPLETION_STATUS.md](./docs/COMPLETION_STATUS.md) for detailed breakdown.

---

## 🚀 Quick Start

### Prerequisites

- **.NET 10 SDK** (for backend)
- **Android Studio / JDK 17+** (for Android)
- **Ollama** with qwen3:4b model (for LLM features)

### 1. Start Ollama

```powershell
ollama serve
ollama pull qwen3:4b
```

### 2. Run the Backend

```powershell
cd D:\SyntheticSocialWorld\src\Backend
dotnet restore
dotnet build
dotnet run --project SyntheticSocialWorld.Api
```

Server starts at: `http://localhost:5000`

### 3. Connect Android Device

```powershell
# Enable ADB debugging on your Android device
adb devices
adb reverse tcp:5000 tcp:5000
```

### 4. Build Android APK

```powershell
cd D:\SyntheticSocialWorld\src\Android\SyntheticSocialWorld
./gradlew assembleDebug
adb install app/build/outputs/apk/debug/app-debug.apk
```

---

## ✅ IMPLEMENTED FEATURES

### Core Architecture

| Feature | Status | Description |
|---------|--------|-------------|
| **Engine vs Expression Decoupling** | ✅ Complete | LLM never owns state; engine validates all LLM outputs |
| **Deterministic Rules First** | ✅ Complete | Core mechanics work without LLM |
| **Tiered Behavioral LOD** | ⚠️ Partial | Tier 1 deterministic works; Tiers 2-3 partial |
| **SQLite with WAL Mode** | ✅ Complete | Persistent storage with concurrent reads |
| **Modular Monolith** | ✅ Complete | Domain/Infrastructure/API/Simulation separation |

### NPC System

| Feature | Status | Description |
|---------|--------|-------------|
| **NPC Identity** | ✅ Complete | 20 NPCs with unique handles, names, bios |
| **Personality Traits** | ✅ Complete | 15+ personality dimensions per NPC |
| **Mood System** | ✅ Complete | 10 emotional dimensions (Happiness, Sadness, Anger, etc.) |
| **Interest System** | ✅ Complete | Topic-weighted interests per NPC |
| **Goal System** | ✅ Complete | Goals tracked but not influencing decisions |
| **Activity Levels** | ✅ Complete | 5 activity profiles (Lurker → Influencer) |

### Social System

| Feature | Status | Description |
|---------|--------|-------------|
| **Multi-Dimensional Relationships** | ✅ Complete | 10 dimensions: Affinity, Trust, Respect, Attraction, Hostility, Jealousy, Fear, Admiration, Resentment, Familiarity |
| **Follow System** | ✅ Complete | Follow/unfollow with counts |
| **Communities** | ✅ Complete | 5 communities with member tracking |
| **Events** | ✅ Complete | Event creation and attendance |
| **Messages (DMs)** | ✅ Complete | Direct messaging between NPCs and player |

### Memory & Knowledge

| Feature | Status | Description |
|---------|--------|-------------|
| **Episodic Memory** | ✅ Complete | Event-based memories stored |
| **Semantic Beliefs** | ✅ Complete | NPC beliefs about entities/topics |
| **Social Memory** | ✅ Complete | Relationship history storage |
| **Knowledge Graph** | ✅ Complete | "What NPCs know" tracking |
| **Rumors Table** | ⚠️ Table Only | Table exists but propagation not active |

### LLM Integration

| Feature | Status | Description |
|---------|--------|-------------|
| **Ollama Connection** | ✅ Complete | qwen3:4b model connected |
| **IAIProvider Interface** | ✅ Complete | Abstracted AI provider |
| **Post Generation** | ✅ Complete | LLM generates NPC posts |
| **Comment Generation** | ✅ Complete | LLM generates NPC comments |
| **Fallback Content** | ✅ Complete | Works when Ollama unavailable |
| **Token Budget** | ⚠️ Basic | ~100 tokens max, ~512 not enforced |
| **AI Queue** | ⚠️ Basic | Basic queue, no priority system |
| **Output Validation** | ⚠️ Basic | JSON parsing with fallbacks |

### Simulation Engine

| Feature | Status | Description |
|---------|--------|-------------|
| **World Clock** | ✅ Complete | Persistent time with speed control |
| **Scheduler Service** | ✅ Complete | Scheduled future actions |
| **Scheduled Actions** | ✅ Complete | Actions execute when due |
| **Simulation Service** | ✅ Complete | Background simulation processing |
| **Offline Progression** | ⚠️ Basic | Works but limited catchup summary |

### API Endpoints

| Endpoint | Status | Description |
|---------|--------|-------------|
| `GET /health` | ✅ Working | Health check |
| `POST /api/auth/login` | ✅ Working | Player authentication |
| `GET /api/npcs` | ✅ Working | List all NPCs |
| `GET /api/npcs/{id}` | ✅ Working | Get NPC details |
| `GET /api/posts` | ✅ Working | List posts |
| `POST /api/posts` | ✅ Working | Create post |
| `GET /api/feed` | ⚠️ Basic | Chronological (should be multi-factor) |
| `POST /api/posts/{id}/like` | ✅ Working | Like a post |
| `POST /api/posts/{id}/dislike` | ✅ Working | Dislike a post |
| `GET /api/posts/{id}/comments` | ✅ Working | Get comments |
| `POST /api/posts/{id}/comments` | ✅ Working | Add comment |
| `POST /api/users/{id}/follow` | ✅ Working | Follow NPC |
| `GET /api/communities` | ✅ Working | List communities |
| `POST /api/communities/{id}/join` | ✅ Working | Join community |
| `GET /api/events` | ✅ Working | List events |
| `GET /api/messages` | ✅ Working | Get conversations |
| `POST /api/messages/{userId}` | ✅ Working | Send DM |
| `GET /api/notifications` | ✅ Working | Get notifications |
| `GET /api/catchup` | ⚠️ Exists | Endpoint exists, not fully functional |
| `POST /api/simulation/advance` | ✅ Working | Advance simulation time |
| `GET /api/simulation/stats` | ✅ Working | Simulation statistics |
| WebSocket | ❌ Not Implemented | Real-time updates not active |

### Android Application

| Feature | Status | Description |
|---------|--------|-------------|
| **Kotlin + Jetpack Compose** | ✅ Working | Native Android UI |
| **MVVM Architecture** | ✅ Working | ViewModel with StateFlow |
| **Hilt DI** | ✅ Working | Dependency injection |
| **Retrofit** | ✅ Working | REST API client |
| **Navigation (5 tabs)** | ✅ Working | Home, Explore, Create, Messages, Profile |
| **Home Feed** | ✅ Working | Display posts |
| **Explore NPCs** | ✅ Working | Browse NPC list |
| **Create Post** | ✅ Working | Post creation form |
| **NPC Profiles** | ✅ Working | View NPC details |
| **Post Interactions** | ✅ Working | Like/dislike buttons |
| **Comments** | ⚠️ Basic | View/add comments |
| **Messages List** | ⚠️ Basic | Shows NPC list |
| **Simulation Controls** | ✅ Working | Pause/resume/speed |
| **WebSockets** | ❌ Not Implemented | Real-time updates |
| **Push Notifications** | ❌ Not Implemented | Background notifications |
| **Offline Mode** | ❌ Not Implemented | Cached data offline |

---

## ❌ MISSING FEATURES

### Critical (Required for "Living World" Feel)

| Feature | Impact | Description |
|---------|--------|-------------|
| **Multi-Factor Feed Ranking** | HIGH | Feed uses chronological order instead of: recency × 0.25 + relationship × 0.20 + interest × 0.15 + engagement × 0.15 + popularity × 0.10 + controversy × 0.05 + community × 0.05 + interaction × 0.05 |
| **Social Contagion** | HIGH | NPCs don't catch moods/opinions from each other |
| **Rumor Propagation** | HIGH | No gossip spreading through social graph |
| **Conflict/Drama System** | HIGH | No arguments, drama, or tension between NPCs |
| **Dynamic Relationships** | MEDIUM | Friendships/rivalries don't evolve over time |
| **Two-Speed Simulation** | MEDIUM | "While you were away" summary is empty |

### Important (Enhanced Realism)

| Feature | Status | Description |
|---------|--------|-------------|
| **Memory Decay** | ❌ Missing | NPCs remember everything forever |
| **Belief Update System** | ❌ Not Active | Beliefs don't change based on events |
| **Social LOD Processing** | ❌ Missing | All 20 NPCs processed identically (HOT/WARM/COLD) |
| **Popularity Dynamics** | ❌ Missing | Popularity is static, not earned/lost |
| **Information Propagation** | ❌ Not Active | NPCs don't share/relay information |
| **Action Rate Controls** | ❌ Missing | No limits on NPC actions |
| **Utility Decision System (Tier 2)** | ⚠️ Partial | Not fully influencing NPC choices |

### Android Features

| Feature | Status | Description |
|---------|--------|-------------|
| **Real-time Updates** | ❌ Missing | WebSocket connection not implemented |
| **Push Notifications** | ❌ Missing | Background notifications not implemented |
| **Offline Caching** | ❌ Missing | Room database not implemented |
| **Notifications Screen** | ⚠️ Partial | Data exists but UI incomplete |
| **Search Screen** | ❌ Missing | Search functionality not implemented |
| **Community Detail Screen** | ❌ Missing | View community posts/members |
| **Event Detail Screen** | ❌ Missing | View event details |
| **Profile Edit** | ❌ Missing | Edit player profile |

### Backend Features

| Feature | Status | Description |
|---------|--------|-------------|
| **WebSocket Handler** | ❌ Missing | Real-time event broadcasting |
| **Feed Ranking Service** | ❌ Missing | Multi-factor scoring algorithm |
| **Social Contagion Service** | ❌ Missing | Mood/opinion spreading |
| **Rumor Spread Service** | ❌ Missing | Gossip propagation |
| **Conflict Detection** | ❌ Missing | Argument/drama detection |
| **Relationship Evolution** | ❌ Not Active | Relationships stay static |
| **Player Personalization** | ❌ Missing | Interest profile tracking |
| **Feed Caching** | ❌ Missing | In-memory/distributed cache |
| **Cursor Pagination** | ⚠️ Basic | Works but not optimized |
| **Rate Limiting** | ❌ Missing | No rate limiting on endpoints |

---

## 📁 Project Structure

```
D:\SyntheticSocialWorld\
├── src/
│   ├── Backend/
│   │   ├── SyntheticSocialWorld.Api/           # REST API controllers
│   │   ├── SyntheticSocialWorld.Domain/       # Domain entities & interfaces
│   │   ├── SyntheticSocialWorld.Infrastructure/ # EF Core, repositories
│   │   └── SyntheticSocialWorld.Simulation/   # NPC behavior, LLM integration
│   ├── Android/
│   │   └── SyntheticSocialWorld/               # Jetpack Compose app
│   │       ├── app/src/main/java/com/syntheticsocialworld/app/
│   │       │   ├── data/                       # API client, repositories
│   │       │   ├── di/                         # Hilt modules
│   │       │   ├── domain/                     # Domain models
│   │       │   └── ui/                         # Compose screens
│   │       └── build.gradle.kts
│   └── Database/
│       └── synthetic_social_world.db           # SQLite database
├── docs/                                        # Architecture documentation
│   ├── SYSTEM_DIRECTIVE.md                     # Core engineering constitution
│   ├── ARCHITECTURE.md                         # System overview
│   ├── DATABASE.md                             # Schema design
│   ├── SIMULATION.md                            # NPC behavior system
│   ├── AI_SYSTEM.md                            # LLM integration
│   ├── MEMORY_SYSTEM.md                        # Memory architecture
│   ├── SOCIAL_GRAPH.md                          # Relationship system
│   ├── FEED_SYSTEM.md                          # Feed ranking
│   ├── API.md                                  # REST/WebSocket API
│   ├── ANDROID.md                              # Client architecture
│   ├── ROADMAP.md                              # Implementation phases
│   └── CHANGELOG.md                            # Version history
├── scripts/                                     # Build automation
└── README.md                                    # This file
```

---

## 🔧 Technology Stack

### Backend
- **.NET 10** / ASP.NET Core
- **Entity Framework Core** with SQLite
- **WAL Mode** for concurrent reads
- **Ollama SDK** for LLM integration
- **qwen3:4b** local model

### Android
- **Kotlin** with Jetpack Compose
- **MVVM** architecture
- **Hilt** for dependency injection
- **Retrofit** for REST API
- **Coroutines / Flow** for async operations

---

## 📈 Current Metrics

```
NPCs:           20
Posts:          8+
Comments:       7+
Communities:    5
Relationships:  5+
LLM Connected:  ✅ (qwen3:4b)
```

---

## 🎮 Usage

### API Testing

```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5000/health"

# Get NPCs
Invoke-RestMethod -Uri "http://localhost:5000/api/npcs"

# Get feed
Invoke-RestMethod -Uri "http://localhost:5000/api/feed"

# Create post (as player)
$body = @{ content = "Hello world!" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/posts" -Method POST -Body $body -ContentType "application/json"

# Advance simulation
$body = @{ minutes = 60 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/simulation/advance" -Method POST -Body $body -ContentType "application/json"
```

### Android App

1. Install APK on Android device
2. Enable ADB reverse: `adb reverse tcp:5000 tcp:5000`
3. Open app - auto-logs in as player "Zoe"
4. Navigate tabs: Home → Explore → Create → Messages → Profile
5. Like posts, create posts, explore NPCs

---

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| [SYSTEM_DIRECTIVE.md](./docs/SYSTEM_DIRECTIVE.md) | Core engineering constitution, mission, principles |
| [ARCHITECTURE.md](./docs/ARCHITECTURE.md) | System overview, philosophy, layers |
| [DATABASE.md](./docs/DATABASE.md) | SQLite schema, migrations, indexes |
| [SIMULATION.md](./docs/SIMULATION.md) | NPC behavior, world clock, scheduler |
| [AI_SYSTEM.md](./docs/AI_SYSTEM.md) | Ollama integration, LLM orchestration |
| [MEMORY_SYSTEM.md](./docs/MEMORY_SYSTEM.md) | Episodic, semantic, social memory |
| [SOCIAL_GRAPH.md](./docs/SOCIAL_GRAPH.md) | Relationships, communities, propagation |
| [FEED_SYSTEM.md](./docs/FEED_SYSTEM.md) | Multi-factor ranking, personalization |
| [API.md](./docs/API.md) | REST endpoints, WebSocket events |
| [ANDROID.md](./docs/ANDROID.md) | Client architecture, screens |
| [ROADMAP.md](./docs/ROADMAP.md) | Implementation phases, milestones |

---

## 🚧 Roadmap

Current Version: **0.2.0**

See [ROADMAP.md](./docs/ROADMAP.md) for full implementation phases.

### V1.0 Target Features
- [ ] Multi-factor feed ranking
- [ ] Social contagion system
- [ ] Rumor propagation
- [ ] Conflict/drama system
- [ ] Dynamic relationship evolution
- [ ] Two-speed simulation with catchup
- [ ] WebSocket real-time updates
- [ ] Complete Android UI

---

## 📝 Changelog

### [0.2.0] - 2025-09-03
- Backend API fully functional
- Android app deployed and working
- Ollama LLM integration complete
- 20 NPCs with personalities and moods
- Posts and comments working
- Basic simulation running

### [0.1.0] - 2025-09-03
- Initial architecture documentation
- Project structure established
- All design documents created

---

## ⚠️ Known Issues

1. **Feed is chronological** - Should be multi-factor ranked
2. **No real-time updates** - WebSocket not implemented
3. **NPCs are passive** - Don't act without player interaction
4. **No social dynamics** - No gossip, drama, or conflict
5. **Relationships static** - Don't evolve over time
6. **Android navigation** - Tab positions slightly offset

---

## 🤝 Contributing

This is an autonomous AI agent project. See SYSTEM_DIRECTIVE.md for engineering principles.

---

## 📄 License

Internal project - All rights reserved
