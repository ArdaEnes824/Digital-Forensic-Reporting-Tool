using System.ComponentModel.DataAnnotations;

namespace DFIR.CaseManagement.DTOs;

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MinLength(4)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Admin, Analyst or Viewer.</summary>
    [Required]
    public string Role { get; set; } = "Viewer";
}

public record AuthResponseDto(string Token, DateTime ExpiresAt, UserDto User);

public record UserDto(int Id, string Username, string Email, string FullName, string Role, bool IsActive);

public class LoginRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Returned on successful authentication / refresh. Carries the short-lived JWT access
/// token, a long-lived refresh token, the user profile and the resolved permission set.
/// </summary>
public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User,
    IReadOnlyCollection<string> Permissions);
