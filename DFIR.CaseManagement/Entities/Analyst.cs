namespace DFIR.CaseManagement.Entities;

/// <summary>Investigator who can work cases and evidence but not manage users.</summary>
public sealed class Analyst : User
{
    public const string RoleName = "Analyst";

    public override string Role => RoleName;

    public override IReadOnlyCollection<string> GetPermissions() => new[]
    {
        "cases:read", "cases:write",
        "evidence:read", "evidence:write",
        "custody:read", "custody:write",
        "malware:read", "malware:write",
        "reports:generate"
    };
}
