using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunitiesController : ControllerBase
{
    private readonly SyntheticSocialWorldDbContext _context;
    
    public CommunitiesController(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Get all communities
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CommunityDto>>> GetCommunities()
    {
        var communities = await _context.Communities
            .OrderByDescending(c => c.Popularity)
            .Select(c => new CommunityDto
            {
                Id = c.Id,
                Handle = c.Handle,
                Name = c.Name,
                Description = c.Description,
                Topic = c.Topic,
                MemberCount = c.MemberCount,
                Popularity = c.Popularity,
                CultureScore = c.CultureScore,
                CreatedAt = c.CreatedAt.ToString("O")
            })
            .ToListAsync();
        
        return communities;
    }
    
    /// <summary>
    /// Get community by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CommunityDto>> GetCommunity(string id)
    {
        var community = await _context.Communities
            .Where(c => c.Id == id)
            .Select(c => new CommunityDto
            {
                Id = c.Id,
                Handle = c.Handle,
                Name = c.Name,
                Description = c.Description,
                Topic = c.Topic,
                MemberCount = c.MemberCount,
                Popularity = c.Popularity,
                CultureScore = c.CultureScore,
                CreatedAt = c.CreatedAt.ToString("O")
            })
            .FirstOrDefaultAsync();
        
        if (community == null)
            return NotFound();
        
        return community;
    }
    
    /// <summary>
    /// Get community members
    /// </summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<List<MemberDto>>> GetCommunityMembers(string id)
    {
        var members = await _context.CommunityMembers
            .Where(cm => cm.CommunityId == id)
            .Include(cm => cm.NPC)
            .Select(cm => new MemberDto
            {
                Id = cm.NPC != null ? cm.NPC.Id : "",
                Handle = cm.NPC != null ? cm.NPC.Handle : "",
                DisplayName = cm.NPC != null ? cm.NPC.DisplayName : "",
                JoinedAt = cm.JoinedAt.ToString("O")
            })
            .ToListAsync();
        
        return members;
    }
}

public class CommunityDto
{
    public string Id { get; set; } = "";
    public string Handle { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Topic { get; set; }
    public int MemberCount { get; set; }
    public double Popularity { get; set; }
    public double CultureScore { get; set; }
    public string CreatedAt { get; set; } = "";
}

public class MemberDto
{
    public string Id { get; set; } = "";
    public string Handle { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string JoinedAt { get; set; } = "";
}
