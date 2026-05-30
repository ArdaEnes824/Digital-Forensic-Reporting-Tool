using DFIR.CaseManagement.Entities;

namespace DFIR.CaseManagement.Interfaces;

/// <summary>
/// Unit of Work. Coordinates the repositories and commits all changes in a single
/// transaction via SaveChangesAsync.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ICaseRepository Cases { get; }
    IEvidenceRepository Evidence { get; }
    IGenericRepository<ChainOfCustody> CustodyRecords { get; }
    IMalwareAnalysisRepository MalwareAnalyses { get; }
    IReportRepository Reports { get; }
    IGenericRepository<User> Users { get; }

    Task<int> SaveChangesAsync();
}
