using SyntheticSocialWorld.Domain.Entities;

namespace SyntheticSocialWorld.Simulation.Services;

/// <summary>
/// Implements conflict and drama system - NPCs can have disagreements, arguments, and drama
/// Based on SOCIAL_GRAPH.md Section 9
/// </summary>
public class ConflictDramaService
{
    private readonly Random _random = new();
    
    // Conflict probability per interaction
    private const double BaseConflictProbability = 0.05; // 5% base chance
    
    // Drama escalation rates
    private const double DramaEscalationRate = 0.15;
    private const double DramaDeescalationRate = 0.10;
    
    /// <summary>
    /// Determine if a conflict should occur between two NPCs
    /// </summary>
    public ConflictCheckResult CheckForConflict(
        NPC npc1,
        NPC npc2,
        NPCRelationship relationship,
        Personality? personality1 = null,
        Personality? personality2 = null,
        string? triggerContext = null)
    {
        var result = new ConflictCheckResult();
        
        // Get personality traits (default to moderate values if not provided)
        var p1 = personality1 ?? new Personality();
        var p2 = personality2 ?? new Personality();
        
        // Base probability
        var probability = BaseConflictProbability;
        
        // Personality clashes increase conflict
        if (p1.Extroversion > 0.7 && p2.Extroversion < 0.3)
            probability *= 1.5; // Introvert vs extrovert
        if (p1.Aggression > 0.6 || p2.Aggression > 0.6)
            probability *= 2.0; // Aggressive personalities
        
        // Relationship tension increases conflict
        probability *= (1.0 + relationship.Hostility);
        probability *= (1.0 + relationship.Jealousy * 0.5);
        probability *= (1.0 + relationship.Resentment * 0.5);
        
        // Trust reduces conflict
        probability *= (1.0 - relationship.Trust * 0.5);
        
        // Context can trigger conflict
        if (!string.IsNullOrEmpty(triggerContext))
        {
            if (triggerContext.Contains("competition"))
                probability *= 2.0;
            if (triggerContext.Contains("disagreement"))
                probability *= 1.5;
        }
        
        // Check if conflict occurs
        if (_random.NextDouble() < probability)
        {
            result.HasConflict = true;
            result.ConflictType = DetermineConflictType(relationship, p1, p2);
            result.Severity = DetermineSeverity(relationship, p1, p2);
            result.Description = GenerateConflictDescription(result.ConflictType, npc1, npc2);
        }
        
        return result;
    }
    
    /// <summary>
    /// Process an ongoing conflict between two NPCs
    /// </summary>
    public ConflictUpdateResult ProcessConflict(
        Conflict conflict,
        IEnumerable<NPCRelationship> witnesses)
    {
        var result = new ConflictUpdateResult();
        
        // Check for escalation
        if (_random.NextDouble() < conflict.CurrentIntensity * DramaEscalationRate)
        {
            conflict.CurrentIntensity = Math.Min(1.0, conflict.CurrentIntensity + 0.1);
            conflict.EscalationCount++;
            result.Escalated = true;
        }
        
        // Check for de-escalation (time heals)
        if (_random.NextDouble() < DramaDeescalationRate)
        {
            conflict.CurrentIntensity = Math.Max(0.1, conflict.CurrentIntensity - 0.05);
            result.Deescalated = true;
        }
        
        // Witnesses affect conflict
        var witnessCount = witnesses.Count();
        if (witnessCount > 0)
        {
            // Public conflicts escalate more
            conflict.CurrentIntensity = Math.Min(1.0, conflict.CurrentIntensity + witnessCount * 0.02);
            
            // Rumors spread from witnessed conflicts
            result.NewRumorsCreated = (int)(witnessCount * 0.3);
        }
        
        // Update status
        conflict.UpdatedAt = DateTimeOffset.UtcNow;
        
        return result;
    }
    
    /// <summary>
    /// Attempt to resolve a conflict
    /// </summary>
    public ConflictResolutionResult TryResolve(
        Conflict conflict,
        NPC initiator,
        NPC target,
        ResolutionType resolution,
        double sincerity,
        Personality? initiatorPersonality = null,
        Personality? targetPersonality = null)
    {
        var result = new ConflictResolutionResult();
        
        // Get personality traits
        var iPersonality = initiatorPersonality ?? new Personality();
        var tPersonality = targetPersonality ?? new Personality();
        
        // Calculate resolution success chance
        var baseChance = resolution switch
        {
            ResolutionType.Apology => 0.6 + sincerity * 0.3,
            ResolutionType.Mediator => 0.7 + sincerity * 0.2,
            ResolutionType.Time => 0.4 + sincerity * 0.2,
            ResolutionType.Compromise => 0.5 + sincerity * 0.3,
            ResolutionType.Avoidance => 0.3,
            _ => 0.5
        };
        
        // Personality affects resolution
        if (iPersonality.Neuroticism > 0.7) // High neuroticism = emotional instability
            baseChance *= 0.8;
        if (tPersonality.Agreeableness < 0.5) // Low agreeableness = harder to forgive
            baseChance *= 0.7;
        
        result.Success = _random.NextDouble() < baseChance;
        
        if (result.Success)
        {
            conflict.Status = ConflictStatus.Resolved;
            conflict.ResolvedAt = DateTimeOffset.UtcNow;
            
            // Calculate residual damage
            result.ResidualDamage = (1.0 - sincerity) * 0.3;
            result.TimeToRecover = resolution switch
            {
                ResolutionType.Apology => TimeSpan.FromDays(7),
                ResolutionType.Mediator => TimeSpan.FromDays(14),
                ResolutionType.Time => TimeSpan.FromDays(30),
                ResolutionType.Compromise => TimeSpan.FromDays(10),
                _ => TimeSpan.FromDays(21)
            };
        }
        else
        {
            // Failed resolution makes it worse
            conflict.CurrentIntensity = Math.Min(1.0, conflict.CurrentIntensity + 0.1);
            result.ResidualDamage = 0.5;
        }
        
        return result;
    }
    
    /// <summary>
    /// Create a new conflict record
    /// </summary>
    public Conflict CreateConflict(
        string npc1Id,
        string npc2Id,
        ConflictType type,
        ConflictLevel severity,
        string? context = null)
    {
        return new Conflict
        {
            Id = Guid.NewGuid().ToString(),
            Npc1Id = npc1Id,
            Npc2Id = npc2Id,
            Type = type,
            Severity = severity,
            CurrentIntensity = severity switch
            {
                ConflictLevel.Minor => 0.2,
                ConflictLevel.Moderate => 0.4,
                ConflictLevel.Serious => 0.6,
                ConflictLevel.Major => 0.8,
                _ => 0.3
            },
            Context = context,
            Status = ConflictStatus.Active,
            EscalationCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
    
    /// <summary>
    /// Generate dramatic events from conflicts
    /// </summary>
    public List<DramaticEvent> GenerateDramaticEvents(Conflict conflict)
    {
        var events = new List<DramaticEvent>();
        
        if (conflict.CurrentIntensity < 0.5)
            return events;
        
        // High intensity conflicts generate dramatic events
        if (_random.NextDouble() < conflict.CurrentIntensity * 0.3)
        {
            events.Add(new DramaticEvent
            {
                EventId = Guid.NewGuid().ToString(),
                ConflictId = conflict.Id,
                Type = DramaticEventType.SocialExclusion,
                Description = GenerateSocialExclusionEvent(conflict),
                Witnesses = (int)(conflict.CurrentIntensity * 5),
                Intensity = conflict.CurrentIntensity
            });
        }
        
        if (_random.NextDouble() < conflict.CurrentIntensity * 0.2)
        {
            events.Add(new DramaticEvent
            {
                EventId = Guid.NewGuid().ToString(),
                ConflictId = conflict.Id,
                Type = DramaticEventType.RumorSpread,
                Description = GenerateRumorEvent(conflict),
                Witnesses = (int)(conflict.CurrentIntensity * 3),
                Intensity = conflict.CurrentIntensity * 0.8
            });
        }
        
        if (conflict.CurrentIntensity > 0.7 && _random.NextDouble() < 0.2)
        {
            events.Add(new DramaticEvent
            {
                EventId = Guid.NewGuid().ToString(),
                ConflictId = conflict.Id,
                Type = DramaticEventType.PublicConfrontation,
                Description = GenerateConfrontationEvent(conflict),
                Witnesses = (int)(conflict.CurrentIntensity * 10),
                Intensity = conflict.CurrentIntensity
            });
        }
        
        return events;
    }
    
    private ConflictType DetermineConflictType(NPCRelationship relationship, Personality p1, Personality p2)
    {
        // Based on relationship tensions
        if (relationship.Jealousy > 0.5)
            return ConflictType.Jealousy;
        if (p1.Competitiveness > 0.5 || p2.Competitiveness > 0.5)
            return ConflictType.Competition;
        if (relationship.Resentment > 0.3)
            return ConflictType.Resentment;
        
        // Based on random chance
        return (ConflictType)_random.Next((int)ConflictType.MinorDisagreement, (int)ConflictType.Count);
    }
    
    private ConflictLevel DetermineSeverity(NPCRelationship relationship, Personality p1, Personality p2)
    {
        var tension = relationship.Hostility + relationship.Jealousy + relationship.Resentment;
        
        // Personality affects severity
        if (p1.Aggression > 0.7 || p2.Aggression > 0.7)
            tension += 0.2;
        
        return tension switch
        {
            < 0.3 => ConflictLevel.Minor,
            < 0.5 => ConflictLevel.Moderate,
            < 0.8 => ConflictLevel.Serious,
            _ => ConflictLevel.Major
        };
    }
    
    private string GenerateConflictDescription(ConflictType type, NPC npc1, NPC npc2)
    {
        return type switch
        {
            ConflictType.Jealousy => $"{npc1.DisplayName} is jealous of {npc2.DisplayName}",
            ConflictType.Competition => $"{npc1.DisplayName} and {npc2.DisplayName} are competing",
            ConflictType.Resentment => $"{npc1.DisplayName} resents {npc2.DisplayName}",
            ConflictType.MinorDisagreement => $"{npc1.DisplayName} and {npc2.DisplayName} had a disagreement",
            ConflictType.Argument => $"{npc1.DisplayName} argued with {npc2.DisplayName}",
            _ => $"{npc1.DisplayName} and {npc2.DisplayName} are in conflict"
        };
    }
    
    private string GenerateSocialExclusionEvent(Conflict conflict)
    {
        return "One NPC excluded the other from a social gathering";
    }
    
    private string GenerateRumorEvent(Conflict conflict)
    {
        return "A rumor spread about the conflict between them";
    }
    
    private string GenerateConfrontationEvent(Conflict conflict)
    {
        return "They had a very public confrontation that others noticed";
    }
}

/// <summary>
/// Represents an active conflict between two NPCs
/// </summary>
public class Conflict
{
    public string Id { get; set; } = "";
    public string Npc1Id { get; set; } = "";
    public string Npc2Id { get; set; } = "";
    public ConflictType Type { get; set; }
    public ConflictLevel Severity { get; set; }
    public double CurrentIntensity { get; set; }
    public string? Context { get; set; }
    public ConflictStatus Status { get; set; }
    public int EscalationCount { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Types of conflicts
/// </summary>
public enum ConflictType
{
    Jealousy,
    Competition,
    Resentment,
    MinorDisagreement,
    Argument,
    Betrayal,
    Count // For range
}

/// <summary>
/// Severity levels for conflicts
/// </summary>
public enum ConflictLevel
{
    Minor,
    Moderate,
    Serious,
    Major
}

/// <summary>
/// Resolution types
/// </summary>
public enum ResolutionType
{
    Apology,
    Mediator,
    Time,
    Compromise,
    Avoidance
}

/// <summary>
/// Conflict status
/// </summary>
public enum ConflictStatus
{
    Active,
    Cooling,
    Resolved,
    Stalemate
}

/// <summary>
/// Dramatic event from a conflict
/// </summary>
public class DramaticEvent
{
    public string EventId { get; set; } = "";
    public string ConflictId { get; set; } = "";
    public DramaticEventType Type { get; set; }
    public string Description { get; set; } = "";
    public int Witnesses { get; set; }
    public double Intensity { get; set; }
}

/// <summary>
/// Types of dramatic events
/// </summary>
public enum DramaticEventType
{
    SocialExclusion,
    RumorSpread,
    PublicConfrontation,
    GroupSplit,
    DramaticReveal
}

/// <summary>
/// Result of conflict check
/// </summary>
public class ConflictCheckResult
{
    public bool HasConflict { get; set; }
    public ConflictType ConflictType { get; set; }
    public ConflictLevel Severity { get; set; }
    public string Description { get; set; } = "";
}

/// <summary>
/// Result of conflict processing
/// </summary>
public class ConflictUpdateResult
{
    public bool Escalated { get; set; }
    public bool Deescalated { get; set; }
    public int NewRumorsCreated { get; set; }
}

/// <summary>
/// Result of conflict resolution
/// </summary>
public class ConflictResolutionResult
{
    public bool Success { get; set; }
    public double ResidualDamage { get; set; }
    public TimeSpan TimeToRecover { get; set; }
}
