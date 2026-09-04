using System.ComponentModel.DataAnnotations;

namespace SyntheticSocialWorld.Domain.Entities;

/// <summary>
/// Represents an NPC (Non-Player Character) in the simulation.
/// NPCs are persistent entities with personalities, moods, memories, and relationships.
/// </summary>
public class NPC : BaseEntity
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
    /// Reference to the world this NPC belongs to.
    /// </summary>
    public string WorldId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this NPC is the human player.
    /// </summary>
    public bool IsPlayer { get; set; }
    
    /// <summary>
    /// Activity level from 0.0 (lurker) to 1.0 (highly active).
    /// </summary>
    public double ActivityLevel { get; set; } = 0.5;
    
    /// <summary>
    /// Popularity score (0-1000 scale).
    /// </summary>
    public double Popularity { get; set; }
    
    /// <summary>
    /// Number of followers.
    /// </summary>
    public int FollowerCount { get; set; }
    
    /// <summary>
    /// Number of accounts this NPC follows.
    /// </summary>
    public int FollowingCount { get; set; }
    
    /// <summary>
    /// General reputation score.
    /// </summary>
    public double Reputation { get; set; }
    
    /// <summary>
    /// Last time this NPC was active.
    /// </summary>
    public DateTimeOffset LastActiveAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Next scheduled action time.
    /// </summary>
    public DateTimeOffset? NextScheduledAction { get; set; }
    
    /// <summary>
    /// Current activity type.
    /// </summary>
    public string? CurrentActivity { get; set; }
    
    // Navigation properties
    public virtual World? World { get; set; }
    public virtual Personality? Personality { get; set; }
    public virtual Mood? Mood { get; set; }
    public virtual ICollection<Interest> Interests { get; set; } = new List<Interest>();
    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

/// <summary>
/// NPC personality traits (Big Five + additional dimensions).
/// All values are 0.0 to 1.0.
/// </summary>
public class Personality : BaseEntity
{
    public string NPCId { get; set; } = string.Empty;
    
    // Big Five
    public double Openness { get; set; } = 0.5;          // Curiosity, creativity
    public double Extroversion { get; set; } = 0.5;       // Sociability, assertiveness
    public double Agreeableness { get; set; } = 0.5;      // Cooperation, trust
    public double Conscientiousness { get; set; } = 0.5;  // Organization, diligence
    public double Neuroticism { get; set; } = 0.5;        // Emotional instability
    
    // Additional traits
    public double Confidence { get; set; } = 0.5;        // Self-assurance
    public double Empathy { get; set; } = 0.5;             // Understanding others
    public double Sarcasm { get; set; } = 0.5;            // Tendency for sarcasm
    public double Humor { get; set; } = 0.5;              // Comedy appreciation
    public double Aggression { get; set; } = 0.5;         // Hostile tendencies
    public double Curiosity { get; set; } = 0.5;           // Inquisitiveness
    public double Impulsiveness { get; set; } = 0.5;      // Spontaneity
    public double Patience { get; set; } = 0.5;           // Tolerance, restraint
    public double Competitiveness { get; set; } = 0.5;    // Drive to win
    public double Jealousy { get; set; } = 0.5;           // Envy tendency
    public double Conformity { get; set; } = 0.5;          // Following norms
    public double Independence { get; set; } = 0.5;        // Self-reliance
    public double RiskTolerance { get; set; } = 0.5;       // Boldness
    public double Sociability { get; set; } = 0.5;         // Social interaction desire
    
    // Navigation
    public virtual NPC? NPC { get; set; }
}

/// <summary>
/// NPC's current emotional state. Values are 0.0 to 1.0.
/// </summary>
public class Mood : BaseEntity
{
    public string NPCId { get; set; } = string.Empty;
    
    public double Happiness { get; set; } = 0.5;
    public double Sadness { get; set; }
    public double Anger { get; set; }
    public double Excitement { get; set; }
    public double Anxiety { get; set; }
    public double Embarrassment { get; set; }
    public double Affection { get; set; }
    public double Jealousy { get; set; }
    public double Loneliness { get; set; }
    public double Confidence { get; set; } = 0.5;
    
    /// <summary>
    /// Primary mood (derived from highest value).
    /// </summary>
    [MaxLength(50)]
    public string PrimaryMood { get; set; } = "neutral";
    
    // Navigation
    public virtual NPC? NPC { get; set; }
}

/// <summary>
/// An interest/topic that an NPC cares about.
/// </summary>
public class Interest : BaseEntity
{
    public string NPCId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Topic { get; set; } = string.Empty;
    
    /// <summary>
    /// Weight of interest from 0.0 to 1.0.
    /// </summary>
    public double Weight { get; set; } = 0.5;
    
    // Navigation
    public virtual NPC? NPC { get; set; }
}

/// <summary>
/// A goal that an NPC is working toward.
/// </summary>
public class Goal : BaseEntity
{
    public string NPCId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string GoalType { get; set; } = string.Empty;
    
    /// <summary>
    /// Priority from 0.0 to 1.0.
    /// </summary>
    public double Priority { get; set; } = 0.5;
    
    /// <summary>
    /// Progress toward goal from 0.0 to 1.0.
    /// </summary>
    public double Progress { get; set; }
    
    // Navigation
    public virtual NPC? NPC { get; set; }
}
