using System.ComponentModel.DataAnnotations;

namespace SyntheticSocialWorld.Domain.Entities;

/// <summary>
/// A scheduled future action for an NPC.
/// </summary>
public class ScheduledAction : BaseEntity
{
    /// <summary>
    /// NPC who should perform this action.
    /// </summary>
    [Required]
    public string NPCId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of action to perform.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of target entity (post, user, community, etc.).
    /// </summary>
    [MaxLength(50)]
    public string? TargetType { get; set; }
    
    /// <summary>
    /// ID of the target entity.
    /// </summary>
    public string? TargetId { get; set; }
    
    /// <summary>
    /// When this action should be executed.
    /// </summary>
    public DateTimeOffset ScheduledFor { get; set; }
    
    /// <summary>
    /// Priority (higher = more urgent).
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// JSON parameters for the action.
    /// </summary>
    public string? Parameters { get; set; }
    
    /// <summary>
    /// Whether this action has been executed.
    /// </summary>
    public bool IsExecuted { get; set; }
    
    // Navigation
    public virtual NPC? NPC { get; set; }
}

/// <summary>
/// Stores a record of all domain events for audit/history.
/// </summary>
public class DomainEventRecord : BaseEntity
{
    /// <summary>
    /// Type of event (e.g., "PostCreated", "RelationshipChanged").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of entity affected.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the affected entity.
    /// </summary>
    [Required]
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON payload of the event.
    /// </summary>
    [Required]
    public string Payload { get; set; } = "{}";
    
    /// <summary>
    /// World time when event occurred.
    /// </summary>
    public DateTimeOffset WorldTime { get; set; }
    
    /// <summary>
    /// Whether this event has been processed by handlers.
    /// </summary>
    public bool IsProcessed { get; set; }
}

/// <summary>
/// Tracks schema version for migrations.
/// </summary>
public class SchemaVersion
{
    public int Version { get; set; }
    public DateTimeOffset AppliedAt { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Configuration key-value store.
/// </summary>
public class ConfigurationEntry
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Feature flag for experimental features.
/// </summary>
public class FeatureFlag
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    
    public bool IsEnabled { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
}
