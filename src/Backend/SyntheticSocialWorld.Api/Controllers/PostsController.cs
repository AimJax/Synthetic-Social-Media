using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly SyntheticSocialWorldDbContext _context;

    public PostsController(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get recent posts
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetRecent([FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        var posts = await _context.Posts
            .Where(p => !p.IsDeleted)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        // Order on client side to avoid SQLite DateTimeOffset issues
        posts = posts.OrderByDescending(p => p.CreatedAt).ToList();

        // Load authors
        var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();
        var authors = await _context.NPCs
            .Where(n => authorIds.Contains(n.Id))
            .ToDictionaryAsync(n => n.Id, n => new AuthorInfo { Id = n.Id, Handle = n.Handle, DisplayName = n.DisplayName });

        var result = posts.Select(p => new PostDto
        {
            Id = p.Id,
            AuthorId = p.AuthorId,
            AuthorName = authors.GetValueOrDefault(p.AuthorId)?.DisplayName ?? "Unknown",
            AuthorHandle = authors.GetValueOrDefault(p.AuthorId)?.Handle ?? "",
            Content = p.Content,
            CommunityId = p.CommunityId,
            LikeCount = p.LikeCount,
            DislikeCount = p.DislikeCount,
            CommentCount = p.CommentCount,
            ShareCount = p.ShareCount,
            ViewCount = p.ViewCount,
            ImportanceScore = p.ImportanceScore,
            Popularity = p.Popularity,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Get post by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetById(string id)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (post == null)
            return NotFound();

        var author = await _context.NPCs.FindAsync(post.AuthorId);

        return Ok(new PostDto
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            AuthorName = author?.DisplayName ?? "Unknown",
            AuthorHandle = author?.Handle ?? "",
            Content = post.Content,
            CommunityId = post.CommunityId,
            LikeCount = post.LikeCount,
            DislikeCount = post.DislikeCount,
            CommentCount = post.CommentCount,
            ShareCount = post.ShareCount,
            ViewCount = post.ViewCount,
            ImportanceScore = post.ImportanceScore,
            Popularity = post.Popularity,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        });
    }

    /// <summary>
    /// Create a new post
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostDto dto)
    {
        var post = new Post
        {
            AuthorId = dto.AuthorId,
            Content = dto.Content,
            CommunityId = dto.CommunityId,
            ImportanceScore = dto.ImportanceScore ?? 0.5
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var author = await _context.NPCs.FindAsync(post.AuthorId);

        return CreatedAtAction(nameof(GetById), new { id = post.Id }, new PostDto
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            AuthorName = author?.DisplayName ?? "Unknown",
            AuthorHandle = author?.Handle ?? "",
            Content = post.Content,
            CommunityId = post.CommunityId,
            LikeCount = post.LikeCount,
            DislikeCount = post.DislikeCount,
            CommentCount = post.CommentCount,
            ShareCount = post.ShareCount,
            ViewCount = post.ViewCount,
            ImportanceScore = post.ImportanceScore,
            Popularity = post.Popularity,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        });
    }

    /// <summary>
    /// Get comments for a post
    /// </summary>
    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(string id, [FromQuery] int limit = 50)
    {
        var comments = await _context.Comments
            .Where(c => c.PostId == id && !c.IsDeleted)
            .Take(limit)
            .ToListAsync();

        // Order on client side
        comments = comments.OrderByDescending(c => c.CreatedAt).ToList();

        // Load authors
        var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
        var authors = await _context.NPCs
            .Where(n => authorIds.Contains(n.Id))
            .ToDictionaryAsync(n => n.Id, n => new AuthorInfo { Id = n.Id, Handle = n.Handle, DisplayName = n.DisplayName });

        var result = comments.Select(c => new CommentDto
        {
            Id = c.Id,
            PostId = c.PostId,
            AuthorId = c.AuthorId,
            AuthorName = authors.GetValueOrDefault(c.AuthorId)?.DisplayName ?? "Unknown",
            Content = c.Content,
            LikeCount = c.LikeCount,
            CreatedAt = c.CreatedAt
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Add a comment to a post
    /// </summary>
    [HttpPost("{id}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(string id, [FromBody] CreateCommentDto dto)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
            return NotFound();

        var comment = new Comment
        {
            PostId = id,
            AuthorId = dto.AuthorId,
            Content = dto.Content,
            ParentCommentId = dto.ParentCommentId
        };

        _context.Comments.Add(comment);
        
        // Increment comment count
        post.CommentCount++;
        
        await _context.SaveChangesAsync();

        var author = await _context.NPCs.FindAsync(dto.AuthorId);

        return CreatedAtAction(nameof(GetComments), new { id }, new CommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            AuthorName = author?.DisplayName ?? "Unknown",
            Content = comment.Content,
            LikeCount = comment.LikeCount,
            CreatedAt = comment.CreatedAt
        });
    }

    /// <summary>
    /// Like a post
    /// </summary>
    [HttpPost("{id}/like")]
    public async Task<ActionResult> Like(string id, [FromBody] LikeDto dto)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
            return NotFound();

        // Check for existing engagement
        var existing = await _context.PostEngagements
            .FirstOrDefaultAsync(e => e.PostId == id && e.NPCId == dto.NpcId && e.Type == "like");

        if (existing != null)
        {
            return BadRequest(new { message = "Already liked" });
        }

        var engagement = new PostEngagement
        {
            PostId = id,
            NPCId = dto.NpcId,
            Type = "like"
        };
        _context.PostEngagements.Add(engagement);
        post.LikeCount++;

        await _context.SaveChangesAsync();
        return Ok(new { likeCount = post.LikeCount });
    }

    /// <summary>
    /// Delete a post (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
            return NotFound();

        post.IsDeleted = true;
        post.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// DTOs
public class PostDto
{
    public string Id { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string AuthorHandle { get; set; } = "";
    public string Content { get; set; } = "";
    public string? CommunityId { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public int ViewCount { get; set; }
    public double ImportanceScore { get; set; }
    public double Popularity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CommentDto
{
    public string Id { get; set; } = "";
    public string PostId { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string Content { get; set; } = "";
    public int LikeCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class AuthorInfo
{
    public string Id { get; set; } = "";
    public string Handle { get; set; } = "";
    public string DisplayName { get; set; } = "";
}

public class CreatePostDto
{
    public string AuthorId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CommunityId { get; set; }
    public double? ImportanceScore { get; set; }
}

public class CreateCommentDto
{
    public string AuthorId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ParentCommentId { get; set; }
}

public class LikeDto
{
    public string NpcId { get; set; } = string.Empty;
}
