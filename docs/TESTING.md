# Testing Strategy

## Synthetic Social World - Testing Pyramid and Quality Assurance

---

## Testing Philosophy

> "Zero speculative completion. Never claim working unless actually tested."

---

## Testing Pyramid

```
                    ┌───────────────┐
                    │   End-to-End  │
                    │     Tests     │
                    └───────────────┘
                   ┌─────────────────┐
                   │  Integration   │
                   │    Tests       │
                   └─────────────────┘
              ┌───────────────────────┐
              │      Unit Tests        │
              │  (Domain, Services)    │
              └───────────────────────┘
```

---

## Unit Tests

### Scope
- Domain entities and value objects
- Utility functions
- Utility scoring algorithms
- Relationship calculations
- Memory ranking
- Memory decay
- Feed ranking
- Probability functions
- Scheduler logic
- Event validation
- DTO mapping

### Example Unit Tests

```csharp
public class UtilityScoringTests
{
    [Fact]
    public void CalculatePostUtility_HighExtroversion_IncreasesUtility()
    {
        // Arrange
        var npc = CreateNpcWithTraits(extroversion: 0.9);
        var topic = CreateTopic("gaming");
        
        // Act
        var utility = _scoringService.CalculatePostUtility(npc, topic);
        
        // Assert
        Assert.True(utility > 0.5);
    }
    
    [Fact]
    public void CalculatePostUtility_LowActivityLevel_DecreasesUtility()
    {
        // Arrange
        var npc = CreateNpcWithTraits(
            extroversion: 0.5,
            activityLevel: 0.1);
        
        // Act
        var utility = _scoringService.CalculatePostUtility(npc, topic);
        
        // Assert
        Assert.True(utility < 0.5);
    }
}

public class RelationshipCalculationTests
{
    [Fact]
    public void UpdateRelationship_GradualChange_NoInstantJump()
    {
        // Arrange
        var relationship = CreateRelationship(initialTrust: 0.5);
        var change = new RelationshipChange
        {
            Dimension = "trust",
            Amount = 0.5, // Large change
            IsMajorEvent = false
        };
        
        // Act
        _service.UpdateRelationship(relationship, change);
        
        // Assert
        Assert.True(relationship.Trust < 0.8); // Capped at 0.3 increase
    }
    
    [Fact]
    public void UpdateRelationship_MajorEvent_CanHaveLargerJump()
    {
        // Arrange
        var relationship = CreateRelationship(initialTrust: 0.5);
        var change = new RelationshipChange
        {
            Dimension = "trust",
            Amount = 0.5,
            IsMajorEvent = true
        };
        
        // Act
        _service.UpdateRelationship(relationship, change);
        
        // Assert
        Assert.True(relationship.Trust > 0.6); // Less capped
    }
}

public class MemoryDecayTests
{
    [Fact]
    public void ProcessDecay_LowImportance_MemoryDecays()
    {
        // Arrange
        var memory = CreateMemory(importance: 0.2, createdAt: DateTimeOffset.UtcNow.AddDays(-30));
        
        // Act
        _decayService.ProcessDecay(memory);
        
        // Assert
        Assert.True(memory.Importance < 0.2);
    }
    
    [Fact]
    public void ProcessDecay_HighImportance_MemorySurvives()
    {
        // Arrange
        var memory = CreateMemory(importance: 0.8, createdAt: DateTimeOffset.UtcNow.AddDays(-30));
        
        // Act
        _decayService.ProcessDecay(memory);
        
        // Assert
        Assert.True(memory.Importance >= 0.7);
    }
}

public class FeedRankingTests
{
    [Fact]
    public void CalculateScore_RecentPost_HigherScore()
    {
        // Arrange
        var recentPost = CreatePost(createdAt: DateTimeOffset.UtcNow.AddMinutes(-10));
        var oldPost = CreatePost(createdAt: DateTimeOffset.UtcNow.AddHours(-24));
        
        // Act
        var recentScore = _rankingService.CalculateScore(recentPost, player);
        var oldScore = _rankingService.CalculateScore(oldPost, player);
        
        // Assert
        Assert.True(recentScore > oldScore);
    }
    
    [Fact]
    public void CalculateScore_PositiveRelationship_HigherScore()
    {
        // Arrange
        var post = CreatePost(authorId: friendId);
        var player = CreatePlayerWithRelationship(friendId, affinity: 0.8);
        
        // Act
        var score = _rankingService.CalculateScore(post, player);
        
        // Assert
        Assert.True(score > 0.5);
    }
}
```

---

## Integration Tests

### Scope
- SQLite persistence
- Repository layer
- API endpoints
- WebSocket connections
- AI provider integration
- Simulation + database
- Full domain flows

### Example Integration Tests

```csharp
public class SQLitePersistenceTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task SaveNPC_Retrievable()
    {
        // Arrange
        var npc = CreateTestNpc();
        
        // Act
        await _repository.SaveAsync(npc);
        var retrieved = await _repository.GetByIdAsync(npc.Id);
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(npc.Handle, retrieved.Handle);
    }
    
    [Fact]
    public async Task UpdateRelationship_Persists()
    {
        // Arrange
        var npc1 = await CreateAndSaveNpc();
        var npc2 = await CreateAndSaveNpc();
        
        // Act
        await _relationshipRepository.CreateAsync(npc1.Id, npc2.Id);
        var relationship = await _relationshipRepository.GetAsync(npc1.Id, npc2.Id);
        relationship.Trust = 0.8;
        await _relationshipRepository.UpdateAsync(relationship);
        
        var retrieved = await _relationshipRepository.GetAsync(npc1.Id, npc2.Id);
        
        // Assert
        Assert.Equal(0.8, retrieved.Trust);
    }
}

public class APIIntegrationTests : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task CreatePost_ReturnsCreated()
    {
        // Arrange
        var request = new CreatePostRequest { Content = "Test post" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/posts", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PostResponse>();
        Assert.NotNull(result.Id);
    }
    
    [Fact]
    public async Task LikePost_UpdatesCount()
    {
        // Arrange
        var post = await CreateTestPost();
        var initialLikeCount = post.LikeCount;
        
        // Act
        await _client.PostAsync($"/api/posts/{post.Id}/like", null);
        
        // Assert
        var updated = await _client.GetFromJsonAsync<PostResponse>($"/api/posts/{post.Id}");
        Assert.Equal(initialLikeCount + 1, updated.LikeCount);
    }
}

public class SimulationPersistenceTests : IClassFixture<SimulationFixture>
{
    [Fact]
    public async Task SimulationState_SurvivesRestart()
    {
        // Arrange
        await _simulator.AddNPCs(10);
        await _simulator.AdvanceTime(TimeSpan.FromHours(1));
        
        var worldTimeBefore = _simulator.WorldClock.CurrentTime;
        var npcStatesBefore = await GetNPCCheckpoints();
        
        // Act - Restart
        await _simulator.RestartAsync();
        
        // Assert
        var worldTimeAfter = _simulator.WorldClock.CurrentTime;
        Assert.True(worldTimeAfter > worldTimeBefore); // Time continued
        
        var npcStatesAfter = await GetNPCCheckpoints();
        Assert.Equal(npcStatesBefore.Count, npcStatesAfter.Count);
    }
}
```

---

## Load Tests

### Scope
- NPC population scaling
- Post volume
- Comment volume
- Social graph operations
- Simulation rate
- Database throughput
- API concurrency

### Example Load Test

```csharp
public class LoadTests
{
    [Fact]
    public async Task Handle_100NPCs_SimulationStable()
    {
        // Arrange
        await _simulator.SpawnNPCs(100);
        
        // Act - Run simulation for 1 hour
        var startTime = DateTimeOffset.UtcNow;
        await _simulator.AdvanceTime(TimeSpan.FromHours(1));
        
        // Assert
        var metrics = _metricsService.GetSimulationMetrics();
        Assert.True(metrics.AverageTickTime < 200);
        Assert.True(metrics.ActiveNpcCount >= 90);
    }
    
    [Fact]
    public async Task Handle_ConcurrentRequests_100RPS()
    {
        // Arrange
        var requests = Enumerable.Range(0, 100)
            .Select(_ => _client.GetAsync("/api/feed"))
            .ToList();
        
        // Act
        var results = await Task.WhenAll(requests);
        
        // Assert
        Assert.All(results, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
    }
}
```

---

## End-to-End Tests

### Scope
- Full user flows
- Android → API → Server → Database → NPC → AI → Response → WebSocket → Android

### Example E2E Flow
```
1. Player opens app
2. Feed loads
3. Player likes post
4. Server validates and persists
5. NPC notices engagement
6. NPC mood updates
7. WebSocket pushes update
8. UI updates incrementally
```

---

## Chaos / Failure Tests

### Failure Scenarios

```csharp
public class ChaosTests
{
    [Fact]
    public async Task OllamaDown_SimulationContinues()
    {
        // Arrange
        await _simulator.StartAsync();
        await _ollama.StopAsync();
        
        // Act - NPCs should still act deterministically
        await _simulator.AdvanceTime(TimeSpan.FromMinutes(10));
        
        // Assert
        var metrics = _simulator.GetActivityMetrics();
        Assert.True(metrics.ActionsExecuted > 0);
        Assert.True(metrics.AiRequestsFailed == 0);
    }
    
    [Fact]
    public async Task MalformedLLMOutput_FallbackUsed()
    {
        // Arrange
        _ollama.SetResponseMode("malformed");
        
        // Act
        var result = await _aiService.GenerateAsync(request);
        
        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.FallbackUsed);
    }
    
    [Fact]
    public async Task DatabaseBusy_RetryWithBackoff()
    {
        // Arrange
        _database.SetBusyMode(true);
        
        // Act
        var result = await _repository.SaveAsync(entity);
        
        // Assert
        Assert.True(result.Success);
        Assert.True(_metrics.BusyRetryCount > 0);
    }
    
    [Fact]
    public async Task ServerRestart_WorldRestored()
    {
        // Arrange
        await _simulator.CreateWorld();
        await _simulator.AddNPCs(10);
        var checkpoint = _simulator.CreateCheckpoint();
        
        // Act
        await _server.RestartAsync();
        
        // Assert
        var restoredWorld = await _worldRepository.LoadAsync();
        Assert.Equal(checkpoint.WorldTime, restoredWorld.CurrentTime);
    }
}
```

---

## Regression Tests

### Pre-Milestone Checklist
- [ ] Build succeeds
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Simulation smoke tests pass
- [ ] Persistence tests pass
- [ ] AI validation tests pass
- [ ] Android smoke test passes
- [ ] Performance within targets

---

## Test Organization

```
tests/
├── Backend.UnitTests/
│   ├── Domain/
│   │   ├── Entities/
│   │   └── ValueObjects/
│   ├── Services/
│   │   ├── Simulation/
│   │   ├── Relationships/
│   │   ├── Memory/
│   │   └── Feed/
│   └── Utilities/
│
├── Backend.IntegrationTests/
│   ├── Database/
│   ├── API/
│   ├── WebSocket/
│   ├── AI/
│   └── Simulation/
│
├── LoadTests/
│   ├── Simulation/
│   ├── API/
│   └── Database/
│
├── ChaosTests/
│   ├── OllamaFailure/
│   ├── DatabaseFailure/
│   └── NetworkFailure/
│
└── AndroidTests/
    ├── Unit/
    ├── Integration/
    └── UI/
```

---

## Continuous Testing

### CI Pipeline
```yaml
test:
  unit:
    script:
      - dotnet test tests/Backend.UnitTests --verbosity normal
    stage: test
    
  integration:
    script:
      - dotnet test tests/Backend.IntegrationTests --verbosity normal
    stage: test
    needs: [unit]
    
  load:
    script:
      - dotnet test tests/LoadTests --verbosity minimal
    stage: test
    only: [main]
    
  android:
    script:
      - ./scripts/build-android.sh
      - ./scripts/run-android-smoke-test.sh
    stage: test
    needs: [integration]
```

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [PERFORMANCE.md](./PERFORMANCE.md) - Performance targets
