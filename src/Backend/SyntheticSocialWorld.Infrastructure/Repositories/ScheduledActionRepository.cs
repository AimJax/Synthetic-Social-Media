using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Domain.Interfaces;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Infrastructure.Repositories;

public class ScheduledActionRepository : IScheduledActionRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public ScheduledActionRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ScheduledAction>> GetDueActionsAsync(DateTimeOffset asOf)
    {
        // Fetch all non-executed actions and filter on client side (SQLite limitation)
        var allActions = await _context.ScheduledActions.ToListAsync();
        return allActions
            .Where(a => !a.IsExecuted && a.ScheduledFor <= asOf)
            .OrderBy(a => a.Priority)
            .ThenBy(a => a.ScheduledFor)
            .ToList();
    }

    public async Task<ScheduledAction?> GetNextActionAsync()
    {
        // Fetch all non-executed actions and filter on client side (SQLite limitation)
        var allActions = await _context.ScheduledActions.ToListAsync();
        return allActions
            .Where(a => !a.IsExecuted)
            .OrderBy(a => a.Priority)
            .ThenBy(a => a.ScheduledFor)
            .FirstOrDefault();
    }

    public async Task<ScheduledAction> AddAsync(ScheduledAction action)
    {
        _context.ScheduledActions.Add(action);
        await _context.SaveChangesAsync();
        return action;
    }

    public async Task UpdateAsync(ScheduledAction action)
    {
        _context.ScheduledActions.Update(action);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ScheduledAction>> GetByNpcAsync(string npcId)
    {
        // Fetch all and filter on client side (SQLite limitation)
        var allActions = await _context.ScheduledActions.ToListAsync();
        return allActions
            .Where(a => a.NPCId == npcId && !a.IsExecuted)
            .OrderBy(a => a.ScheduledFor)
            .ToList();
    }

    public async Task CancelByNpcAsync(string npcId, string actionType)
    {
        // Fetch all and filter on client side (SQLite limitation)
        var allActions = await _context.ScheduledActions.ToListAsync();
        var actions = allActions
            .Where(a => a.NPCId == npcId && a.ActionType == actionType && !a.IsExecuted)
            .ToList();

        foreach (var action in actions)
        {
            action.IsExecuted = true;
        }

        await _context.SaveChangesAsync();
    }
}

public class MemoryRepository : IMemoryRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public MemoryRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<EpisodicMemory> AddMemoryAsync(EpisodicMemory memory)
    {
        _context.EpisodicMemories.Add(memory);
        await _context.SaveChangesAsync();
        return memory;
    }

    public async Task<IEnumerable<EpisodicMemory>> GetMemoriesForNpcAsync(string npcId, int limit = 50)
    {
        return await _context.EpisodicMemories
            .Where(m => m.OwnerId == npcId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<EpisodicMemory>> GetRelevantMemoriesAsync(string npcId, string? targetNpcId, IEnumerable<string>? topics, int limit = 20)
    {
        var query = _context.EpisodicMemories
            .Where(m => m.OwnerId == npcId);

        return await query
            .OrderByDescending(m => m.Timestamp)
            .ThenByDescending(m => m.Importance)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<SemanticBelief?> GetBeliefAsync(string npcId, string subject)
    {
        return await _context.SemanticBeliefs
            .FirstOrDefaultAsync(b => b.OwnerId == npcId && b.Subject == subject);
    }

    public async Task<SemanticBelief> AddBeliefAsync(SemanticBelief belief)
    {
        _context.SemanticBeliefs.Add(belief);
        await _context.SaveChangesAsync();
        return belief;
    }

    public async Task UpdateBeliefAsync(SemanticBelief belief)
    {
        belief.UpdatedAt = DateTimeOffset.UtcNow;
        _context.SemanticBeliefs.Update(belief);
        await _context.SaveChangesAsync();
    }

    public async Task ProcessDecayAsync(string npcId)
    {
        // Decay old memories
        var oldMemories = await _context.EpisodicMemories
            .Where(m => m.OwnerId == npcId && m.Timestamp < DateTimeOffset.UtcNow.AddDays(-30))
            .ToListAsync();

        foreach (var memory in oldMemories)
        {
            memory.Importance = Math.Max(0, memory.Importance - 0.1);
            if (memory.Importance <= 0)
            {
                _context.EpisodicMemories.Remove(memory);
            }
        }

        await _context.SaveChangesAsync();
    }
}
