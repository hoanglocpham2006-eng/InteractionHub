using InteractHub.Core.DTOs.Friends;
using InteractHub.Core.Entities;
using InteractHub.Core.Interfaces;
using InteractHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Infrastructure.Services;

public class FriendService : IFriendService
{
    private readonly AppDbContext _context;

    public FriendService(AppDbContext context)
    {
        _context = context;
    }

    // Gửi lời mời kết bạn
   public async Task<FriendResponseDto?> SendFriendRequestAsync(string senderId, string receiverId)
    {
        // Kiểm tra receiver có tồn tại không
        var receiverExists = await _context.Users.AnyAsync(u => u.Id == receiverId);
        if (!receiverExists)
            throw new Exception("Receiver user does not exist.");

        // Kiểm tra sender có tồn tại không
        var senderExists = await _context.Users.AnyAsync(u => u.Id == senderId);
        if (!senderExists)
            throw new Exception("Sender user does not exist.");

        // Kiểm tra trùng lặp request
        var existing = await _context.Friendships
            .FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == receiverId);
        if (existing != null)
            throw new Exception("Friend request already sent.");

        var friendship = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendshipStatus.Pending
        };

        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();

        return await GetFriendshipDtoAsync(friendship.Id);
    }
    // Chấp nhận lời mời
    public async Task<FriendResponseDto?> AcceptFriendRequestAsync(
        int friendshipId, string userId)
    {
        var friendship = await _context.Friendships.FirstOrDefaultAsync(
            f => f.Id == friendshipId && f.ReceiverId == userId);

        if (friendship == null) return null;

        friendship.Status = FriendshipStatus.Accepted;
        friendship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetFriendshipDtoAsync(friendshipId);
    }

    // Từ chối lời mời
    public async Task<FriendResponseDto?> DeclineFriendRequestAsync(
        int friendshipId, string userId)
    {
        var friendship = await _context.Friendships.FirstOrDefaultAsync(
            f => f.Id == friendshipId && f.ReceiverId == userId);

        if (friendship == null) return null;

        friendship.Status = FriendshipStatus.Declined;
        friendship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetFriendshipDtoAsync(friendshipId);
    }

    // Hủy kết bạn
    public async Task<bool> UnfriendAsync(string userId, string friendId)
    {
        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            (f.SenderId == userId && f.ReceiverId == friendId) ||
            (f.SenderId == friendId && f.ReceiverId == userId));

        if (friendship == null) return false;

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync();
        return true;
    }

    // Lấy danh sách bạn bè
    public async Task<List<FriendResponseDto>> GetFriendsAsync(string userId)
    {
        var friendships = await _context.Friendships
            .Include(f => f.Sender)
            .Include(f => f.Receiver)
            .Where(f =>
                (f.SenderId == userId || f.ReceiverId == userId) &&
                f.Status == FriendshipStatus.Accepted)
            .ToListAsync();

        return friendships.Select(f => MapToDto(f)).ToList();
    }

    // Lấy danh sách lời mời đang chờ
    public async Task<List<FriendResponseDto>> GetPendingRequestsAsync(string userId)
    {
        var friendships = await _context.Friendships
            .Include(f => f.Sender)
            .Include(f => f.Receiver)
            .Where(f =>
                f.ReceiverId == userId &&
                f.Status == FriendshipStatus.Pending)
            .ToListAsync();

        return friendships.Select(f => MapToDto(f)).ToList();
    }

    // Kiểm tra trạng thái friendship
    public async Task<string> GetFriendshipStatusAsync(
        string userId, string otherUserId)
    {
        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            (f.SenderId == userId && f.ReceiverId == otherUserId) ||
            (f.SenderId == otherUserId && f.ReceiverId == userId));

        if (friendship == null) return "None";
        return friendship.Status.ToString();
    }

    // Helper: lấy friendship theo ID
    private async Task<FriendResponseDto?> GetFriendshipDtoAsync(int friendshipId)
    {
        var f = await _context.Friendships
            .Include(f => f.Sender)
            .Include(f => f.Receiver)
            .FirstOrDefaultAsync(f => f.Id == friendshipId);

        if (f == null) return null;
        return MapToDto(f);
    }

    // Helper: map entity sang DTO
    private FriendResponseDto MapToDto(Friendship f) => new()
    {
        Id = f.Id,
        SenderId = f.SenderId,
        SenderUsername = f.Sender.UserName!,
        SenderFullName = f.Sender.FullName,
        SenderAvatarUrl = f.Sender.AvatarUrl,
        ReceiverId = f.ReceiverId,
        ReceiverUsername = f.Receiver.UserName!,
        ReceiverFullName = f.Receiver.FullName,
        ReceiverAvatarUrl = f.Receiver.AvatarUrl,
        Status = f.Status.ToString(),
        CreatedAt = f.CreatedAt
    };
}