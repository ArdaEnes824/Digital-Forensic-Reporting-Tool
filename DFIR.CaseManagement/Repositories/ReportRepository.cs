using DFIR.CaseManagement.Data;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DFIR.CaseManagement.Repositories;

/// <summary>Concrete report repository: generic CRUD + per-case lookup.</summary>
public class ReportRepository : GenericRepository<Report>, IReportRepository
{
    public ReportRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Report>> GetByCaseAsync(int caseId)
        => await Set.AsNoTracking()
            .Where(r => r.CaseId == caseId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
}
