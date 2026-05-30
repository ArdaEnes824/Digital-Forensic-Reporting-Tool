using DFIR.CaseManagement.Data;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Repositories;

/// <summary>Unit of Work: one DbContext, many repositories, one commit.</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ICaseRepository Cases { get; }
    public IEvidenceRepository Evidence { get; }
    public IGenericRepository<ChainOfCustody> CustodyRecords { get; }
    public IMalwareAnalysisRepository MalwareAnalyses { get; }
    public IReportRepository Reports { get; }
    public IGenericRepository<User> Users { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Cases = new CaseRepository(context);
        Evidence = new EvidenceRepository(context);
        CustodyRecords = new GenericRepository<ChainOfCustody>(context);
        MalwareAnalyses = new MalwareAnalysisRepository(context);
        Reports = new ReportRepository(context);
        Users = new GenericRepository<User>(context);
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
