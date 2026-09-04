using System.ComponentModel.DataAnnotations;

namespace SyntheticSocialWorld.Domain.Entities;

/// <summary>
/// Author types for social entities.
/// </summary>
public enum AuthorType
{
    NPC,
    Player
}

/// <summary>
/// A social media post created by an NPC or player.
/// </summary>
public class Post : BaseEntity
{
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of author (NPC or Player).
    /// </summary>
    public AuthorType AuthorType { get; set; } = AuthorType.NPC;
    
    /// <summary>
    /// Optional community this post was made in.
    /// </summary>
    public string? CommunityId { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public int ViewCount { get; set; }
    
    /// <summary>
    /// Whether this post has been deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Importance score for simulation priority.
    /// </summary>
    public double ImportanceScore { get; set; } = 0.1;
    
    /// <summary>
    /// Computed popularity score for ranking.
    /// </summary>
    public double Popularity { get; set; }
    
    // Navigation
    public virtual NPC? NpcAuthor { get; set; }
    public virtual Player? PlayerAuthor { get; set; }
    public virtual Community? Community { get; set; }
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

/// <summary>
/// A comment on a post or reply to another comment.
/// </summary>
public class Comment : BaseEntity
{
    [Required]
    public string PostId { get; set; } = string.Empty;
    
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of author (NPC or Player).
    /// </summary>
    public AuthorType AuthorType { get; set; } = AuthorType.NPC;
    
    /// <summary>
    /// Parent comment if this is a reply.
    /// </summary>
    public string? ParentCommentId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
    
    public int LikeCount { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation
    public virtual Post? Post { get; set; }
    public virtual NPC? NpcAuthor { get; set; }
    public virtual Player? PlayerAuthor { get; set; }
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}

/// <summary>
/// A direct message between two entities (NPC or Player).
/// </summary>
public class Message : BaseEntity
{
    [Required]
    public string SenderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of sender (NPC or Player).
    /// </summary>
    public AuthorType SenderType { get; set; } = AuthorType.NPC;
    
    [Required]
    public string RecipientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of recipient (NPC or Player).
    /// </summary>
    public AuthorType RecipientType { get; set; } = AuthorType.NPC;
    
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public bool IsRead { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation
    public virtual NPC? NpcSender { get; set; }
    public virtual Player? PlayerSender { get; set; }
    public virtual NPC? NpcRecipient { get; set; }
    public virtual Player? PlayerRecipient { get; set; }
}

/// <summary>
/// A notification sent to an NPC or player.
/// </summary>
public class Notification : BaseEntity
{
    [Required]
    public string RecipientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of recipient (NPC or Player).
    /// </summary>
    public AuthorType RecipientType { get; set; } = AuthorType.NPC;
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Body { get; set; }
    
    /// <summary>
    /// ID of the related entity (post, user, etc.).
    /// </summary>
    public string? RelatedEntityId { get; set; }
    
    /// <summary>
    /// Type of related entity.
    /// </summary>
    [MaxLength(50)]
    public string? RelatedEntityType { get; set; }
    
    public bool IsRead { get; set; }
}


