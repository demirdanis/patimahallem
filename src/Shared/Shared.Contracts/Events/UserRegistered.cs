namespace Shared. Contracts.Events;

public record UserRegistered
{
    public long UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}