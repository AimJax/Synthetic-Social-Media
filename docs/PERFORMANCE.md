# Performance and Benchmarking

## Synthetic Social World - Performance Targets and Measurements

---

## Performance Philosophy

> "Maximum social believability per unit of computation."

Performance is a feature. The system must be fast enough to be responsive while allowing sufficient AI and simulation work.

---

## Server-Side Performance Targets

### Response Times

| Endpoint | Target | Acceptable | Critical |
|----------|--------|------------|----------|
| Feed (first page) | < 100ms | < 200ms | < 500ms |
| Feed (paged) | < 50ms | < 100ms | < 200ms |
| Post creation | < 100ms | < 200ms | < 500ms |
| Like/Dislike | < 50ms | < 100ms | < 200ms |
| User profile | < 50ms | < 100ms | < 200ms |
| Messages list | < 100ms | < 200ms | < 500ms |
| Search | < 200ms | < 500ms | < 1000ms |
| Notifications | < 100ms | < 200ms | < 500ms |

### Throughput

| Metric | Target | Acceptable |
|--------|--------|------------|
| Concurrent players | 10 | 5 |
| Requests/second | 100 | 50 |
| AI requests/minute | 60 | 30 |
| NPCs active | 20-50 | 20 |

### Resource Usage

| Resource | Target | Warning | Critical |
|----------|--------|---------|----------|
| CPU (simulation) | < 30% | < 50% | < 80% |
| RAM (server) | < 1GB | < 2GB | < 4GB |
| GPU (Ollama) | < 50% | < 70% | < 90% |
| VRAM | < 4GB | < 6GB | < 8GB |
| SQLite latency | < 5ms | < 10ms | < 50ms |
| Simulation tick | < 100ms | < 200ms | < 500ms |
| AI queue length | < 10 | < 50 | < 100 |

---

## Android Client Performance Targets

### UI Performance

| Metric | Target | Acceptable | Critical |
|--------|--------|------------|----------|
| Frame time | < 16ms | < 33ms | < 50ms |
| Dropped frames | < 1% | < 5% | < 10% |
| Feed scroll | 60 FPS | 30 FPS | < 30 FPS |
| Startup time | < 2s | < 5s | < 10s |
| Cold start | < 3s | < 5s | < 10s |
| Input latency | < 50ms | < 100ms | < 200ms |

### Network Performance

| Metric | Target | Acceptable | Critical |
|--------|--------|------------|----------|
| API latency | < 200ms | < 500ms | < 1000ms |
| WebSocket connect | < 500ms | < 1000ms | < 2000ms |
| Data usage (session) | < 50MB | < 100MB | < 200MB |
| Offline capability | Basic | Full | - |

### Battery and Thermal

| Metric | Target | Acceptable | Critical |
|--------|--------|------------|----------|
| Battery drain | < 5%/hour | < 10%/hour | < 20%/hour |
| Thermal throttling | Never | Rare | Occasional |

---

## AI Performance Targets

### Generation Latency

| Content Type | Target | Acceptable | Critical |
|--------------|--------|------------|----------|
| Short comment (~40 tokens) | < 3s | < 5s | < 10s |
| Reply (~80 tokens) | < 5s | < 8s | < 15s |
| Normal post (~150 tokens) | < 8s | < 12s | < 20s |
| DM (~200 tokens) | < 10s | < 15s | < 25s |
| Summary (~400 tokens) | < 15s | < 25s | < 40s |

### Generation Quality

| Metric | Target | Acceptable | Critical |
|--------|--------|------------|----------|
| Tokens/second | > 15 | > 10 | > 5 |
| Failure rate | < 1% | < 5% | < 10% |
| Retry rate | < 5% | < 10% | < 20% |
| Validation pass rate | > 95% | > 85% | > 70% |

---

## Scale Targets

### Population Benchmarks

| NPCs | Status | Target FPS | AI Budget |
|------|--------|-----------|-----------|
| 20 | Initial | 60 | Full |
| 50 | Small | 60 | Full with batching |
| 100 | Medium | 30-60 | LOD applied |
| 250 | Large | 30 | Aggressive LOD |
| 500 | Very Large | 30 | Statistical sim |
| 1000 | Massive | 30 | Statistical + targeted |

### Database Scale

| Metric | Small | Medium | Large |
|--------|-------|--------|-------|
| Posts | 10,000 | 100,000 | 1,000,000 |
| Comments | 50,000 | 500,000 | 5,000,000 |
| Memories | 100,000 | 1,000,000 | 10,000,000 |
| Relationships | 1,000 | 10,000 | 100,000 |

---

## Performance Measurement Tools

### Server Metrics

```csharp
public class PerformanceMetrics
{
    public class ServerMetrics
    {
        public double CpuUsage { get; set; }
        public long MemoryUsageBytes { get; set; }
        public double GpuUtilization { get; set; }
        public long GpuMemoryBytes { get; set; }
    }
    
    public class SimulationMetrics
    {
        public int ActiveNpcCount { get; set; }
        public double TickDurationMs { get; set; }
        public int ScheduledActionCount { get; set; }
        public double SchedulerThroughput { get; set; }
    }
    
    public class DatabaseMetrics
    {
        public double AverageLatencyMs { get; set; }
        public double MaxLatencyMs { get; set; }
        public int BusyCount { get; set; }
        public int WriteQueueDepth { get; set; }
    }
    
    public class AiMetrics
    {
        public int QueueLength { get; set; }
        public double AverageLatencyMs { get; set; }
        public double TokensPerSecond { get; set; }
        public int FailureCount { get; set; }
        public double FailureRate { get; set; }
    }
}
```

### Profiling Endpoints

```
GET /api/debug/metrics          - All metrics
GET /api/debug/metrics/simulation - Simulation only
GET /api/debug/metrics/database    - Database only
GET /api/debug/metrics/ai         - AI only
GET /api/debug/metrics/npc/{id}   - Specific NPC
```

---

## Benchmark Scripts

### Simulation Benchmark
```bash
# Benchmark with 20 NPCs
dotnet run --project src/Backend/SyntheticSocialWorld.Api \
    --benchmark simulation --npc-count 20 --duration 60s

# Benchmark with 50 NPCs
dotnet run --project src/Backend/SyntheticSocialWorld.Api \
    --benchmark simulation --npc-count 50 --duration 60s
```

### AI Benchmark
```bash
# Benchmark AI generation
dotnet run --project src/Backend/SyntheticSocialWorld.Api \
    --benchmark ai --request-count 100 --content-type post
```

### Database Benchmark
```bash
# Benchmark database operations
dotnet run --project src/Backend/SyntheticSocialWorld.Api \
    --benchmark database --operations 10000
```

---

## Performance Optimization Strategies

### Server Optimizations

1. **Caching**
   - Feed cache (1 minute TTL)
   - NPC cache (hot NPCs in memory)
   - Relationship cache
   - Community cache

2. **Batching**
   - Database writes batched by 100ms
   - AI requests batched by priority
   - Notification aggregation

3. **Indexed Queries**
   - All foreign keys indexed
   - Timestamp indexes for feeds
   - Composite indexes for common patterns

4. **Background Processing**
   - AI jobs in background
   - Metric collection async
   - Cache warming on startup

### Android Optimizations

1. **Image Loading**
   - Lazy loading with Coil
   - Memory cache
   - Disk cache
   - Downsampling for thumbnails

2. **List Performance**
   - LazyColumn with keys
   - Pagination at 20 items
   - Incremental updates
   - DiffUtil for changes

3. **Network**
   - Connection pooling
   - Response caching
   - Debounced requests
   - Optimistic updates

---

## Performance Regression Prevention

### CI/CD Benchmarks
Every build runs:
- Unit tests with timing assertions
- Integration tests with timing assertions
- AI benchmark (sample size 10)
- Database benchmark (sample size 100)

### Alert Thresholds
Alert when:
- API latency > 500ms
- AI queue > 50
- Database busy > 5%
- Frame drops > 5%
- Crash rate > 1%

---

## Related Documents

- [TESTING.md](./TESTING.md) - Testing strategy
- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
