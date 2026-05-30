using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Entities;

namespace DFIR.CaseManagement.Interfaces;

public interface ICaseService
{
    Task<IReadOnlyList<CaseDto>> GetAllAsync();
    Task<CaseDto?> GetByIdAsync(int id);
    Task<CaseDto> CreateAsync(CaseCreateDto dto);
    Task<CaseDto?> UpdateAsync(int id, CaseUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<CaseDto?> AdvanceStageAsync(int id, WorkflowStage stage);
}

public interface IEvidenceService
{
    Task<IReadOnlyList<EvidenceDto>> GetAllAsync();
    Task<IReadOnlyList<EvidenceDto>> GetByCaseAsync(int caseId);
    Task<EvidenceDto?> GetByIdAsync(int id);
    Task<EvidenceDto> CreateAsync(EvidenceCreateDto dto);
    Task<EvidenceDto?> UpdateAsync(int id, EvidenceCreateDto dto);
    Task<bool> DeleteAsync(int id);

    /// <summary>Recomputes the hashes of the supplied bytes and verifies them against the stored hash.</summary>
    Task<bool> VerifyHashesAsync(int id, byte[] data);
}

public interface ICustodyService
{
    Task<IReadOnlyList<CustodyDto>> GetByCaseAsync(int caseId);
    Task<CustodyDto> CreateAsync(CustodyCreateDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IMalwareService
{
    Task<IReadOnlyList<MalwareAnalysisDto>> GetAllAsync();
    Task<MalwareAnalysisDto?> GetByIdAsync(int id);
    Task<MalwareAnalysisDto> AnalyzeAsync(string fileName, byte[] content, int? caseId);
    Task<bool> DeleteAsync(int id);

    /// <summary>Shannon entropy (bits/byte, 0..8) of the supplied bytes.</summary>
    double CalculateEntropy(byte[] data);

    /// <summary>Derives a 0..100 risk score from the entropy and artifact size.</summary>
    double CalculateRiskScore(double entropy, long fileSize);
}

public interface IReportService
{
    Task<(byte[] Content, string ContentType, string FileName)?> GenerateAsync(int caseId, string format, string generatedBy);

    /// <summary>Full case report in the requested format (pdf / html / xlsx).</summary>
    Task<(byte[] Content, string ContentType, string FileName)?> GenerateCaseReportAsync(int caseId, string format, string generatedBy);

    /// <summary>Evidence-focused report for a case in the requested format.</summary>
    Task<(byte[] Content, string ContentType, string FileName)?> GenerateEvidenceReportAsync(int caseId, string format, string generatedBy);

    Task<IReadOnlyList<ReportDto>> GetByCaseAsync(int caseId);
}

public interface IDashboardService
{
    Task<DashboardDto> GetSummaryAsync();
}

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<UserDto?> RegisterAsync(RegisterDto dto);
    Task<IReadOnlyList<UserDto>> GetUsersAsync();
    Task<bool> DeleteUserAsync(int id);

    /// <summary>Authenticates a user and issues an access + refresh token pair.</summary>
    Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);

    /// <summary>Rotates a valid refresh token into a fresh access + refresh token pair.</summary>
    Task<LoginResponseDto?> RefreshAsync(RefreshRequestDto request);
}
