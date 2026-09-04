using SyntheticSocialWorld.Domain.Entities;

namespace SyntheticSocialWorld.Simulation.Services;

/// <summary>
/// Implements dynamic relationship evolution - relationships change over time based on interactions
/// Based on SOCIAL_GRAPH.md Section 4
/// </summary>
public class RelationshipEvolutionService
{
    private readonly Random _random = new();
    
    // Change rates per interaction
    private const double PositiveInteractionAffinityBoost = 0.02;
    private const double NegativeInteractionAffinityDrop = -0.03;
    private const double PositiveInteractionTrustBoost = 0.015;
    private const double NegativeInteractionTrustDrop = -0.05;
    
    // Decay rates (per day of no interaction)
    private const double FamiliarityDecayRate = 0.01;
    private const double AffinityDriftRate = 0.005;
    
    // Thresholds
    private const double FriendshipThreshold = 0.5; // Affinity + Trust
    private const double RivalryThreshold = -0.3; // Low affinity + high hostility
    private const double RomanceThreshold = 0.6; // High affinity + high attraction
    
    /// <summary>
    /// Process an interaction and update relationships accordingly
    /// </summary>
    public RelationshipUpdateResult ProcessInteraction(
        NPCRelationship relationship,
        InteractionOutcome outcome,
        bool wasReciprocated)
    {
        var result = new RelationshipUpdateResult
        {
            OldType = DetermineRelationshipType(relationship)
        };
        
        // Calculate base change
        var baseChange = GetBaseChangeForOutcome(outcome);
        
        // Apply personality modifiers
        baseChange = ApplyPersonalityModifiers(baseChange, relationship);
        
        // Apply reciprocity bonus
        if (wasReciprocated)
        {
            baseChange.Affinity *= 1.5;
            baseChange.Trust *= 1.3;
        }
        
        // Apply gradual change limits
        var clampedChanges = ClampChanges(baseChange);
        
        // Update relationship
        relationship.Affinity = Math.Clamp(relationship.Affinity + clampedChanges.Affinity, -1.0, 1.0);
        relationship.Trust = Math.Clamp(relationship.Trust + clampedChanges.Trust, -1.0, 1.0);
        relationship.Respect = Math.Clamp(relationship.Respect + clampedChanges.Respect, -1.0, 1.0);
        relationship.Hostility = Math.Clamp(relationship.Hostility + clampedChanges.Hostility, 0.0, 1.0);
        relationship.Familiarity = Math.Min(1.0, relationship.Familiarity + 0.05);
        relationship.UpdatedAt = DateTimeOffset.UtcNow;
        
        // Determine new relationship type
        result.Changes = clampedChanges;
        result.NewType = DetermineRelationshipType(relationship);
        
        return result;
    }
    
    /// <summary>
    /// Process time-based decay for relationships
    /// </summary>
    public RelationshipUpdateResult ProcessTimeDecay(NPCRelationship relationship, double daysSinceInteraction)
    {
        var result = new RelationshipUpdateResult();
        
        if (daysSinceInteraction < 1) return result;
        
        // Familiarity decays over time
        var familiarityDecay = FamiliarityDecayRate * daysSinceInteraction;
        relationship.Familiarity = Math.Max(0, relationship.Familiarity - familiarityDecay);
        
        // Affinity drifts toward neutral over time
        if (Math.Abs(relationship.Affinity) > 0.2)
        {
            var drift = Math.Sign(relationship.Affinity) * AffinityDriftRate * daysSinceInteraction;
            relationship.Affinity = Math.Clamp(relationship.Affinity - drift, -1.0, 1.0);
        }
        
        relationship.UpdatedAt = DateTimeOffset.UtcNow;
        
        return result;
    }
    
    /// <summary>
    /// Trigger jealousy based on events (e.g., friend got close with rival)
    /// </summary>
    public void TriggerJealousy(NPCRelationship relationship, double jealousyAmount)
    {
        relationship.Jealousy = Math.Min(1.0, relationship.Jealousy + jealousyAmount);
        
        // Jealousy reduces trust
        relationship.Trust = Math.Max(-1.0, relationship.Trust - jealousyAmount * 0.5);
        
        // Jealousy increases hostility toward perceived rival
        relationship.Hostility = Math.Min(1.0, relationship.Hostility + jealousyAmount * 0.3);
        
        relationship.UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Process a conflict between two NPCs
    /// </summary>
    public ConflictResult ProcessConflict(
        NPCRelationship relationship,
        ConflictLevel severity)
    {
        var result = new ConflictResult();
        
        // Calculate conflict impact based on severity
        var hostilityIncrease = severity switch
        {
            ConflictLevel.Minor => 0.1,
            ConflictLevel.Moderate => 0.25,
            ConflictLevel.Serious => 0.4,
            ConflictLevel.Major => 0.6,
            _ => 0.1
        };
        
        // Trust takes a big hit
        var trustDrop = severity switch
        {
            ConflictLevel.Minor => 0.05,
            ConflictLevel.Moderate => 0.15,
            ConflictLevel.Serious => 0.3,
            ConflictLevel.Major => 0.5,
            _ => 0.05
        };
        
        relationship.Hostility = Math.Min(1.0, relationship.Hostility + hostilityIncrease);
        relationship.Trust = Math.Max(-1.0, relationship.Trust - trustDrop);
        relationship.Affinity = Math.Max(-1.0, relationship.Affinity - hostilityIncrease * 0.5);
        relationship.Resentment = Math.Min(1.0, relationship.Resentment + hostilityIncrease * 0.5);
        
        relationship.UpdatedAt = DateTimeOffset.UtcNow;
        
        result.HostilityIncrease = hostilityIncrease;
        result.TrustDrop = trustDrop;
        result.NewRelationshipType = DetermineRelationshipType(relationship);
        
        return result;
    }
    
    /// <summary>
    /// Attempt to reconcile after a conflict
    /// </summary>
    public bool TryReconcile(NPCRelationship relationship, double apologySincerity)
    {
        // High sincerity + low resentment = successful reconciliation
        var reconciliationChance = apologySincerity * (1.0 - relationship.Resentment);
        
        if (_random.NextDouble() < reconciliationChance)
        {
            // Partial forgiveness
            relationship.Hostility = Math.Max(0, relationship.Hostility - 0.3);
            relationship.Resentment = Math.Max(0, relationship.Resentment - 0.4);
            relationship.Trust = Math.Min(1.0, relationship.Trust + 0.1);
            relationship.UpdatedAt = DateTimeOffset.UtcNow;
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Determine relationship type based on dimensions
    /// </summary>
    private string DetermineRelationshipType(NPCRelationship relationship)
    {
        var score = relationship.Affinity + relationship.Trust + relationship.Respect;
        var negativeScore = relationship.Hostility + relationship.Jealousy + relationship.Resentment;
        
        // High positive + low negative = friends or romance
        if (score > FriendshipThreshold && negativeScore < 0.3)
        {
            if (relationship.Attraction > RomanceThreshold)
                return "romantic_partner";
            return "friend";
        }
        
        // High negative = rival or enemy
        if (negativeScore > RivalryThreshold || relationship.Hostility > 0.5)
        {
            return "rival";
        }
        
        // High familiarity, neutral everything = acquaintance
        if (relationship.Familiarity > 0.5 && Math.Abs(score) < 0.2)
        {
            return "acquaintance";
        }
        
        // Low everything = stranger
        if (relationship.Familiarity < 0.2)
        {
            return "stranger";
        }
        
        return "neutral";
    }
    
    /// <summary>
    /// Get base relationship changes for an interaction outcome
    /// </summary>
    private RelationshipChanges GetBaseChangeForOutcome(InteractionOutcome outcome)
    {
        return outcome switch
        {
            InteractionOutcome.Positive => new RelationshipChanges
            {
                Affinity = PositiveInteractionAffinityBoost,
                Trust = PositiveInteractionTrustBoost,
                Respect = 0.01,
                Hostility = -0.01
            },
            InteractionOutcome.Negative => new RelationshipChanges
            {
                Affinity = NegativeInteractionAffinityDrop,
                Trust = NegativeInteractionTrustDrop,
                Respect = -0.02,
                Hostility = 0.05
            },
            InteractionOutcome.Neutral => new RelationshipChanges
            {
                Affinity = 0.005,
                Trust = 0,
                Respect = 0,
                Hostility = 0
            },
            InteractionOutcome.Helpful => new RelationshipChanges
            {
                Affinity = 0.03,
                Trust = 0.03,
                Respect = 0.02,
                Hostility = -0.02
            },
            InteractionOutcome.Hurtful => new RelationshipChanges
            {
                Affinity = -0.05,
                Trust = -0.08,
                Respect = -0.03,
                Hostility = 0.1
            },
            InteractionOutcome.Supportive => new RelationshipChanges
            {
                Affinity = 0.04,
                Trust = 0.02,
                Respect = 0.01,
                Hostility = 0
            },
            InteractionOutcome.Competitive => new RelationshipChanges
            {
                Affinity = -0.02,
                Trust = -0.01,
                Respect = 0.02,
                Hostility = 0.03
            },
            _ => new RelationshipChanges()
        };
    }
    
    /// <summary>
    /// Apply personality-based modifiers to relationship changes
    /// </summary>
    private RelationshipChanges ApplyPersonalityModifiers(RelationshipChanges changes, NPCRelationship relationship)
    {
        // Personality traits would affect how changes are perceived
        // For now, apply generic modifiers
        
        // People with high agreeableness are more affected by positive interactions
        // People with high neuroticism are more affected by negative interactions
        
        return changes;
    }
    
    /// <summary>
    /// Clamp changes to prevent instant dramatic shifts
    /// </summary>
    private RelationshipChanges ClampChanges(RelationshipChanges changes)
    {
        // Maximum change per interaction
        const double maxAffinityChange = 0.15;
        const double maxTrustChange = 0.1;
        const double maxRespectChange = 0.05;
        const double maxHostilityChange = 0.1;
        
        return new RelationshipChanges
        {
            Affinity = Math.Clamp(changes.Affinity, -maxAffinityChange, maxAffinityChange),
            Trust = Math.Clamp(changes.Trust, -maxTrustChange, maxTrustChange),
            Respect = Math.Clamp(changes.Respect, -maxRespectChange, maxRespectChange),
            Hostility = Math.Clamp(changes.Hostility, -maxHostilityChange, maxHostilityChange)
        };
    }
}

/// <summary>
/// Result of relationship update
/// </summary>
public class RelationshipUpdateResult
{
    public string OldType { get; set; } = "";
    public string NewType { get; set; } = "";
    public RelationshipChanges Changes { get; set; } = new();
    public bool TypeChanged => OldType != NewType;
}

/// <summary>
/// Magnitude of relationship changes
/// </summary>
public class RelationshipChanges
{
    public double Affinity { get; set; }
    public double Trust { get; set; }
    public double Respect { get; set; }
    public double Hostility { get; set; }
}

/// <summary>
/// Outcome of an interaction
/// </summary>
public enum InteractionOutcome
{
    Positive,      // Liked, enjoyed
    Negative,       // Disliked, annoyed
    Neutral,        // Indifferent
    Helpful,        // Was helpful
    Hurtful,        // Was harmful
    Supportive,     // Showed support
    Competitive     // Competitive behavior
}

/// <summary>
/// Result of conflict processing
/// </summary>
public class ConflictResult
{
    public double HostilityIncrease { get; set; }
    public double TrustDrop { get; set; }
    public string NewRelationshipType { get; set; } = "";
}

// Extension to add RelationshipType to NPCRelationship if it doesn't exist
public static class RelationshipExtensions
{
    public static string GetRelationshipType(this NPCRelationship relationship)
    {
        // Check if RelationshipType property exists
        var prop = relationship.GetType().GetProperty("RelationshipType");
        if (prop != null)
        {
            return prop.GetValue(relationship) as string ?? "unknown";
        }
        return "unknown";
    }
}
