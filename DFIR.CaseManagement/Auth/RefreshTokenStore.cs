using System.Collections.Concurrent;

namespace DFIR.CaseManagement.Auth;

public interface IRefreshTokenStore
{
    void Store(string refreshToken, int userId, DateTime expiresUtc);

    /// <summary>Validates and consumes (single-use rotation) a refresh token, returning the owning user id.</summary>
    bool TryConsume(string refreshToken, out int userId);

    void RevokeForUser(int userId);
}

/// <summary>
/// In-memory refresh token store. Tokens are single-use (rotated on every refresh) and
/// expire after the configured window. Suitable for development / coursework; a production
/// system would persist these in a database or distributed cache.
/// </summary>
public class RefreshTokenStore : IRefreshTokenStore
{
    private record Entry(int UserId, DateTime ExpiresUtc);

    private readonly ConcurrentDictionary<string, Entry> _tokens = new();

    public void Store(string refreshToken, int userId, DateTime expiresUtc)
        => _tokens[refreshToken] = new Entry(userId, expiresUtc);

    public bool TryConsume(string refreshToken, out int userId)
    {
        userId = 0;
        if (string.IsNullOrEmpty(refreshToken)) return false;
        if (!_tokens.TryRemove(refreshToken, out var entry)) return false;
        if (entry.ExpiresUtc < DateTime.UtcNow) return false;

        userId = entry.UserId;
        return true;
    }

    public void RevokeForUser(int userId)
    {
        foreach (var pair in _tokens.Where(p => p.Value.UserId == userId).ToList())
            _tokens.TryRemove(pair.Key, out _);
    }
}
