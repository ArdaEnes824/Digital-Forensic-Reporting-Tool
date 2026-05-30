using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services;

/// <summary>
/// Service Layer for user management. Instantiates the correct User subclass
/// (Admin / Analyst / Viewer) via POLYMORPHISM and hashes passwords before persisting.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork uow, ILogger<UserService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync()
    {
        var users = await _uow.Users.GetAllAsync();
        return users.Select(Map).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        return user is null ? null : Map(user);
    }

    public async Task<UserDto> CreateAsync(RegisterDto dto)
    {
        var existing = await _uow.Users.FindAsync(u => u.Username == dto.Username);
        if (existing.Count > 0)
            throw new InvalidOperationException($"Username '{dto.Username}' is already taken.");

        var user = CreateForRole(dto.Role);
        user.Username = dto.Username;
        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.IsActive = true;
        user.SetPasswordHash(PasswordHasher.Hash(dto.Password));

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Created user {Username} with role {Role}", user.Username, user.Role);
        return Map(user);
    }

    public async Task<UserDto?> SetActiveAsync(int id, bool isActive)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user is null) return null;

        user.IsActive = isActive;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("User {Username} active state set to {State}", user.Username, isActive);
        return Map(user);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user is null) return false;

        _uow.Users.Remove(user);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Deleted user {Username} (Id {Id})", user.Username, id);
        return true;
    }

    /// <summary>Factory selecting the concrete User subclass for a role name.</summary>
    private static User CreateForRole(string role) => role?.Trim().ToLowerInvariant() switch
    {
        "admin" => new Admin(),
        "analyst" => new Analyst(),
        "viewer" => new Viewer(),
        _ => throw new InvalidOperationException($"Unknown role '{role}'. Use Admin, Analyst or Viewer.")
    };

    private static UserDto Map(User u) => new(u.Id, u.Username, u.Email, u.FullName, u.Role, u.IsActive);
}
