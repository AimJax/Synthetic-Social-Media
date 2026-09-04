using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;

namespace SyntheticSocialWorld.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for Synthetic Social World.
/// </summary>
public class SyntheticSocialWorldDbContext : DbContext
{
    public SyntheticSocialWorldDbContext(DbContextOptions<SyntheticSocialWorldDbContext> options)
        : base(options)
    {
    }

    // Core entities
    public DbSet<World> Worlds => Set<World>();
    public DbSet<NPC> NPCs => Set<NPC>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerInterest> PlayerInterests => Set<PlayerInterest>();
    public DbSet<Personality> NPCPersonalities => Set<Personality>();
    public DbSet<Mood> NPCMoods => Set<Mood>();
    public DbSet<Interest> NPCInterests => Set<Interest>();
    public DbSet<Goal> NPCGoals => Set<Goal>();

    // Social entities
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Community and events
    public DbSet<Community> Communities => Set<Community>();
    public DbSet<CommunityMember> CommunityMembers => Set<CommunityMember>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventAttendee> EventAttendees => Set<EventAttendee>();

    // Relationships
    public DbSet<NPCRelationship> NPCRelationships => Set<NPCRelationship>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<PostEngagement> PostEngagements => Set<PostEngagement>();

    // Memory
    public DbSet<EpisodicMemory> EpisodicMemories => Set<EpisodicMemory>();
    public DbSet<SemanticBelief> SemanticBeliefs => Set<SemanticBelief>();
    public DbSet<SocialMemory> SocialMemories => Set<SocialMemory>();
    public DbSet<Rumor> Rumors => Set<Rumor>();
    public DbSet<KnowledgeEntry> KnowledgeEntries => Set<KnowledgeEntry>();

    // Simulation
    public DbSet<ScheduledAction> ScheduledActions => Set<ScheduledAction>();
    public DbSet<DomainEventRecord> DomainEvents => Set<DomainEventRecord>();
    public DbSet<ConfigurationEntry> ConfigurationEntries => Set<ConfigurationEntry>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // World
        modelBuilder.Entity<World>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CurrentTime).IsRequired();
        });

        // Player
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Handle).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Handle).IsUnique();
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.LastActiveAt);

            entity.HasOne(e => e.World)
                  .WithMany(w => w.Players)
                  .HasForeignKey(e => e.WorldId);
        });

        // PlayerInterest
        modelBuilder.Entity<PlayerInterest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PlayerId, e.Topic }).IsUnique();
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(100);
        });

        // NPC
        modelBuilder.Entity<NPC>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Handle).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Handle).IsUnique();
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.LastActiveAt);

            entity.HasOne(e => e.World)
                  .WithMany(w => w.NPCs)
                  .HasForeignKey(e => e.WorldId);

            entity.HasOne(e => e.Personality)
                  .WithOne(p => p.NPC)
                  .HasForeignKey<Personality>(p => p.NPCId);

            entity.HasOne(e => e.Mood)
                  .WithOne(m => m.NPC)
                  .HasForeignKey<Mood>(m => m.NPCId);
        });

        // Personality
        modelBuilder.Entity<Personality>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NPCId).IsUnique();
        });

        // Mood
        modelBuilder.Entity<Mood>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NPCId).IsUnique();
            entity.Property(e => e.PrimaryMood).HasMaxLength(50);
        });

        // Interest
        modelBuilder.Entity<Interest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.NPCId, e.Topic }).IsUnique();
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(100);
        });

        // Goal
        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NPCId);
            entity.Property(e => e.GoalType).IsRequired().HasMaxLength(100);
        });

        // Post
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.CommunityId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ImportanceScore);

            entity.HasOne(e => e.NpcAuthor)
                  .WithMany(n => n.Posts)
                  .HasForeignKey(e => e.AuthorId)
                  .HasPrincipalKey(n => n.Id)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Comment
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.ParentCommentId);

            entity.HasOne(e => e.Post)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(e => e.PostId);

            entity.HasOne(e => e.NpcAuthor)
                  .WithMany(n => n.Comments)
                  .HasForeignKey(e => e.AuthorId)
                  .HasPrincipalKey(n => n.Id)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Message
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
            entity.HasIndex(e => e.SenderId);
            entity.HasIndex(e => e.RecipientId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.NpcSender)
                  .WithMany(n => n.SentMessages)
                  .HasForeignKey(e => e.SenderId)
                  .HasPrincipalKey(n => n.Id)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NpcRecipient)
                  .WithMany(n => n.ReceivedMessages)
                  .HasForeignKey(e => e.RecipientId)
                  .HasPrincipalKey(n => n.Id)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.RecipientId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.NpcRecipient)
                  .WithMany(n => n.Notifications)
                  .HasForeignKey(e => e.RecipientId)
                  .HasPrincipalKey(n => n.Id)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Community
        modelBuilder.Entity<Community>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Handle).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Handle).IsUnique();
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.Popularity);
        });

        // CommunityMember
        modelBuilder.Entity<CommunityMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CommunityId, e.NPCId }).IsUnique();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        // Event
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.CommunityId);
            entity.HasIndex(e => e.OrganizerId);
            entity.HasIndex(e => e.StartTime);
        });

        // EventAttendee
        modelBuilder.Entity<EventAttendee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EventId, e.NPCId }).IsUnique();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        });

        // NPCRelationship
        modelBuilder.Entity<NPCRelationship>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceNpcId, e.TargetNpcId }).IsUnique();
            entity.HasIndex(e => e.SourceNpcId);
            entity.HasIndex(e => e.TargetNpcId);
        });

        // Follow
        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.FollowerId, e.FollowedId }).IsUnique();
            entity.HasIndex(e => e.FollowerId);
            entity.HasIndex(e => e.FollowedId);
        });

        // PostEngagement
        modelBuilder.Entity<PostEngagement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PostId, e.NPCId, e.Type }).IsUnique();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
        });

        // EpisodicMemory
        modelBuilder.Entity<EpisodicMemory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.Importance);
            entity.HasIndex(e => e.Timestamp);
        });

        // SemanticBelief
        modelBuilder.Entity<SemanticBelief>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Belief).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.Subject);
        });

        // SocialMemory
        modelBuilder.Entity<SocialMemory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.Timestamp);
        });

        // Rumor
        modelBuilder.Entity<Rumor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Subject);
            entity.HasIndex(e => e.CreatedAt);
        });

        // KnowledgeEntry
        modelBuilder.Entity<KnowledgeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.KnowledgeType).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.NPCId);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });

        // ScheduledAction
        modelBuilder.Entity<ScheduledAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionType).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.NPCId);
            entity.HasIndex(e => e.ScheduledFor);
            entity.HasIndex(e => e.Priority);
        });

        // DomainEventRecord
        modelBuilder.Entity<DomainEventRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsProcessed);
        });

        // ConfigurationEntry
        modelBuilder.Entity<ConfigurationEntry>(entity =>
        {
            entity.HasKey(e => e.Key);
        });

        // FeatureFlag
        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(e => e.Key);
        });
    }
}
