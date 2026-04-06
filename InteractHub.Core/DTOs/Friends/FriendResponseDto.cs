namespace InteractHub.Core.DTOs.Friends;

public class FriendResponseDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderUsername { get; set; } = string.Empty;
    public string SenderFullName { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverUsername { get; set; } = string.Empty;
    public string ReceiverFullName { get; set; } = string.Empty;
    public string? ReceiverAvatarUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}