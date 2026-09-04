using System.ComponentModel.DataAnnotations;

namespace SyntheticSocialWorld.Domain.Entities;

/// <summary>
/// An episodic memory - a specific event the NPC experienced.
/// </summary>
public class EpisodicMemory : BaseEntity
{
    /// <summary>
    /// NPC who owns this memory.
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of event (post_created, argument, compliment, etc.).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable description of the event.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON array of NPC IDs who participated.
    /// </summary>
    public string? Participants { get; set; }
    
    /// <summary>
    /// Importance score from 0.0 to 1.0.
    /// </summary>
    public double Importance { get; set; } = 0.1;
    
    /// <summary>
    /// Primary emotion associated (anger, joy, sadness, etc.).
    /// </summary>
    [MaxLength(50)]
    public string? Emotion { get; set; }
    
    /// <summary>
    /// When the event occurred (may differ from CreatedAt).
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
    
    /// <summary>
    /// How this memory was acquired (direct, told, observed).
    /// </summary>
    [MaxLength(50)]
    public string Source { get; set; } = "direct";
    
    /// <summary>
    /// Confidence in the memory's accuracy.
    /// </summary>
    public double Confidence { get; set; } = 1.0;
    
    // Navigation
    public virtual NPC? Owner { get; set; }
}

/// <summary>
/// A semantic belief - what the NPC believes about a subject.
/// </summary>
public class SemanticBelief : BaseEntity
{
    /// <summary>
    /// NPC who holds this belief.
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    
    /// <summary>
    /// The subject of the belief (entity or topic).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// The belief claim.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Belief { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence in this belief from 0.0 to 1.0.
    /// </summary>
    public double Confidence { get; set; } = 0.5;
    
    /// <summary>
    /// JSON array of supporting evidence.
    /// </summary>
    public string? SupportingEvidence { get; set; }
    
    /// <summary>
    /// JSON array of conflicting evidence.
    /// </summary>
    public string? ConflictingEvidence { get; set; }
    
    /// <summary>
    /// How this belief was formed (direct, inference, hearsay).
    /// </summary>
    [MaxLength(50)]
    public string Source { get; set; } = "direct";
    
    /// <summary>
    /// When the belief was first formed.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
    
    // Navigation
    public virtual NPC? Owner { get; set; }
}

/// <summary>
/// A social memory - record of a social interaction.
/// </summary>
public class SocialMemory : BaseEntity
{
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON array of participant NPC IDs.
    /// </summary>
    public string? Participants { get; set; }
    
    /// <summary>
    /// Type of social interaction (support, betrayal, collaboration, etc.).
    /// </summary>
    [MaxLength(50)]
    public string? RelationshipType { get; set; }
    
    public double Importance { get; set; } = 0.1;
    
    public DateTimeOffset Timestamp { get; set; }
    
    // Navigation
    public virtual NPC? Owner { get; set; }
}

/// <summary>
/// A rumor spreading through the social network.
/// </summary>
public class Rumor : BaseEntity
{
    /// <summary>
    /// NPC who originated this rumor.
    /// </summary>
    [Required]
    public string OriginatorId { get; set; } = string.Empty;
    
    /// <summary>
    /// Who/what the rumor is about.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Content of the rumor.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence in the rumor.
    /// </summary>
    public double Confidence { get; set; } = 0.5;
    
    /// <summary>
    /// JSON array tracking who spread it.
    /// </summary>
    public string? SourceChain { get; set; }
    
    /// <summary>
    /// Number of times this rumor was spread.
    /// </summary>
    public int SpreadCount { get; set; }
    
    // Navigation
    public virtual NPC? Originator { get; set; }
}

/// <summary>
/// An entry in the knowledge graph - what an NPC knows.
/// </summary>
public class KnowledgeEntry : BaseEntity
{
    [Required]
    public string NPCId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of entity known (post, comment, event, npc, community).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the known entity.
    /// </summary>
    [Required]
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// How the knowledge was acquired.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string KnowledgeType { get; set; } = "observed"; // observed, told, read, inferred, learned
    
    /// <summary>
    /// Confidence in the knowledge.
    /// </summary>
    public double Confidence { get; set; } = 1.0;
    
    /// <summary>
    /// When the knowledge was acquired.
    /// </summary>
    public DateTimeOffset AcquiredAt { get; set; }
    
    /// <summary>
    /// NPC who told them (if applicable).
    /// </summary>
    public string? SourceId { get; set; }
    
    // Navigation
    public virtual NPC? NPC { get; set; }
}
