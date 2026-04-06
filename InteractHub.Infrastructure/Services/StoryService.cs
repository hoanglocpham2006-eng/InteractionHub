using InteractHub.Core.DTOs.Stories;
using InteractHub.Core.Entities;
using InteractHub.Core.Interfaces;
using InteractHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Infrastructure.Services;

public class StoryService : IStoryService
{
    private readonly AppDbContext _context;

    public StoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StoryResponseDto>> GetStoriesAsync(string userId)
    {
        return await _context.Stories
            .Include(s => s.User)
            .Where(s => s.ExpiresAt > DateTime.UtcNow) // chỉ lấy story chưa hết hạn
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StoryResponseDto
            {
                Id = s.Id,
                Content = s.Content,
                ImageUrl = s.ImageUrl,
                BackgroundColor = s.BackgroundColor,
                CreatedAt = s.CreatedAt,
                ExpiresAt = s.ExpiresAt,
                UserId = s.UserId,
                Username = s.User.UserName!,
                AvatarUrl = s.User.AvatarUrl
            })
            .ToListAsync();
    }

    public async Task<StoryResponseDto> CreateStoryAsync(string userId, CreateStoryDto dto)
    {
        var story = new Story
        {
            Content = dto.Content,
            ImageUrl = dto.ImageUrl,
            BackgroundColor = dto.BackgroundColor,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return new StoryResponseDto
        {
            Id = story.Id,
            Content = story.Content,
            ImageUrl = story.ImageUrl,
            BackgroundColor = story.BackgroundColor,
            CreatedAt = story.CreatedAt,
            ExpiresAt = story.ExpiresAt,
            UserId = userId,
            Username = user!.UserName!,
            AvatarUrl = user.AvatarUrl
        };
    }

    public async Task<bool> DeleteStoryAsync(int storyId, string userId)
    {
        var story = await _context.Stories.FirstOrDefaultAsync(
            s => s.Id == storyId && s.UserId == userId);

        if (story == null) return false;

        _context.Stories.Remove(story);
        await _context.SaveChangesAsync();
        return true;
    }
}