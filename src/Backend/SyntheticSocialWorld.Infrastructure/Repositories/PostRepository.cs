using Microsoft.EntityFrameworkCore;
using SyntheticSocialWorld.Domain.Entities;
using SyntheticSocialWorld.Domain.Interfaces;
using SyntheticSocialWorld.Infrastructure.Data;

namespace SyntheticSocialWorld.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly SyntheticSocialWorldDbContext _context;

    public PostRepository(SyntheticSocialWorldDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(string id)
    {
        return await _context.Posts
            .Include(p => p.Community)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<IEnumerable<Post>> GetByAuthorAsync(string authorId, int limit = 50, int offset = 0)
    {
        // SQLite doesn't support DateTimeOffset in ORDER BY, so we use client-side evaluation
        var posts = await _context.Posts
            .Where(p => p.AuthorId == authorId && !p.IsDeleted)
            .Take((limit + offset) * 2)
            .ToListAsync();
        
        return posts
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToList();
    }

    public async Task<IEnumerable<Post>> GetByCommunityAsync(string communityId, int limit = 50, int offset = 0)
    {
        // SQLite doesn't support DateTimeOffset in ORDER BY, so we use client-side evaluation
        var posts = await _context.Posts
            .Where(p => p.CommunityId == communityId && !p.IsDeleted)
            .Take((limit + offset) * 2)
            .ToListAsync();
        
        return posts
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToList();
    }

    public async Task<IEnumerable<Post>> GetRecentAsync(int count)
    {
        // SQLite doesn't support DateTimeOffset in ORDER BY, so we use client-side evaluation
        var posts = await _context.Posts
            .Where(p => !p.IsDeleted)
            .Take(count * 2)
            .ToListAsync();
        
        return posts
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToList();
    }

    public async Task<Post> AddAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task UpdateAsync(Post post)
    {
        post.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            post.IsDeleted = true;
            post.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementEngagementAsync(string postId, string engagementType)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post != null)
        {
            switch (engagementType.ToLower())
            {
                case "like":
                    post.LikeCount++;
                    break;
                case "dislike":
                    post.DislikeCount++;
                    break;
                case "share":
                    post.ShareCount++;
                    break;
                case "comment":
                    post.CommentCount++;
                    break;
                case "view":
                    post.ViewCount++;
                    break;
            }
            post.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
