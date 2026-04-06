using InteractHub.Core.DTOs.Stories;
using InteractHub.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InteractHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoriesController : ControllerBase
{
    private readonly IStoryService _storyService;

    public StoriesController(IStoryService storyService)
    {
        _storyService = storyService;
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET /api/stories
    [HttpGet]
    public async Task<IActionResult> GetStories()
    {
        var stories = await _storyService.GetStoriesAsync(GetCurrentUserId());
        return Ok(stories);
    }

    // POST /api/stories
    [HttpPost]
    public async Task<IActionResult> CreateStory([FromBody] CreateStoryDto dto)
    {
        var story = await _storyService.CreateStoryAsync(GetCurrentUserId(), dto);
        return Ok(story);
    }

    // DELETE /api/stories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStory(int id)
    {
        var result = await _storyService.DeleteStoryAsync(id, GetCurrentUserId());
        if (!result)
            return NotFound(new { message = "Không tìm thấy story hoặc không có quyền" });

        return Ok(new { message = "Xóa story thành công" });
    }
}