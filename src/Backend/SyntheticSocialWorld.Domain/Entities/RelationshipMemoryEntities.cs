using System.ComponentModel.DataAnnotations;

namespace SyntheticSocialWorld.Domain.Entities;

/// <summary>
/// A directional, multi-dimensional relationship between two NPCs.
/// </summary>
public class NPCRelationship : BaseEntity
{
    /// <summary>
    /// NPC who holds this relationship (source).
    /// </summary>
    [Required]
    public string SourceNpcId { get; set; } = string.Empty;
    
    /// <summary>
    /// NPC this relationship is about (target).
    /// </summary>
    [Required]
    public string TargetNpcId { get; set; } = string.Empty;
    
    // Multi-dimensional relationship values (-1.0 to 1.0 or 0.0 to 1.0 where noted)
    
    /// <summary>
    /// General liking/disliking (-1.0 to 1.0).
    /// </summary>
    public double Affinity { get; set; }
    
    /// <summary>
    /// Trustworthiness and reliability (-1.0 to 1.0).
    /// </summary>
    public double Trust { get; set; }
    
    /// <summary>
    /// Admiration and respect (-1.0 to 1.0).
    /// </summary>
    public double Respect { get; set; }
    
    /// <summary>
    /// Romantic interest (-1.0 to 1.0).
    /// </summary>
    public double Attraction { get; set; }
    
    /// <summary>
    /// Active antagonism (0.0 to 1.0).
    /// </summary>
    public double Hostility { get; set; }
    
    /// <summary>
    /// Envy and rivalry (0.0 to 1.0).
    /// </summary>
    public double Jealousy { get; set; }
    
    /// <summary>
    /// Anxiety about the target (0.0 to 1.0).
    /// </summary>
    public double Fear { get; set; }
    
    /// <summary>
    /// Positive esteem (0.0 to 1.0).
    /// </summary>
    public double Admiration { get; set; }
    
    /// <summary>
    /// Stored bitterness (0.0 to 1.0).
    /// </summary>
    public double Resentment { get; set; }
    
    /// <summary>
    /// How well they know each other (0.0 to 1.0).
    /// </summary>
    public double Familiarity { get; set; }
    
    /// <summary>
    /// Mutual connection indicator (0.0 to 1.0).
    /// </summary>
    public double MutualConnection { get; set; }
    
    /// <summary>
    /// Last time these NPCs interacted.
    /// </summary>
    public DateTimeOffset? LastInteractionAt { get; set; }
    
    // Navigation
    public virtual NPC? SourceNpc { get; set; }
    public virtual NPC? TargetNpc { get; set; }
}

/// <summary>
/// A follow relationship.
/// </summary>
public class Follow : BaseEntity
{
    [Required]
    public string FollowerId { get; set; } = string.Empty;
    
    [Required]
    public string FollowedId { get; set; } = string.Empty;
    
    // Navigation
    public virtual NPC? Follower { get; set; }
    public virtual NPC? Followed { get; set; }
}

/// <summary>
/// A like, dislike, or share of a post.
/// </summary>
public class PostEngagement : BaseEntity
{
    [Required]
    public string PostId { get; set; } = string.Empty;
    
    [Required]
    public string NPCId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Type { get; set; } = "like"; // like, dislike, share
    
    // Navigation
    public virtual Post? Post { get; set; }
    public virtual NPC? NPC { get; set; }
}
