namespace DFIR.CaseManagement.Auth;

/// <summary>
/// Permission strings embedded as claims in the JWT (see User.GetPermissions) and the
/// matching authorization policy names. Read access is granted to any authenticated user;
/// these policies gate the write / delete / management operations.
/// </summary>
public static class Permissions
{
    public const string CasesWrite = "cases:write";
    public const string CasesDelete = "cases:delete";
    public const string EvidenceWrite = "evidence:write";
    public const string EvidenceDelete = "evidence:delete";
    public const string CustodyWrite = "custody:write";
    public const string MalwareWrite = "malware:write";
    public const string ReportsGenerate = "reports:generate";
    public const string UsersManage = "users:manage";

    /// <summary>All permissions, used to register one policy per permission claim.</summary>
    public static readonly string[] All =
    {
        CasesWrite, CasesDelete, EvidenceWrite, EvidenceDelete,
        CustodyWrite, MalwareWrite, ReportsGenerate, UsersManage
    };
}
