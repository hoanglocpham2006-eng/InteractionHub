using InteractHub.Core.DTOs.Friends;
using InteractHub.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InteractHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET /api/friends
    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var friends = await _friendService.GetFriendsAsync(GetCurrentUserId());
        return Ok(friends);
    }

    // GET /api/friends/pending
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var requests = await _friendService.GetPendingRequestsAsync(GetCurrentUserId());
        return Ok(requests);
    }

    // POST /api/friends/request
    [HttpPost("request")]
    public async Task<IActionResult> SendRequest([FromBody] FriendRequestDto dto)
    {
        if (dto.ReceiverId == GetCurrentUserId())
            return BadRequest(new { message = "Không thể kết bạn với chính mình" });

        var result = await _friendService.SendFriendRequestAsync(
            GetCurrentUserId(), dto.ReceiverId);

        if (result == null)
            return BadRequest(new { message = "Đã gửi lời mời hoặc đã là bạn bè" });

        return Ok(result);
    }

    // PUT /api/friends/accept/5
    [HttpPut("accept/{friendshipId}")]
    public async Task<IActionResult> AcceptRequest(int friendshipId)
    {
        var result = await _friendService.AcceptFriendRequestAsync(
            friendshipId, GetCurrentUserId());

        if (result == null)
            return NotFound(new { message = "Không tìm thấy lời mời" });

        return Ok(result);
    }

    // PUT /api/friends/decline/5
    [HttpPut("decline/{friendshipId}")]
    public async Task<IActionResult> DeclineRequest(int friendshipId)
    {
        var result = await _friendService.DeclineFriendRequestAsync(
            friendshipId, GetCurrentUserId());

        if (result == null)
            return NotFound(new { message = "Không tìm thấy lời mời" });

        return Ok(result);
    }

    // DELETE /api/friends/{friendId}
    [HttpDelete("{friendId}")]
    public async Task<IActionResult> Unfriend(string friendId)
    {
        var result = await _friendService.UnfriendAsync(GetCurrentUserId(), friendId);
        if (!result)
            return NotFound(new { message = "Không tìm thấy kết bạn" });

        return Ok(new { message = "Hủy kết bạn thành công" });
    }

    // GET /api/friends/status/{userId}
    [HttpGet("status/{userId}")]
    public async Task<IActionResult> GetFriendshipStatus(string userId)
    {
        var status = await _friendService.GetFriendshipStatusAsync(
            GetCurrentUserId(), userId);
        return Ok(new { status });
    }
}