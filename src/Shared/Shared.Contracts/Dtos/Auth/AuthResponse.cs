namespace Shared. Contracts.Dtos.Auth;

public record AuthResponse
{
    public long UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
}