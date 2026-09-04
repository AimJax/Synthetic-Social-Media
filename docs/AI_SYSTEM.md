# AI System Architecture

## Synthetic Social World - Ollama Integration and LLM Orchestration

---

## Core Principles

1. **LLM Never Authoritative**: The LLM is an expression engine, not a simulation engine
2. **Abstraction Layer**: Simulation code must not depend directly on Ollama
3. **Centralized Queue**: All AI inference passes through one orchestration layer
4. **Graceful Degradation**: LLM failure reduces richness but never halts simulation
5. **Budget Enforcement**: Token limits, concurrency limits, and rate limiting are enforced

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    SIMULATION ENGINE                             │
│  (NPC behavior, social actions, event processing)               │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    AI ORCHESTRATION LAYER                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │  AI Queue   │  │  Priority   │  │    Context Builder     │  │
│  │   Manager   │  │   System    │  │  (compact, ~512 tokens) │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │   Output    │  │   Retry     │  │       Metrics           │  │
│  │  Validator  │  │   Policy    │  │     Collector           │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    AI PROVIDER ABSTRACTION                       │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │                    IAIProvider                           │    │
│  │  Task<AIResponse> GenerateAsync(AIRequest request)      │    │
│  └─────────────────────────────────────────────────────────┘    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐           │
│  │   Ollama    │  │    Local    │  │   Remote    │           │
│  │  Provider   │  │  Provider   │  │  Provider   │           │
│  └─────────────┘  └─────────────┘  └─────────────┘           │
│                        ┌─────────────┐                          │
│                        │    Mock     │                          │
│                        │  Provider   │                          │
│                        └─────────────┘                          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      OLLAMA RUNTIME                             │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Qwen3-4B-Instruct-2507 (or derivative)     │    │
│  └─────────────────────────────────────────────────────────┘    │
│  localhost:11434                                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## AI Provider Interface

```csharp
public interface IAIProvider
{
    string ModelName { get; }
    Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default);
    Task<bool> IsAvailableAsync();
    AIProviderMetrics GetMetrics();
}
```

### Ollama Implementation
```csharp
public class OllamaAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;
    
    public async Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken ct)
    {
        var payload = new
        {
            model = _model,
            prompt = request.Prompt,
            system = request.SystemPrompt,
            options = new
            {
                temperature = request.Temperature,
                num_predict = request.MaxTokens,
                stop = request.StopTokens
            },
            stream = false
        };
        
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/generate", payload, ct);
        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(ct);
        
        return new AIResponse
        {
            Text = result.response,
            TokensGenerated = result.eval_count,
            Duration = result.eval_duration,
            Model = _model
        };
    }
}
```

---

## AI Request Structure

```csharp
public class AIRequest
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public string Prompt { get; set; }
    public string SystemPrompt { get; set; }
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 150;
    public List<string> StopTokens { get; set; }
    public string PromptVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class AIResponse
{
    public string Text { get; set; }
    public int TokensGenerated { get; set; }
    public long DurationMs { get; set; }
    public string Model { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
}
```

---

## AI Job Structure

```csharp
public class AIJob
{
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public AIJobPriority Priority { get; set; }
    public AIRequest Request { get; set; }
    public AIJobType Type { get; set; }
    public string TargetNpcId { get; set; }
    public string TargetEntityId { get; set; }
    public DateTimeOffset QueuedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int RetryCount { get; set; }
    public AIJobStatus Status { get; set; }
}

public enum AIJobPriority
{
    DirectPlayerInteraction = 100,
    PlayerDM = 95,
    PlayerReply = 90,
    MajorRelationshipEvent = 80,
    MajorPublicDrama = 70,
    ImportantNPCConversation = 60,
    MeaningfulBackgroundContent = 40,
    OrdinaryNPCContent = 20,
    TrivialBackgroundChatter = 10
}

public enum AIJobType
{
    PostGeneration,
    CommentGeneration,
    ReplyGeneration,
    DMGeneration,
    ArgumentGeneration,
    ConversationGeneration,
    EventDescription,
    CatchUpSummary
}
```

---

## AI Queue Manager

```csharp
public class AIQueueManager
{
    private readonly Channel<AIJob> _highPriorityQueue;
    private readonly Channel<AIJob> _lowPriorityQueue;
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly int _maxConcurrentJobs;
    private readonly TimeSpan _timeout;
    
    public async Task<AIJob> EnqueueAsync(AIRequest request, AIJobPriority priority, AIJobType type)
    {
        var job = new AIJob
        {
            Request = request,
            Priority = priority,
            Type = type,
            QueuedAt = DateTimeOffset.UtcNow
        };
        
        if (priority >= AIJobPriority.ImportantNPCConversation)
            await _highPriorityQueue.Writer.WriteAsync(job);
        else
            await _lowPriorityQueue.Writer.WriteAsync(job);
        
        return job;
    }
    
    public async Task<AIJob> DequeueAsync(CancellationToken ct)
    {
        // Always check high priority first
        if (_highPriorityQueue.Reader.TryRead(out var highJob))
            return highJob;
        
        // Then check low priority with timeout
        if (await _lowPriorityQueue.Reader.WaitToReadAsync(ct))
        {
            _lowPriorityQueue.Reader.TryRead(out var lowJob);
            return lowJob;
        }
        
        return null;
    }
}
```

---

## Context Builder

### Purpose
Build compact, relevant context for LLM prompts.

### Principles
- Memory budget: ~512 tokens (default)
- Retrieve based on: target person, current topic, event type, emotional relevance, recency, importance, relationship
- Include only: compact system instructions, current NPC state, immediate interaction, recent conversation

### Example Context

```
SYSTEM: You are Sarah, a 25-year-old software developer. Keep responses short and casual.

NPC STATE:
- Mood: Annoyed (primary), Anger 0.6
- Personality: Aggression 0.61, Sarcasm 0.82, Humor 0.75
- Current goal: Gain influence in gaming community

RELATIONSHIP WITH ALEX:
- Hostility: 0.81
- Trust: -0.70
- Last interaction: Alex insulted you yesterday

RELEVANT MEMORIES (2):
1. Alex mocked your gaming community (Importance: 0.7)
2. Alex helped you fix code last week (Importance: 0.4)

CURRENT SITUATION:
Alex posted: "Some people shouldn't run communities lol."

TASK: Write a short reply (under 100 tokens).
```

---

## Output Validation

### Validation Rules
1. **Maximum length**: Must not exceed configured token limit
2. **Non-empty**: Must contain actual text
3. **No malformed text**: Must be valid UTF-8, no control characters
4. **No speaker labels**: No "NPC:" prefixes unless intended
5. **No fake dialogue turns**: No artificial conversation patterns
6. **No system text**: No accidental system prompts in output
7. **No repeated output**: Detect loops/repetition
8. **No unexpected structured data**: Unless parsing structured JSON
9. **No prompt leakage**: No revealing of internal prompts

### Validation Implementation
```csharp
public class OutputValidator
{
    private readonly int _maxLength;
    private readonly AIOutputSchema _expectedSchema;
    
    public ValidationResult Validate(string output)
    {
        var result = new ValidationResult { IsValid = true };
        
        if (string.IsNullOrWhiteSpace(output))
        {
            result.IsValid = false;
            result.Error = "Empty output";
            return result;
        }
        
        if (output.Length > _maxLength)
        {
            result.IsValid = false;
            result.Error = $"Output exceeds max length {_maxLength}";
            return result;
        }
        
        if (ContainsControlCharacters(output))
        {
            result.IsValid = false;
            result.Error = "Contains control characters";
            return result;
        }
        
        if (IsRepetitive(output))
        {
            result.IsValid = false;
            result.Error = "Output is repetitive";
            return result;
        }
        
        // If expecting JSON, validate JSON structure
        if (_expectedSchema != null)
        {
            var jsonResult = ValidateJson(output);
            if (!jsonResult.IsValid)
                return jsonResult;
        }
        
        return result;
    }
}
```

---

## Parser Fallback Strategy

```
1. LLM generates output
2. Parse attempt #1:
   - Parse based on expected format (JSON, text, etc.)
   - If successful and valid → return result
   
3. Parse attempt #2 (retry with compact correction):
   - Send correction prompt requesting simple format
   - If successful and valid → return result
   
4. Tier 2 Fallback (deterministic):
   - Use utility-based deterministic response
   - Log LLM failure for review
   - Continue simulation
   
5. NEVER allow malformed output to halt simulation
6. NEVER allow malformed output to corrupt persistent state
```

---

## Structured Output Schema

### Maximum 4 Root Fields

For Qwen3-4B (small model):

```json
{
  "action": "reply",
  "tone": "hostile",
  "emotion": "annoyed",
  "text": "..."
}
```

### Schema Examples by Job Type

**Post Generation:**
```json
{
  "topic": "gaming",
  "style": "casual",
  "emotion": "excited",
  "text": "..."
}
```

**Comment Generation:**
```json
{
  "type": "agreement",
  "tone": "supportive",
  "text": "..."
}
```

**DM Generation:**
```json
{
  "intent": "flirt",
  "tone": "playful",
  "text": "..."
}
```

---

## Token Budgets

| Content Type | Target Tokens | Max Tokens |
|-------------|--------------|------------|
| Short comment | ~40 | 60 |
| Reply | ~60-100 | 120 |
| Normal post | ~80-150 | 200 |
| DM | ~150-300 | 400 |
| Catch-up summary | ~200-400 | 500 |

*These are starting values. Benchmark and adjust.*

---

## Prompt Versioning

```csharp
public class PromptTemplate
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Template { get; set; }
    public string ModelId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Description { get; set; }
}
```

Usage:
```csharp
public class PromptRegistry
{
    private readonly Dictionary<string, PromptTemplate> _templates;
    
    public PromptTemplate Get(string name, string version = "latest")
    {
        var key = $"{name}:{version}";
        return _templates.GetValueOrDefault(key);
    }
    
    public void Register(PromptTemplate template)
    {
        var key = $"{template.Name}:{template.Version}";
        _templates[key] = template;
    }
}
```

---

## Metrics Collection

```csharp
public class AIMetrics
{
    public int QueueLength { get; set; }
    public int ActiveJobs { get; set; }
    public double AverageLatencyMs { get; set; }
    public double TokensPerSecond { get; set; }
    public int TotalRequests { get; set; }
    public int FailedRequests { get; set; }
    public double FailureRate { get; set; }
    public int Retries { get; set; }
    public DateTimeOffset LastSuccessfulRequest { get; set; }
}
```

---

## Configuration

```json
{
  "AI": {
    "Provider": "ollama",
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "Model": "qwen3:4b",
      "TimeoutSeconds": 30
    },
    "Queue": {
      "MaxConcurrentJobs": 2,
      "HighPriorityWorkers": 2,
      "LowPriorityWorkers": 1,
      "MaxQueueSize": 1000
    },
    "Budgets": {
      "MaxTokensPost": 200,
      "MaxTokensComment": 120,
      "MaxTokensDM": 400,
      "MaxTokensSummary": 500,
      "MemoryBudgetTokens": 512
    },
    "Retry": {
      "MaxRetries": 1,
      "BaseDelayMs": 500,
      "MaxDelayMs": 2000
    },
    "Fallback": {
      "UseTemplates": true,
      "UsePhrasePools": true
    }
  }
}
```

---

## Stop Conditions

**IMPORTANT**: Do NOT use social media-sensitive characters as stop tokens.

Bad:
```csharp
StopTokens = ["@"]  // Unacceptable - @mentions are common
StopTokens = ["\n\n"]  // Unacceptable - newlines are natural
```

Good:
```csharp
StopTokens = []  // Use bounded tokens instead
```

Alternative approach:
- Use bounded output tokens
- Use concise prompts
- Use structured output
- Server-side validation
- Explicit output requirements

---

## Related Documents

- [ARCHITECTURE.md](./ARCHITECTURE.md) - System overview
- [SIMULATION.md](./SIMULATION.md) - NPC behavior system
- [MEMORY_SYSTEM.md](./MEMORY_SYSTEM.md) - Memory architecture
