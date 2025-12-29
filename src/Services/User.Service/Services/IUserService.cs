using Shared. Contracts. Dtos. Auth;

namespace User.Service.Services;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<Shared.Domain.Entities.User? > GetUserByIdAsync(long userId);
    Task<Shared.Domain.Entities.User?> GetUserByEmailAsync(string email);
}