using DFIR.CaseManagement.Entities;

namespace DFIR.CaseManagement.Auth;

/// <summary>Issues JWT access tokens (with role + permission claims) and opaque refresh tokens.</summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user);
    string CreateRefreshToken();
}
