using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly SyntheticSocialWorldDbContext _context;
    
    public SearchController(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<SearchResultsResponse>> Search(
        [FromQuery] string query,
        [FromQuery] string? filter = null,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return BadRequest(new { error = "Query must be at least 2 characters" });
        }
        
        var results = new SearchResultsResponse();
        var searchTerm = query.ToLower();
        
        // Search NPCs
        if (filter == null || filter == "npcs")
        {
            var npcs = await _context.NPCs
                .Where(n => !n.IsPlayer && (
                    n.DisplayName.ToLower().Contains(searchTerm) ||
                    n.Handle.ToLower().Contains(searchTerm) ||
                    (n.Bio != null && n.Bio.ToLower().Contains(searchTerm))))
                .Take(limit)
                .ToListAsync();
            
            results.Npcs = npcs.Select(n => new NpcSearchResult
            {
                Id = n.Id,
                Handle = n.Handle,
                DisplayName = n.DisplayName,
                Bio = n.Bio,
                AvatarUrl = n.AvatarUrl,
                FollowerCount = n.FollowerCount,
                Popularity = n.Popularity
            }).ToList();
        }
        
        // Search Posts
        if (filter == null || filter == "posts")
        {
            var postsQuery = await _context.Posts
                .Where(p => !p.IsDeleted && p.Content.ToLower().Contains(searchTerm))
                .Take(limit * 2) // Take more since we filter in memory
                .ToListAsync();
            
            results.Posts = postsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .Select(p => new PostSearchResult
                {
                    Id = p.Id,
                    Content = p.Content,
                    AuthorId = p.AuthorId,
                    AuthorName = "Unknown",
                    AuthorHandle = "unknown",
                    LikeCount = p.LikeCount,
                    CommentCount = p.CommentCount,
                    CreatedAt = p.CreatedAt.ToString("O")
                })
                .ToList();
        }
        
        // Search Communities
        if (filter == null || filter == "communities")
        {
            results.Communities = await _context.Communities
                .Where(c => 
                    c.Name.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm)))
                .Take(limit)
                .Select(c => new CommunitySearchResult
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    MemberCount = c.MemberCount,
                    Topic = c.Topic
                })
                .ToListAsync();
        }
        
        return Ok(results);
    }
}

public class SearchResultsResponse
{
    public List<NpcSearchResult> Npcs { get; set; } = new();
    public List<PostSearchResult> Posts { get; set; } = new();
    public List<CommunitySearchResult> Communities { get; set; } = new();
}

public class NpcSearchResult
{
    public string Id { get; set; } = "";
    public string Handle { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public int FollowerCount { get; set; }
    public double Popularity { get; set; }
}

public class PostSearchResult
{
    public string Id { get; set; } = "";
    public string Content { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string AuthorHandle { get; set; } = "";
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public string CreatedAt { get; set; } = "";
}

public class CommunitySearchResult
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int MemberCount { get; set; }
    public string? Topic { get; set; }
}
