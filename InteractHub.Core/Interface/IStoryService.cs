using InteractHub.Core.DTOs.Stories;

namespace InteractHub.Core.Interfaces;

public interface IStoryService
{
    Task<List<StoryResponseDto>> GetStoriesAsync(string userId);
    Task<StoryResponseDto> CreateStoryAsync(string userId, CreateStoryDto dto);
    Task<bool> DeleteStoryAsync(int storyId, string userId);
}