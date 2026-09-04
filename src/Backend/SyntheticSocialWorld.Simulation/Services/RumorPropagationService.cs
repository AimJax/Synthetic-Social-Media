using SyntheticSocialWorld.Domain.Entities;

namespace SyntheticSocialWorld.Simulation.Services;

/// <summary>
/// Implements rumor propagation - information spreading through the social network
/// Based on SOCIAL_GRAPH.md Section 7
/// </summary>
public class RumorPropagationService
{
    private readonly Random _random = new();
    
    // Rumor spread probability per hour
    private const double BaseSpreadProbability = 0.15; // 15% base chance
    
    // Rumor decay
    private const double RumorDecayRate = 0.02; // 2% confidence loss per day
    
    // Maximum spread hops (6 degrees of separation)
    private const int MaxSpreadHops = 6;
    
    /// <summary>
    /// Process rumor spread to neighbors
    /// </summary>
    public RumorSpreadResult ProcessSpread(
        Rumor rumor,
        IEnumerable<string> neighborIds,
        IEnumerable<NPCRelationship> relationships)
    {
        var result = new RumorSpreadResult
        {
            RumorId = rumor.Id,
            SpreadTo = new List<string>()
        };
        
        var relationshipDict = relationships.ToDictionary(r => r.TargetNpcId);
        
        foreach (var neighborId in neighborIds)
        {
            // Calculate spread probability
            var spreadProbability = CalculateSpreadProbability(rumor, neighborId, relationshipDict);
            
            if (_random.NextDouble() < spreadProbability)
            {
                result.SpreadTo.Add(neighborId);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Calculate probability that an NPC will spread a rumor
    /// </summary>
    private double CalculateSpreadProbability(
        Rumor rumor,
        string targetId,
        Dictionary<string, NPCRelationship> relationships)
    {
        double probability = BaseSpreadProbability;
        
        if (relationships.TryGetValue(targetId, out var relationship))
        {
            // High familiarity increases spread chance
            probability *= (0.5 + relationship.Familiarity * 0.5);
            
            // Trust increases spread
            if (relationship.Trust > 0.5)
            {
                probability *= 1.2;
            }
            
            // Hostility reduces spread to target
            if (relationship.Hostility > 0.3)
            {
                probability *= 0.7;
            }
            
            // Jealousy increases gossip spread
            if (relationship.Jealousy > 0.3)
            {
                probability *= 1.3;
            }
        }
        
        // Confidence affects spread
        probability *= rumor.Confidence;
        
        return Math.Clamp(probability, 0.0, 1.0);
    }
    
    /// <summary>
    /// Calculate how rumor confidence changes over time
    /// </summary>
    public double CalculateDecay(double currentConfidence, double daysSinceUpdate)
    {
        // Exponential decay
        return currentConfidence * Math.Exp(-RumorDecayRate * daysSinceUpdate);
    }
    
    /// <summary>
    /// Determine if a rumor should escalate to a belief
    /// </summary>
    public bool ShouldEscalateToBelief(Rumor rumor)
    {
        // High confidence + multiple sources = becomes belief
        return rumor.Confidence > 0.7 && rumor.SpreadCount > 5;
    }
    
    /// <summary>
    /// Get rumors about a specific NPC
    /// </summary>
    public IEnumerable<Rumor> GetRumorsAbout(
        IEnumerable<Rumor> allRumors,
        string npcId)
    {
        return allRumors.Where(r => 
            r.Subject == npcId && 
            r.Confidence > 0.3);
    }
    
    /// <summary>
    /// Generate rumor description for display
    /// </summary>
    public string GenerateRumorDescription(Rumor rumor, string subjectName)
    {
        var confidenceText = rumor.Confidence switch
        {
            > 0.8 => "Apparently,",
            > 0.5 => "People are saying",
            > 0.3 => "There's a rumor that",
            _ => "I heard that"
        };
        
        return $"{confidenceText} {subjectName} {rumor.Content}";
    }
}

/// <summary>
/// Types of rumors
/// </summary>
public enum RumorType
{
    Gossip,
    Scandal,
    Achievement,
    Relationship,
    Event,
    Career
}

/// <summary>
/// Result of rumor spread processing
/// </summary>
public class RumorSpreadResult
{
    public string RumorId { get; set; } = "";
    public List<string> SpreadTo { get; set; } = new();
    public int SuccessfulSpread => SpreadTo.Count;
}
