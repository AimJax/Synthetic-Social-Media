using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Domain.Interfaces;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Infrastructure.Repositories;

public class NpcRepository : INpcRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public NpcRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<NPC?> GetByIdAsync(string id)
    {
        return await _context.NPCs
            .Include(n => n.Personality)
            .Include(n => n.Mood)
            .Include(n => n.Interests)
            .Include(n => n.Goals)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<NPC?> GetByHandleAsync(string handle)
    {
        return await _context.NPCs
            .Include(n => n.Personality)
            .Include(n => n.Mood)
            .FirstOrDefaultAsync(n => n.Handle == handle);
    }

    public async Task<IEnumerable<NPC>> GetAllAsync()
    {
        return await _context.NPCs.ToListAsync();
    }

    public async Task<IEnumerable<NPC>> GetActiveAsync(int count)
    {
        // SQLite doesn't support DateTimeOffset in ORDER BY, so we use client-side evaluation
        var npcs = await _context.NPCs
            .Where(n => !n.IsPlayer)
            .Take(count * 2) // Take more to account for client-side filtering
            .ToListAsync();
        
        return npcs
            .OrderByDescending(n => n.LastActiveAt)
            .Take(count)
            .ToList();
    }

    public async Task<IEnumerable<NPC>> GetNeighborsAsync(string npcId, int depth = 1)
    {
        // Get NPCs connected through follows
        var neighborIds = new HashSet<string>();
        var frontier = new Queue<string>();
        frontier.Enqueue(npcId);
        var visited = new HashSet<string> { npcId };

        while (frontier.Count > 0 && depth > 0)
        {
            var current = frontier.Dequeue();
            
            // Get followers
            var followerIds = await _context.Follows
                .Where(f => f.FollowedId == current)
                .Select(f => f.FollowerId)
                .ToListAsync();

            // Get following
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == current)
                .Select(f => f.FollowedId)
                .ToListAsync();

            foreach (var id in followerIds.Concat(followingIds))
            {
                if (!visited.Contains(id))
                {
                    neighborIds.Add(id);
                    visited.Add(id);
                    frontier.Enqueue(id);
                }
            }

            if (frontier.Count == 0)
                depth--;
        }

        // Client-side filtering for DateTimeOffset support
        var npcs = await _context.NPCs
            .ToListAsync();
        
        return npcs.Where(n => neighborIds.Contains(n.Id)).ToList();
    }

    public async Task<NPC> AddAsync(NPC npc)
    {
        _context.NPCs.Add(npc);
        await _context.SaveChangesAsync();
        return npc;
    }

    public async Task UpdateAsync(NPC npc)
    {
        npc.UpdatedAt = DateTimeOffset.UtcNow;
        _context.NPCs.Update(npc);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var npc = await _context.NPCs.FindAsync(id);
        if (npc != null)
        {
            _context.NPCs.Remove(npc);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.NPCs.AnyAsync(n => n.Id == id);
    }
}
