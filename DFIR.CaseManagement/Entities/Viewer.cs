namespace DFIR.CaseManagement.Entities;

/// <summary>Read-only stakeholder.</summary>
public sealed class Viewer : User
{
    public const string RoleName = "Viewer";

    public override string Role => RoleName;

    public override IReadOnlyCollection<string> GetPermissions() => new[]
    {
        "cases:read",
        "evidence:read",
        "custody:read",
        "malware:read"
    };
}
