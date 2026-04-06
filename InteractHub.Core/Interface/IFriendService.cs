using InteractHub.Core.DTOs.Friends;

namespace InteractHub.Core.Interfaces;

public interface IFriendService
{
    Task<FriendResponseDto?> SendFriendRequestAsync(string senderId, string receiverId);
    Task<FriendResponseDto?> AcceptFriendRequestAsync(int friendshipId, string userId);
    Task<FriendResponseDto?> DeclineFriendRequestAsync(int friendshipId, string userId);
    Task<bool> UnfriendAsync(string userId, string friendId);
    Task<List<FriendResponseDto>> GetFriendsAsync(string userId);
    Task<List<FriendResponseDto>> GetPendingRequestsAsync(string userId);
    Task<string> GetFriendshipStatusAsync(string userId, string otherUserId);
}