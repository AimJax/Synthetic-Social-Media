using SyntheticSocialWorld.Domain.Interfaces;

namespace SyntheticSocialWorld.Simulation;

/// <summary>
/// Main simulation service that coordinates NPC behavior and world state.
/// </summary>
public class SimulationService : ISimulationService
{
    private readonly INpcRepository _npcRepository;
    private readonly IPostRepository _postRepository;
    private readonly IWorldRepository _worldRepository;
    private readonly IRelationshipRepository _relationshipRepository;
    private readonly IScheduledActionRepository _scheduledActionRepository;
    private readonly IAIProvider? _aiProvider;
    
    private readonly Random _random = new Random();
    
    public SimulationService(
        INpcRepository npcRepository,
        IPostRepository postRepository,
        IWorldRepository worldRepository,
        IRelationshipRepository relationshipRepository,
        IScheduledActionRepository scheduledActionRepository,
        IAIProvider? aiProvider = null)
    {
        _npcRepository = npcRepository;
        _postRepository = postRepository;
        _worldRepository = worldRepository;
        _relationshipRepository = relationshipRepository;
        _scheduledActionRepository = scheduledActionRepository;
        _aiProvider = aiProvider;
    }
    
    public async Task AdvanceTimeAsync(TimeSpan delta)
    {
        var world = await _worldRepository.GetDefaultAsync();
        if (world == null) return;
        
        if (world.IsPaused) return;
        
        // Advance world time
        world.CurrentTime = world.CurrentTime.Add(delta);
        world.LastProcessedAt = DateTimeOffset.UtcNow;
        
        // Process due scheduled actions
        var dueActions = await _scheduledActionRepository.GetDueActionsAsync(world.CurrentTime);
        foreach (var action in dueActions)
        {
            await ExecuteScheduledActionAsync(action);
            action.IsExecuted = true;
            await _scheduledActionRepository.UpdateAsync(action);
        }
        
        await _worldRepository.UpdateAsync(world);
    }
    
    public async Task ProcessNpcAsync(string npcId)
    {
        var npc = await _npcRepository.GetByIdAsync(npcId);
        if (npc == null || npc.IsPlayer) return;
        
        // Update last active time
        npc.LastActiveAt = DateTimeOffset.UtcNow;
        await _npcRepository.UpdateAsync(npc);
        
        // Execute tier 1 (deterministic) actions based on personality and mood
        await ExecuteTier1ActionsAsync(npc);
        
        // Schedule next action
        await ScheduleNextActionAsync(npc);
    }
    
    private async Task ExecuteTier1ActionsAsync(Domain.Entities.NPC npc)
    {
        if (npc.Personality == null) return;
        
        var personality = npc.Personality;
        
        // Random chance to engage based on activity level and extroversion
        var engagementChance = npc.ActivityLevel * personality.Extroversion;
        
        if (_random.NextDouble() < engagementChance)
        {
            // Choose action based on personality
            var actionRoll = _random.NextDouble();
            
            if (actionRoll < 0.4)
            {
                // Like a post
                await SimulateLikeAsync(npc);
            }
            else if (actionRoll < 0.6)
            {
                // Browse feed (simulated as view)
                await SimulateBrowseAsync(npc);
            }
            else if (actionRoll < 0.8)
            {
                // Comment
                await SimulateCommentAsync(npc);
            }
            else
            {
                // Post
                await SimulatePostAsync(npc);
            }
        }
    }
    
    private async Task SimulateLikeAsync(Domain.Entities.NPC npc)
    {
        // Get recent posts
        var posts = await _postRepository.GetRecentAsync(20);
        var postList = posts.Where(p => p.AuthorId != npc.Id).ToList();
        
        if (postList.Count > 0)
        {
            var post = postList[_random.Next(postList.Count)];
            
            // Calculate like probability based on personality
            var likeProbability = 0.5;
            if (npc.Personality != null)
            {
                likeProbability += (npc.Personality.Agreeableness - 0.5) * 0.3;
                likeProbability += (npc.Personality.Extroversion - 0.5) * 0.2;
            }
            
            if (_random.NextDouble() < likeProbability)
            {
                await _postRepository.IncrementEngagementAsync(post.Id, "like");
            }
        }
    }
    
    private async Task SimulateBrowseAsync(Domain.Entities.NPC npc)
    {
        // Get recent posts and "view" them
        var posts = await _postRepository.GetRecentAsync(5);
        foreach (var post in posts)
        {
            if (_random.NextDouble() < 0.3)
            {
                await _postRepository.IncrementEngagementAsync(post.Id, "view");
            }
        }
    }
    
    private async Task SimulateCommentAsync(Domain.Entities.NPC npc)
    {
        var posts = await _postRepository.GetRecentAsync(10);
        var postList = posts.Where(p => p.AuthorId != npc.Id).ToList();
        
        if (postList.Count > 0)
        {
            var post = postList[_random.Next(postList.Count)];
            
            // Only comment sometimes based on personality
            var commentProbability = 0.1;
            if (npc.Personality != null)
            {
                commentProbability += npc.Personality.Extroversion * 0.1;
                commentProbability += npc.Personality.Openness * 0.1;
            }
            
            if (_random.NextDouble() < commentProbability)
            {
                // Generate comment content
                var commentContent = await GenerateCommentContentAsync(npc, post.Content);
                
                var comment = new Domain.Entities.Comment
                {
                    PostId = post.Id,
                    AuthorId = npc.Id,
                    Content = commentContent
                };
                
                await _postRepository.AddCommentAsync(comment);
                await _postRepository.IncrementEngagementAsync(post.Id, "comment");
            }
        }
    }
    
    private async Task<string> GenerateCommentContentAsync(Domain.Entities.NPC npc, string postContent)
    {
        if (_aiProvider == null)
        {
            // No LLM available, use simple fallback
            var fallbacks = new[]
            {
                "Great post!",
                "I totally agree!",
                "Interesting perspective.",
                "Thanks for sharing!",
                "Well said!"
            };
            return fallbacks[_random.Next(fallbacks.Length)];
        }
        
        try
        {
            var personality = npc.Personality;
            var mood = npc.Mood;
            
            var prompt = $@"You are {npc.DisplayName}, leaving a comment on someone's post.
Post you're commenting on: ""{postContent.Substring(0, Math.Min(100, postContent.Length))}...""
Personality: {(personality?.Extroversion > 0.5 ? "outgoing" : "reserved")}, {(personality?.Agreeableness > 0.5 ? "friendly" : "critical")}, {(personality?.Humor > 0.5 ? "humorous" : "serious")}.
Current mood: {mood?.PrimaryMood ?? "neutral"}.
Generate a short reply comment (under 100 characters) that matches your personality.";

            var content = await _aiProvider.GenerateAsync(prompt);
            
            // Ensure content isn't too long
            if (content.Length > 100)
            {
                content = content.Substring(0, 97) + "...";
            }
            
            return content;
        }
        catch (Exception)
        {
            return "Interesting!";
        }
    }
    
    private async Task SimulatePostAsync(Domain.Entities.NPC npc)
    {
        // Only post based on activity level
        if (_random.NextDouble() < npc.ActivityLevel * 0.3)
        {
            var topics = npc.Interests?.Select(i => i.Topic).ToArray() ?? Array.Empty<string>();
            var topic = topics.Length > 0 ? topics[_random.Next(topics.Length)] : "general";
            
            // Generate content - either from LLM or fallback
            var content = await GeneratePostContentAsync(npc, topic);
            
            var post = new Domain.Entities.Post
            {
                AuthorId = npc.Id,
                Content = content,
                ImportanceScore = npc.ActivityLevel * 0.5
            };
            
            await _postRepository.AddAsync(post);
        }
    }
    
    private async Task<string> GeneratePostContentAsync(Domain.Entities.NPC npc, string topic)
    {
        if (_aiProvider == null)
        {
            // No LLM available, use simple fallback
            var fallbacks = new[]
            {
                $"Thinking about {topic} today.",
                $"Just posted about {topic}!",
                $"My thoughts on {topic}...",
                $"Sharing my perspective on {topic}.",
                $"Exploring {topic} today."
            };
            return fallbacks[_random.Next(fallbacks.Length)];
        }
        
        try
        {
            var personality = npc.Personality;
            var mood = npc.Mood;
            
            var prompt = $@"You are {npc.DisplayName}, a social media user.
Personality: {(personality?.Extroversion > 0.5 ? "outgoing" : "reserved")}, {(personality?.Agreeableness > 0.5 ? "friendly" : "critical")}, {(personality?.Humor > 0.5 ? "humorous" : "serious")}.
Current mood: {mood?.PrimaryMood ?? "neutral"} ({(mood?.Happiness * 100)?.ToString("0") ?? "50"}% happiness).
Interest: {topic}.
Generate a short social media post (under 200 characters) about {topic} that matches your personality and mood. Be natural and conversational.";

            var content = await _aiProvider.GenerateAsync(prompt);
            
            // Ensure content isn't too long
            if (content.Length > 280)
            {
                content = content.Substring(0, 277) + "...";
            }
            
            return content;
        }
        catch (Exception)
        {
            // Fallback on error
            return $"Sharing my thoughts on {topic} today!";
        }
    }
    
    private async Task ScheduleNextActionAsync(Domain.Entities.NPC npc)
    {
        // Calculate delay based on activity level (more active = shorter delays)
        var baseDelayMinutes = 60.0;
        var delayMinutes = baseDelayMinutes / npc.ActivityLevel;
        
        // Add randomness
        delayMinutes *= (0.5 + _random.NextDouble());
        
        var nextAction = new Domain.Entities.ScheduledAction
        {
            NPCId = npc.Id,
            ActionType = "process_npc",
            ScheduledFor = DateTimeOffset.UtcNow.AddMinutes(delayMinutes),
            Priority = (int)(npc.ActivityLevel * 100)
        };
        
        await _scheduledActionRepository.AddAsync(nextAction);
    }
    
    private async Task ExecuteScheduledActionAsync(Domain.Entities.ScheduledAction action)
    {
        switch (action.ActionType)
        {
            case "process_npc":
                await ProcessNpcAsync(action.NPCId);
                break;
                // Add more action types as needed
        }
    }
    
    public async Task SimulateOfflineAsync(TimeSpan duration)
    {
        // Calculate how many NPC actions should have happened
        var world = await _worldRepository.GetDefaultAsync();
        if (world == null) return;
        
        // Get active NPCs
        var npcs = await _npcRepository.GetActiveAsync(20);
        
        // Simulate actions for each NPC
        foreach (var npc in npcs)
        {
            if (npc.IsPlayer) continue;
            
            // Calculate expected actions based on activity level
            var expectedActions = (int)(npc.ActivityLevel * duration.TotalMinutes / 30);
            
            for (int i = 0; i < expectedActions; i++)
            {
                await ExecuteTier1ActionsAsync(npc);
            }
        }
        
        // Advance world time
        world.CurrentTime = world.CurrentTime.Add(duration);
        world.LastProcessedAt = DateTimeOffset.UtcNow;
        await _worldRepository.UpdateAsync(world);
    }
}

public interface ISimulationService
{
    Task AdvanceTimeAsync(TimeSpan delta);
    Task ProcessNpcAsync(string npcId);
    Task SimulateOfflineAsync(TimeSpan duration);
}
