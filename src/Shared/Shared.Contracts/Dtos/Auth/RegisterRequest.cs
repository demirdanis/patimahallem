namespace Shared.Contracts.Dtos.Auth;

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string?  Phone { get; init; }
}