namespace SyntheticSocialWorld.Domain.Entities;

/// <summary>
/// Represents the simulation world containing all NPCs and state.
/// </summary>
public class World : BaseEntity
{
    public string Name { get; set; } = "Synthetic Social World";
    
    /// <summary>
    /// Authoritative world time - persisted and survives restarts.
    /// </summary>
    public DateTimeOffset CurrentTime { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Last time the world was processed.
    /// </summary>
    public DateTimeOffset LastProcessedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Simulation speed multiplier (1.0 = real-time, 10.0 = 10x faster).
    /// </summary>
    public double Speed { get; set; } = 1.0;
    
    /// <summary>
    /// Whether the simulation is paused.
    /// </summary>
    public bool IsPaused { get; set; }
    
    /// <summary>
    /// Schema version for migrations.
    /// </summary>
    public int Version { get; set; } = 1;
    
    // Navigation properties
    public virtual ICollection<NPC> NPCs { get; set; } = new List<NPC>();
    public virtual ICollection<Community> Communities { get; set; } = new List<Community>();
}
