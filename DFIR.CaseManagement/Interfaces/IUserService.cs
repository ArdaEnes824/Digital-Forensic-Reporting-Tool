using DFIR.CaseManagement.DTOs;

namespace DFIR.CaseManagement.Interfaces;

/// <summary>
/// User management service. Handles CRUD over the User inheritance tree
/// (Admin / Analyst / Viewer) with password hashing. Authentication / JWT issuance
/// is handled separately by IAuthService.
/// </summary>
public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(RegisterDto dto);
    Task<UserDto?> SetActiveAsync(int id, bool isActive);
    Task<bool> DeleteAsync(int id);
}
