# Changelog

## Synthetic Social World

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [0.2.0] - 2025-09-03 - Foundation Complete

### Added

#### Backend API
- **ASP.NET Core 8 API** with full REST endpoints
- **SQLite database** with WAL mode and EF Core
- **20 NPCs** with unique personalities, moods, and interests
- **5 Communities** (Tech Talk, Gaming, Art & Music, Sports Fans, Foodies)
- **Posts and Comments** with engagement (likes, dislikes)
- **Multi-dimensional relationships** (10 dimensions per relationship)
- **Scheduled actions** for NPC activity
- **Simulation service** for background processing
- **Health check endpoint** at `/health`

#### LLM Integration
- **Ollama integration** with qwen3:4b model
- **IAIProvider interface** for abstracted AI calls
- **Post generation** - NPCs generate posts via LLM
- **Comment generation** - NPCs generate comments via LLM
- **Fallback content** when Ollama unavailable
- **Token budget** with max 100 tokens per request
- **`/no think` prefix** for concise responses

#### Android App
- **Kotlin + Jetpack Compose** native UI
- **MVVM architecture** with ViewModel and StateFlow
- **Hilt dependency injection**
- **Retrofit** REST API client
- **5-tab navigation**: Home, Explore, Create, Messages, Profile
- **Home Feed** with post display
- **Explore Screen** showing all NPCs
- **Create Post** form with submit
- **Messages Screen** showing NPC contacts
- **Profile Screen** with simulation controls
- **Like/Dislike** post interactions

#### Simulation Features
- **World Clock** with pause/resume/speed control
- **Activity levels** (Lurker, Casual, Active, Highly Active, Influencer)
- **Mood tracking** (10 emotional dimensions)
- **Personality traits** (15+ dimensions per NPC)
- **Interest system** with topic weights
- **Goal tracking** per NPC

### Working Endpoints
- `GET /health` - Health check
- `POST /api/auth/login` - Player authentication
- `GET /api/npcs` - List all NPCs
- `GET /api/npcs/{id}` - Get NPC details
- `GET /api/posts` - List posts
- `POST /api/posts` - Create post
- `GET /api/feed` - Get feed
- `POST /api/posts/{id}/like` - Like post
- `POST /api/posts/{id}/dislike` - Dislike post
- `GET /api/posts/{id}/comments` - Get comments
- `POST /api/posts/{id}/comments` - Add comment
- `POST /api/users/{id}/follow` - Follow NPC
- `GET /api/communities` - List communities
- `POST /api/communities/{id}/join` - Join community
- `GET /api/events` - List events
- `GET /api/messages` - Get conversations
- `POST /api/messages/{userId}` - Send DM
- `GET /api/notifications` - Get notifications
- `POST /api/simulation/advance` - Advance time
- `GET /api/simulation/stats` - Get statistics

---

## [0.1.0] - 2025-09-03 - Repository Foundation

### Added

#### Architecture Documentation
Complete documentation suite in `docs/`:
- **SYSTEM_DIRECTIVE.md** - Core engineering constitution
- **ARCHITECTURE.md** - System overview and principles
- **DATABASE.md** - SQLite schema design
- **SIMULATION.md** - NPC behavior and world simulation
- **AI_SYSTEM.md** - Ollama integration and LLM orchestration
- **MEMORY_SYSTEM.md** - Memory and knowledge management
- **SOCIAL_GRAPH.md** - Relationships and communities
- **FEED_SYSTEM.md** - Feed ranking algorithm
- **API.md** - REST and WebSocket API
- **ANDROID.md** - Native Android client architecture
- **PERFORMANCE.md** - Performance targets
- **TESTING.md** - Testing strategy
- **DECISIONS.md** - Architecture decision records
- **ROADMAP.md** - Implementation phases
- **COMPLETION_STATUS.md** - Feature tracking

#### Project Structure
```
src/
├── Backend/
│   ├── SyntheticSocialWorld.Api/
│   ├── SyntheticSocialWorld.Domain/
│   ├── SyntheticSocialWorld.Infrastructure/
│   └── SyntheticSocialWorld.Simulation/
└── Android/
    └── SyntheticSocialWorld/
```

---

## Versioning

This project uses semantic versioning with the following format:

`MAJOR.MINOR.PATCH`

- **MAJOR**: Incompatible changes to the simulation engine or API
- **MINOR**: New features in a backwards-compatible manner
- **PATCH**: Backwards-compatible bug fixes

### Current Version: 0.2.0

This is pre-1.0 software. API and architecture may change significantly between minor versions during the initial development phases.

---

## Migration Guides

### Upgrading from 0.1.x to 0.2.x
- Backend now requires .NET 10
- Android app now requires Android SDK 35+
- Database schema has been updated

### Upgrading from 0.0.x to 0.1.x
Initial release - no migration needed.

### Future Migration Guides
Will be added as needed for breaking changes.

---

## Related Documents

- [ROADMAP.md](./ROADMAP.md) - Implementation phases
- [DECISIONS.md](./DECISIONS.md) - Technical decisions
- [ARCHITECTURE.md](./ARCHITECTURE.md) - System design
- [COMPLETION_STATUS.md](./COMPLETION_STATUS.md) - Feature tracking
