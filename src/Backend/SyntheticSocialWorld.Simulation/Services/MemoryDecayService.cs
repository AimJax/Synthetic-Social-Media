namespace SyntheticSocialWorld.Simulation.Services;

/// <summary>
/// Implements memory decay - NPCs forget information over time
/// Based on SOCIAL_GRAPH.md Section 6
/// </summary>
public class MemoryDecayService
{
    private readonly Random _random = new();
    
    // Decay rates (per day)
    private const double SurfaceMemoryDecay = 0.05; // 5% per day
    private const double EmotionalMemoryDecay = 0.02; // 2% per day (slower)
    private const double InteractionMemoryDecay = 0.03; // 3% per day
    
    // Thresholds
    private const double ForgettingThreshold = 0.1; // Below this, memory is essentially gone
    private const double ConsolidationThreshold = 0.8; // Above this, memory is consolidated
    
    /// <summary>
    /// Calculate decay factor for a memory based on type and emotional weight
    /// </summary>
    public double CalculateDecay(
        MemoryType memoryType,
        double emotionalWeight,
        int daysSince)
    {
        var baseDecay = memoryType switch
        {
            MemoryType.Surface => SurfaceMemoryDecay,
            MemoryType.Emotional => EmotionalMemoryDecay,
            MemoryType.Interaction => InteractionMemoryDecay,
            MemoryType.Episodic => SurfaceMemoryDecay * 0.8,
            MemoryType.Semantic => EmotionalMemoryDecay * 0.5, // Slower decay
            _ => SurfaceMemoryDecay
        };
        
        // Emotional memories decay slower
        if (emotionalWeight > 0.7)
        {
            baseDecay *= 0.6; // 40% slower for high emotion
        }
        else if (emotionalWeight > 0.4)
        {
            baseDecay *= 0.8; // 20% slower for medium emotion
        }
        
        // Linear decay per day
        return Math.Pow(1.0 - baseDecay, daysSince);
    }
    
    /// <summary>
    /// Process memory consolidation - strong memories become permanent
    /// </summary>
    public bool ShouldConsolidate(double currentStrength, double emotionalWeight)
    {
        // High emotional weight + strong recall = consolidation
        if (currentStrength > ConsolidationThreshold && emotionalWeight > 0.5)
        {
            return true;
        }
        
        // Repeated recall increases consolidation chance
        return currentStrength > 0.95;
    }
    
    /// <summary>
    /// Calculate memory retrieval strength (chance of remembering)
    /// </summary>
    public double CalculateRetrievalStrength(
        double currentStrength,
        double relevanceToCurrentContext,
        double familiarity)
    {
        // Base retrieval chance
        var retrieval = currentStrength;
        
        // Context relevance helps retrieval
        retrieval *= (0.5 + relevanceToCurrentContext * 0.5);
        
        // Familiarity helps retrieval
        retrieval *= (0.7 + familiarity * 0.3);
        
        // Random factor for memory retrieval
        retrieval *= (0.8 + _random.NextDouble() * 0.4);
        
        return Math.Clamp(retrieval, 0, 1);
    }
    
    /// <summary>
    /// Get list of memories that should be forgotten
    /// </summary>
    public List<string> GetMemoriesToForget(IEnumerable<MemoryInfo> memories)
    {
        var toForget = new List<string>();
        
        foreach (var memory in memories)
        {
            if (memory.Strength < ForgettingThreshold)
            {
                toForget.Add(memory.Id);
            }
        }
        
        return toForget;
    }
    
    /// <summary>
    /// Process reactivation of a memory (strengthens it)
    /// </summary>
    public double ProcessReactivation(double currentStrength, double emotionalWeight)
    {
        // Reactivation strengthens memory
        var boost = 0.1 + emotionalWeight * 0.1;
        
        // More emotional memories strengthen more on recall
        return Math.Min(1.0, currentStrength + boost);
    }
    
    /// <summary>
    /// Calculate importance decay based on usage patterns
    /// </summary>
    public double CalculateImportanceDecay(
        double currentImportance,
        int daysSinceUsed,
        int usageCount)
    {
        // Base decay
        var decay = currentImportance * Math.Pow(0.95, daysSinceUsed);
        
        // Frequent users maintain importance longer
        var usageBonus = Math.Min(0.1, usageCount * 0.01);
        
        return Math.Max(0, decay + usageBonus);
    }
    
    /// <summary>
    /// Determine if an NPC would remember a specific interaction
    /// </summary>
    public bool WouldRemember(
        double memoryStrength,
        double emotionalWeight,
        double relevanceToCurrentContext,
        double familiarity)
    {
        var retrievalStrength = CalculateRetrievalStrength(
            memoryStrength, 
            relevanceToCurrentContext,
            familiarity);
        
        // ~60% chance of retrieval for memories at threshold
        return _random.NextDouble() < retrievalStrength;
    }
    
    /// <summary>
    /// Get decay rate description for debugging/logging
    /// </summary>
    public string GetDecayDescription(MemoryType memoryType)
    {
        return memoryType switch
        {
            MemoryType.Surface => "Surface memories decay at 5%/day",
            MemoryType.Emotional => "Emotional memories decay at 2%/day",
            MemoryType.Interaction => "Interaction memories decay at 3%/day",
            MemoryType.Episodic => "Episodic memories decay at 4%/day",
            MemoryType.Semantic => "Semantic memories decay at 1%/day (slowest)",
            _ => "Unknown memory type"
        };
    }
    
    /// <summary>
    /// Calculate how many days until a memory is effectively forgotten
    /// </summary>
    public int DaysUntilForgotten(double initialStrength, double emotionalWeight, MemoryType memoryType)
    {
        var decayRate = memoryType switch
        {
            MemoryType.Surface => SurfaceMemoryDecay,
            MemoryType.Emotional => EmotionalMemoryDecay,
            MemoryType.Interaction => InteractionMemoryDecay,
            _ => SurfaceMemoryDecay
        };
        
        // Adjust for emotional weight
        if (emotionalWeight > 0.7)
            decayRate *= 0.6;
        else if (emotionalWeight > 0.4)
            decayRate *= 0.8;
        
        // Solve for days: initial * (1 - rate)^days = threshold
        // days = log(threshold/initial) / log(1 - rate)
        if (decayRate >= 1.0) return 1;
        
        var days = Math.Log(ForgettingThreshold / initialStrength) / Math.Log(1.0 - decayRate);
        return (int)Math.Max(1, Math.Ceiling(days));
    }
}

/// <summary>
/// Types of memories with different decay rates
/// </summary>
public enum MemoryType
{
    Surface,      // Basic facts, quick to forget
    Emotional,    // Emotionally charged memories, last longer
    Interaction,  // Details of specific interactions
    Episodic,     // Event memories
    Semantic      // Deep knowledge, very slow decay
}

/// <summary>
/// Information about a memory
/// </summary>
public class MemoryInfo
{
    public string Id { get; set; } = "";
    public MemoryType Type { get; set; }
    public double Strength { get; set; }
    public double EmotionalWeight { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int RecallCount { get; set; }
}
