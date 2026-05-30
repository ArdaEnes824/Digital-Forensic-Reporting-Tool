namespace DFIR.CaseManagement.Entities;

/// <summary>Full-control user. INHERITANCE + POLYMORPHISM.</summary>
public sealed class Admin : User
{
    public const string RoleName = "Admin";

    public override string Role => RoleName;

    public override IReadOnlyCollection<string> GetPermissions() => new[]
    {
        "cases:read", "cases:write", "cases:delete",
        "evidence:read", "evidence:write", "evidence:delete",
        "custody:read", "custody:write",
        "malware:read", "malware:write",
        "reports:generate",
        "users:manage"
    };
}
