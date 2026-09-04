using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Domain.Interfaces;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Infrastructure.Repositories;

public class WorldRepository : IWorldRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public WorldRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<World?> GetByIdAsync(string id)
    {
        return await _context.Worlds
            .Include(w => w.NPCs)
            .Include(w => w.Communities)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<World?> GetDefaultAsync()
    {
        return await _context.Worlds.FirstOrDefaultAsync();
    }

    public async Task<World> AddAsync(World world)
    {
        _context.Worlds.Add(world);
        await _context.SaveChangesAsync();
        return world;
    }

    public async Task UpdateAsync(World world)
    {
        world.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Worlds.Update(world);
        await _context.SaveChangesAsync();
    }

    public async Task EnsureDefaultWorldExistsAsync()
    {
        var existing = await GetDefaultAsync();
        if (existing == null)
        {
            var world = new World
            {
                Name = "Synthetic Social World",
                CurrentTime = DateTimeOffset.UtcNow,
                LastProcessedAt = DateTimeOffset.UtcNow
            };
            await AddAsync(world);
            
            // Seed initial NPCs
            await SeedInitialNpcsAsync(world.Id);
            
            // Seed initial communities
            await SeedInitialCommunitiesAsync(world.Id);
        }
    }

    private async Task SeedInitialNpcsAsync(string worldId)
    {
        var random = new Random(42); // Deterministic seed
        var firstNames = new[] { "Sarah", "Alex", "Mike", "Jessica", "David", "Emma", "Chris", "Olivia", "James", "Sophia", "Ben", "Luna", "Ryan", "Chloe", "Nathan", "Mia", "Ethan", "Ava", "Marcus", "Zoe" };
        var topics = new[] { "gaming", "tech", "music", "sports", "movies", "art", "science", "politics", "food", "travel" };

        for (int i = 0; i < 20; i++)
        {
            var npc = new NPC
            {
                WorldId = worldId,
                Handle = firstNames[i].ToLower() + (i > 0 ? i.ToString() : ""),
                DisplayName = firstNames[i],
                Bio = GenerateBio(firstNames[i], random),
                IsPlayer = false,
                ActivityLevel = 0.3 + random.NextDouble() * 0.7,
                Reputation = random.NextDouble()
            };

            _context.NPCs.Add(npc);
            await _context.SaveChangesAsync();

            // Add personality
            var personality = new Personality
            {
                NPCId = npc.Id,
                Openness = 0.3 + random.NextDouble() * 0.6,
                Extroversion = 0.3 + random.NextDouble() * 0.6,
                Agreeableness = 0.3 + random.NextDouble() * 0.6,
                Conscientiousness = 0.3 + random.NextDouble() * 0.6,
                Neuroticism = random.NextDouble() * 0.5,
                Confidence = 0.4 + random.NextDouble() * 0.5,
                Empathy = 0.3 + random.NextDouble() * 0.6,
                Sarcasm = random.NextDouble() * 0.8,
                Humor = 0.4 + random.NextDouble() * 0.5,
                Aggression = random.NextDouble() * 0.4,
                Curiosity = 0.5 + random.NextDouble() * 0.4,
                Impulsiveness = random.NextDouble() * 0.6,
                Patience = 0.4 + random.NextDouble() * 0.5,
                Competitiveness = random.NextDouble() * 0.7,
                Jealousy = random.NextDouble() * 0.5,
                Conformity = 0.3 + random.NextDouble() * 0.5,
                Independence = 0.3 + random.NextDouble() * 0.5,
                RiskTolerance = random.NextDouble() * 0.6,
                Sociability = 0.4 + random.NextDouble() * 0.5
            };
            _context.NPCPersonalities.Add(personality);

            // Add mood
            var mood = new Mood
            {
                NPCId = npc.Id,
                Happiness = 0.4 + random.NextDouble() * 0.4,
                Sadness = random.NextDouble() * 0.3,
                Anger = random.NextDouble() * 0.2,
                Excitement = random.NextDouble() * 0.4,
                Anxiety = random.NextDouble() * 0.3,
                PrimaryMood = "neutral"
            };
            _context.NPCMoods.Add(mood);

            // Add interests (2-4 random topics)
            var numInterests = 2 + random.Next(3);
            var selectedTopics = topics.OrderBy(_ => random.Next()).Take(numInterests);
            foreach (var topic in selectedTopics)
            {
                var interest = new Interest
                {
                    NPCId = npc.Id,
                    Topic = topic,
                    Weight = 0.5 + random.NextDouble() * 0.4
                };
                _context.NPCInterests.Add(interest);
            }

            // Add goals (1-2 random goals)
            var goalTypes = new[] { "gain_followers", "find_romance", "make_friends", "avoid_conflict", "gain_influence", "have_fun" };
            var numGoals = 1 + random.Next(2);
            var selectedGoals = goalTypes.OrderBy(_ => random.Next()).Take(numGoals);
            foreach (var goalType in selectedGoals)
            {
                var goal = new Goal
                {
                    NPCId = npc.Id,
                    GoalType = goalType,
                    Priority = 0.4 + random.NextDouble() * 0.4,
                    Progress = random.NextDouble() * 0.3
                };
                _context.NPCGoals.Add(goal);
            }

            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedInitialCommunitiesAsync(string worldId)
    {
        var random = new Random(42);
        var communities = new[]
        {
            ("Gaming", "gaming", "gaming", "For all gamers!"),
            ("Tech Talk", "tech", "tech", "Discuss the latest in technology"),
            ("Music Lovers", "music", "music", "Share and discover music"),
            ("Movie Buffs", "movies", "movies", "For cinema enthusiasts"),
            ("Sports Central", "sports", "sports", "All things sports")
        };

        foreach (var (name, handle, topic, desc) in communities)
        {
            var community = new Community
            {
                WorldId = worldId,
                Name = name,
                Handle = handle,
                Topic = topic,
                Description = desc,
                CultureScore = 0.6 + random.NextDouble() * 0.3,
                Popularity = 100 + random.Next(500)
            };
            _context.Communities.Add(community);
        }

        await _context.SaveChangesAsync();
    }

    private string GenerateBio(string name, Random random)
    {
        var templates = new[]
        {
            $"{name} here. Just living life.",
            $"Professional {PickRandom(new[] { "gamer", "developer", "music lover", "foodie" }, random)}.",
            $"Coffee and {PickRandom(new[] { "code", "games", "music", "movies" }, random)}.",
            $"Just here to have a good time.",
            $"Making the world a better place, one post at a time.",
            $"Following my passions.",
            $"Life's too short to be boring.",
            $"Professional procrastinator. Part-time productive."
        };
        return templates[random.Next(templates.Length)];
    }

    private string PickRandom(string[] options, Random random)
    {
        return options[random.Next(options.Length)];
    }
}
