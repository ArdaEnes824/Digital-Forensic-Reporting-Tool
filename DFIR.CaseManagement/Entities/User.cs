namespace DFIR.CaseManagement.Entities;

/// <summary>
/// Abstract base for all system users (INHERITANCE root).
/// Uses EF Core Table-Per-Hierarchy: Admin / Analyst / Viewer share one table
/// distinguished by a discriminator column.
/// </summary>
public abstract class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    // ENCAPSULATION: the raw hash is stored in a private field and only exposed
    // through controlled accessors; password verification logic lives here.
    private string _passwordHash = string.Empty;
    public string PasswordHash
    {
        get => _passwordHash;
        private set => _passwordHash = value;
    }

    public bool IsActive { get; set; } = true;

    /// <summary>The role name surfaced in JWT claims. POLYMORPHISM: each subclass answers differently.</summary>
    public abstract string Role { get; }

    /// <summary>Coarse permission set per role. POLYMORPHISM.</summary>
    public abstract IReadOnlyCollection<string> GetPermissions();

    public void SetPasswordHash(string hash) => PasswordHash = hash;

    public override string Describe() => $"User[{Role}] {Username} ({Email})";
}
