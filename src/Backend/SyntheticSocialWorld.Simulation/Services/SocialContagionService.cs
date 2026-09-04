using SyntheticSocialWorld.Domain.Entities;

namespace SyntheticSocialWorld.Simulation.Services;

/// <summary>
/// Implements social contagion - moods, opinions, and behaviors spreading through the social network
/// Based on SOCIAL_GRAPH.md Section 8
/// </summary>
public class SocialContagionService
{
    private readonly Random _random = new();
    
    // Contagion rates per hour
    private const double MoodContagionRate = 0.05; // 5% chance per interaction
    private const double OpinionContagionRate = 0.03; // 3% chance per interaction
    private const double BehaviorContagionRate = 0.02; // 2% chance per interaction
    
    // Maximum influence per event (prevents instant shifts)
    private const double MaxMoodInfluence = 0.1;
    private const double MaxOpinionInfluence = 0.05;
    
    /// <summary>
    /// Process social contagion for an NPC after an interaction
    /// Called when two NPCs interact (comment, like, follow, message)
    /// </summary>
    public ContagionResult ProcessInteraction(
        string actorId,
        string targetId,
        double actorHappiness,
        double targetHappiness,
        NPCRelationship relationship,
        InteractionType interactionType)
    {
        var result = new ContagionResult();
        
        // Calculate relationship strength for contagion probability
        var relationshipStrength = CalculateRelationshipStrength(relationship);
        
        // Mood contagion - emotions spread
        if (_random.NextDouble() < MoodContagionRate * relationshipStrength)
        {
            var moodInfluence = CalculateMoodInfluence(actorHappiness, targetHappiness, relationship);
            result.MoodChanges = moodInfluence;
        }
        
        // Opinion contagion - beliefs spread through trusted sources
        if (relationship.Trust > 0.3 && _random.NextDouble() < OpinionContagionRate * relationship.Trust)
        {
            var opinionInfluence = CalculateOpinionInfluence(actorId, targetId, relationship);
            result.OpinionChanges = opinionInfluence;
        }
        
        // Behavior contagion - imitate others
        if (relationship.Affinity > 0.3 && _random.NextDouble() < BehaviorContagionRate * relationship.Affinity)
        {
            result.BehaviorAdoption = DetermineBehaviorAdoption(actorId, targetId, interactionType);
        }
        
        return result;
    }
    
    /// <summary>
    /// Calculate how strong the relationship is for contagion probability
    /// </summary>
    private double CalculateRelationshipStrength(NPCRelationship relationship)
    {
        // Familiarity increases contagion probability
        var familiarityBonus = relationship.Familiarity * 0.2;
        
        // Positive relationships spread more
        var positiveBonus = Math.Max(0, relationship.Affinity + relationship.Trust) * 0.3;
        
        // Negative relationships reduce contagion (but not completely)
        var negativePenalty = Math.Max(0, relationship.Hostility + relationship.Jealousy) * 0.1;
        
        return Math.Clamp(0.5 + familiarityBonus + positiveBonus - negativePenalty, 0.1, 1.0);
    }
    
    /// <summary>
    /// Calculate how much one NPC's mood affects another
    /// </summary>
    private Dictionary<string, double> CalculateMoodInfluence(double actorHappiness, double targetHappiness, NPCRelationship relationship)
    {
        var changes = new Dictionary<string, double>();
        
        // Empathy determines how much others' moods affect you
        var empathy = 0.5; // Placeholder - would be from personality
        
        // High empathy + high affinity = catch the mood
        var contagionStrength = empathy * (0.5 + relationship.Affinity);
        
        // Happy moods spread easier than sad moods
        if (actorHappiness > 0.5)
        {
            contagionStrength *= 1.2; // Good moods spread 20% faster
        }
        else
        {
            contagionStrength *= 0.8; // Bad moods spread 20% slower
        }
        
        var influence = Math.Min(contagionStrength * MaxMoodInfluence, MaxMoodInfluence);
        
        if (actorHappiness > targetHappiness)
        {
            changes["Happiness"] = influence;
            changes["Sadness"] = -influence * 0.5;
        }
        else
        {
            changes["Sadness"] = influence;
            changes["Happiness"] = -influence * 0.3;
        }
        
        return changes;
    }
    
    /// <summary>
    /// Calculate how beliefs spread between NPCs
    /// </summary>
    private List<OpinionChange> CalculateOpinionInfluence(string actorId, string targetId, NPCRelationship relationship)
    {
        var changes = new List<OpinionChange>();
        
        // Only trusted sources influence beliefs
        if (relationship.Trust < 0.3) return changes;
        
        // High respect + trust = belief adoption likely
        var adoptionChance = relationship.Trust * 0.5 + relationship.Respect * 0.3;
        
        if (_random.NextDouble() < adoptionChance)
        {
            changes.Add(new OpinionChange
            {
                Topic = "general",
                NewConfidence = relationship.Trust * MaxOpinionInfluence,
                Source = actorId
            });
        }
        
        return changes;
    }
    
    /// <summary>
    /// Determine if target adopts a behavior from actor
    /// </summary>
    private string? DetermineBehaviorAdoption(string actorId, string targetId, InteractionType interaction)
    {
        // Follow behavior adoption
        if (interaction == InteractionType.Follow && _random.NextDouble() < 0.3)
        {
            return "reciprocal_follow";
        }
        
        return null;
    }
    
    /// <summary>
    /// Process ambient contagion - gradual mood changes based on network
    /// Called periodically for active NPCs
    /// </summary>
    public Dictionary<string, double> ProcessAmbientContagion(
        IEnumerable<(string NpcId, double Happiness)> nearbyNPCMoods)
    {
        var aggregateChanges = new Dictionary<string, double>();
        
        foreach (var mood in nearbyNPCMoods)
        {
            // Weight by happiness
            var weight = mood.Happiness;
            
            if (!aggregateChanges.ContainsKey("Happiness"))
                aggregateChanges["Happiness"] = 0;
            aggregateChanges["Happiness"] += (mood.Happiness - 0.5) * weight * 0.01;
        }
        
        return aggregateChanges;
    }
}

/// <summary>
/// Result of contagion processing
/// </summary>
public class ContagionResult
{
    public Dictionary<string, double> MoodChanges { get; set; } = new();
    public List<OpinionChange> OpinionChanges { get; set; } = new();
    public string? BehaviorAdoption { get; set; }
}

/// <summary>
/// Represents an opinion/belief change
/// </summary>
public class OpinionChange
{
    public string Topic { get; set; } = "";
    public double NewConfidence { get; set; }
    public string Source { get; set; } = "";
}

/// <summary>
/// Types of interactions that can spread contagion
/// </summary>
public enum InteractionType
{
    Comment,
    Like,
    Follow,
    Message,
    Mention,
    Share
}

/// <summary>
/// Extension methods for NPC mood access
/// </summary>
public static class NPCMoodExtensions
{
    public static double GetMoodHappiness(this NPC npc)
    {
        // This would access the actual mood from the NPC's loaded mood data
        // For now, return a default that can be replaced with actual mood lookup
        return 0.5;
    }
}
