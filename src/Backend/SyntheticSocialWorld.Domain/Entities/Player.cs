using System.ComponentModel.DataAnnotations;

namespace SyntheticSocialWorld.Domain.Entities;

/// <summary>
/// Represents the human player in the Synthetic Social World.
/// The player is distinct from NPCs and has their own persistent identity.
/// </summary>
public class Player : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Handle { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// The world this player belongs to.
    /// </summary>
    public string WorldId { get; set; } = string.Empty;
    
    /// <summary>
    /// Last time the player was active.
    /// </summary>
    public DateTimeOffset LastActiveAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Number of followers.
    /// </summary>
    public int FollowerCount { get; set; }
    
    /// <summary>
    /// Number of accounts the player follows.
    /// </summary>
    public int FollowingCount { get; set; }
    
    /// <summary>
    /// Player's reputation score.
    /// </summary>
    public double Reputation { get; set; } = 50.0;
    
    /// <summary>
    /// Popularity score (0-1000 scale).
    /// </summary>
    public double Popularity { get; set; }
    
    // Navigation properties
    public virtual World? World { get; set; }
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public virtual ICollection<PlayerInterest> Interests { get; set; } = new List<PlayerInterest>();
}

/// <summary>
/// Player's interests/topics they care about.
/// </summary>
public class PlayerInterest : BaseEntity
{
    public string PlayerId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Topic { get; set; } = string.Empty;
    
    /// <summary>
    /// Weight of interest from 0.0 to 1.0.
    /// </summary>
    public double Weight { get; set; } = 0.5;
    
    // Navigation
    public virtual Player? Player { get; set; }
}

/// <summary>
/// DTO for creating a new player.
/// </summary>
public class CreatePlayerRequest
{
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Handle can only contain letters, numbers, and underscores")]
    public string Handle { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    public string? AvatarUrl { get; set; }
    
    public List<string>? Interests { get; set; }
}

/// <summary>
/// DTO for updating player profile.
/// </summary>
public class UpdatePlayerRequest
{
    [MaxLength(100)]
    public string? DisplayName { get; set; }
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    public string? AvatarUrl { get; set; }
    
    public List<string>? Interests { get; set; }
}

/// <summary>
/// DTO for player profile (public view).
/// </summary>
public class PlayerProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public double Reputation { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastActiveAt { get; set; }
    public List<string> Interests { get; set; } = new();
    public int PostCount { get; set; }
}

/// <summary>
/// DTO for current player (includes private info).
/// </summary>
public class CurrentPlayerDto : PlayerProfileDto
{
    public int UnreadNotificationCount { get; set; }
}
