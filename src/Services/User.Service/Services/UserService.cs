using MassTransit;
using Microsoft. EntityFrameworkCore;
using Shared.Contracts.Dtos.Auth;
using Shared. Contracts.Events;
using Shared.Infrastructure. Jwt;
using User.Service.Data;
using BCrypt.Net;

namespace User. Service.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UserService> _logger;

    public UserService(
        AppDbContext context,
        JwtService jwtService,
        IPublishEndpoint publishEndpoint,
        ILogger<UserService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Email kontrolü
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Yeni kullanıcı oluştur
        var user = new Shared.Domain.Entities. User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net. BCrypt.HashPassword(request. Password),
            FullName = request.FullName,
            Phone = request.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Default role ata (bagisci)
        var defaultRole = await _context.Roles
            . FirstOrDefaultAsync(r => r.Name == "bagisci");

        if (defaultRole != null)
        {
            var userRole = new Shared.Domain.Entities.UserRole
            {
                UserId = user. Id,
                RoleId = (int)defaultRole.Id,
                CreatedAt = DateTime. UtcNow
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        // Rolleri yükle
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user. Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role. Name)
            .ToListAsync();

        // JWT token oluştur
        var token = _jwtService.GenerateToken(
            user.Id,
            user.Email,
            user.FullName,
            roles
        );

        // Event yayınla (async)
        try
        {
            await _publishEndpoint.Publish(new UserRegistered
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                RegisteredAt = DateTime.UtcNow
            });

            _logger.LogInformation("User registered event published for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish user registered event for user {UserId}", user.Id);
        }

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Token = token,
            Roles = roles
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Kullanıcıyı bul
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Şifre kontrolü
        if (!BCrypt. Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Rolleri yükle
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        // JWT token oluştur
        var token = _jwtService.GenerateToken(
            user.Id,
            user. Email,
            user.FullName,
            roles
        );

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Token = token,
            Roles = roles
        };
    }

    public async Task<Shared.Domain.Entities.User?> GetUserByIdAsync(long userId)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<Shared.Domain.Entities. User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}