using System.ComponentModel.DataAnnotations;

namespace SyntheticSocialWorld.Domain.Entities;

/// <summary>
/// A community/forum where NPCs can gather around shared interests.
/// </summary>
public class Community : BaseEntity
{
    [Required]
    public string WorldId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Handle { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Topic { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(2000)]
    public string? Rules { get; set; }
    
    /// <summary>
    /// Culture score from 0.0 (toxic) to 1.0 (healthy).
    /// </summary>
    public double CultureScore { get; set; } = 0.5;
    
    /// <summary>
    /// Toxicity level from 0.0 to 1.0.
    /// </summary>
    public double ToxicityLevel { get; set; }
    
    /// <summary>
    /// Popularity score.
    /// </summary>
    public double Popularity { get; set; }
    
    /// <summary>
    /// Number of members.
    /// </summary>
    public int MemberCount { get; set; }
    
    /// <summary>
    /// ID of the NPC who created this community.
    /// </summary>
    public string? CreatedById { get; set; }
    
    // Navigation
    public virtual World? World { get; set; }
    public virtual NPC? CreatedBy { get; set; }
    public virtual ICollection<CommunityMember> Members { get; set; } = new List<CommunityMember>();
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}

/// <summary>
/// Membership of an NPC in a community.
/// </summary>
public class CommunityMember : BaseEntity
{
    [Required]
    public string CommunityId { get; set; } = string.Empty;
    
    [Required]
    public string NPCId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "member"; // member, moderator, admin
    
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Navigation
    public virtual Community? Community { get; set; }
    public virtual NPC? NPC { get; set; }
}

/// <summary>
/// A scheduled or occurring social event.
/// </summary>
public class Event : BaseEntity
{
    /// <summary>
    /// Optional community hosting this event.
    /// </summary>
    public string? CommunityId { get; set; }
    
    [Required]
    public string OrganizerId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = "meetup"; // party, tournament, meetup, protest, livestream, celebration
    
    [MaxLength(200)]
    public string? Location { get; set; }
    
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    
    public int AttendeeCount { get; set; }
    public int? MaxAttendees { get; set; }
    
    /// <summary>
    /// Popularity score.
    /// </summary>
    public double Popularity { get; set; }
    
    // Navigation
    public virtual Community? Community { get; set; }
    public virtual NPC? Organizer { get; set; }
    public virtual ICollection<EventAttendee> Attendees { get; set; } = new List<EventAttendee>();
}

/// <summary>
/// Attendance record for an event.
/// </summary>
public class EventAttendee : BaseEntity
{
    [Required]
    public string EventId { get; set; } = string.Empty;
    
    [Required]
    public string NPCId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "attending"; // attending, interested, not_attending
    
    // Navigation
    public virtual Event? Event { get; set; }
    public virtual NPC? NPC { get; set; }
}
