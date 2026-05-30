using DFIR.CaseManagement.Entities;

namespace DFIR.CaseManagement.Interfaces;

/// <summary>
/// Case-specific repository. Extends the generic Repository Pattern contract with
/// domain queries that the generic CRUD surface cannot express on its own.
/// </summary>
public interface ICaseRepository : IGenericRepository<Case>
{
    Task<Case?> GetByCaseNumberAsync(string caseNumber);

    /// <summary>Loads a case together with its evidence items and custody records.</summary>
    Task<Case?> GetWithDetailsAsync(int id);

    Task<IReadOnlyList<Case>> GetByStatusAsync(CaseStatus status);

    /// <summary>Produces the next sequential, unique case number (e.g. CASE-2026-0007).</summary>
    Task<string> GenerateNextCaseNumberAsync();
}

/// <summary>Evidence-specific repository.</summary>
public interface IEvidenceRepository : IGenericRepository<Evidence>
{
    Task<IReadOnlyList<Evidence>> GetByCaseAsync(int caseId);
    Task<Evidence?> GetByEvidenceCodeAsync(string evidenceCode);

    /// <summary>Produces the next unique evidence code for a case (e.g. EVD-12-0003).</summary>
    Task<string> GenerateNextEvidenceCodeAsync(int caseId);
}

/// <summary>Malware analysis-specific repository.</summary>
public interface IMalwareAnalysisRepository : IGenericRepository<MalwareAnalysis>
{
    Task<IReadOnlyList<MalwareAnalysis>> GetByCaseAsync(int caseId);
    Task<IReadOnlyList<MalwareAnalysis>> GetHighRiskAsync();
    Task<int> CountHighRiskAsync();
}

/// <summary>Report metadata-specific repository.</summary>
public interface IReportRepository : IGenericRepository<Report>
{
    Task<IReadOnlyList<Report>> GetByCaseAsync(int caseId);
}
