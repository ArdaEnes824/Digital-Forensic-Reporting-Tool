using DFIR.CaseManagement.Auth;
using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;
using Microsoft.Extensions.Options;

namespace DFIR.CaseManagement.Services;

/// <summary>
/// Authentication service: verifies credentials (PBKDF2), issues JWT access tokens with
/// role + permission claims, manages single-use refresh tokens, and handles user CRUD
/// for the User Management module.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStore _refreshTokens;
    private readonly JwtSettings _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork uow,
        ITokenService tokenService,
        IRefreshTokenStore refreshTokens,
        IOptions<JwtSettings> jwt,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _tokenService = tokenService;
        _refreshTokens = refreshTokens;
        _jwt = jwt.Value;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
    {
        var user = await FindByUsernameAsync(request.Username);
        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("Login denied for '{Username}': user not found or inactive", request.Username);
            return null;
        }

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login denied for '{Username}': bad password", request.Username);
            return null;
        }

        _logger.LogInformation("User '{Username}' authenticated as {Role}", user.Username, user.Role);
        return IssueTokens(user);
    }

    public async Task<LoginResponseDto?> RefreshAsync(RefreshRequestDto request)
    {
        if (!_refreshTokens.TryConsume(request.RefreshToken, out var userId))
        {
            _logger.LogWarning("Refresh denied: invalid or expired refresh token");
            return null;
        }

        var user = await _uow.Users.GetByIdAsync(userId);
        if (user is null || !user.IsActive) return null;

        _logger.LogInformation("Refreshed tokens for user '{Username}'", user.Username);
        return IssueTokens(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await AuthenticateAsync(new LoginRequestDto { Username = dto.Username, Password = dto.Password });
        return response is null ? null : new AuthResponseDto(response.AccessToken, response.ExpiresAt, response.User);
    }

    public async Task<UserDto?> RegisterAsync(RegisterDto dto)
    {
        var existing = await FindByUsernameAsync(dto.Username);
        if (existing is not null)
        {
            _logger.LogWarning("Registration failed: username '{Username}' already exists", dto.Username);
            return null;
        }

        var user = CreateForRole(dto.Role);
        user.Username = dto.Username;
        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.IsActive = true;
        user.SetPasswordHash(PasswordHasher.Hash(dto.Password));

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Registered user '{Username}' as {Role}", user.Username, user.Role);
        return Map(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync()
    {
        var users = await _uow.Users.GetAllAsync();
        return users.Select(Map).ToList();
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user is null) return false;

        _uow.Users.Remove(user);
        await _uow.SaveChangesAsync();
        _refreshTokens.RevokeForUser(id);

        _logger.LogInformation("Deleted user '{Username}' (Id {Id})", user.Username, id);
        return true;
    }

    private LoginResponseDto IssueTokens(User user)
    {
        var (accessToken, expiresAt) = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.CreateRefreshToken();
        _refreshTokens.Store(refreshToken, user.Id, DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays));

        return new LoginResponseDto(accessToken, refreshToken, expiresAt, Map(user), user.GetPermissions());
    }

    private async Task<User?> FindByUsernameAsync(string username)
    {
        var matches = await _uow.Users.FindAsync(u => u.Username == username);
        return matches.FirstOrDefault();
    }

    private static User CreateForRole(string role) => role?.Trim().ToLowerInvariant() switch
    {
        "admin" => new Admin(),
        "analyst" => new Analyst(),
        "viewer" => new Viewer(),
        _ => throw new InvalidOperationException($"Unknown role '{role}'. Use Admin, Analyst or Viewer.")
    };

    private static UserDto Map(User u) => new(u.Id, u.Username, u.Email, u.FullName, u.Role, u.IsActive);
}
