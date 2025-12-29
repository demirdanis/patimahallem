using Microsoft.AspNetCore.Authorization;
using Microsoft. AspNetCore.Mvc;
using Shared.Contracts. Dtos.Auth;
using User.Service.Services;

namespace User.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Yeni kullanıcı kaydı
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _userService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetMe), new { }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Kullanıcı girişi
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _userService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger. LogError(ex, "Error during login");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Giriş yapmış kullanıcının bilgileri
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes. NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid token" });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                fullName = user.FullName,
                phone = user.Phone,
                isActive = user.IsActive,
                roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                createdAt = user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user info");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}