using DFIR.CaseManagement.Data;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DFIR.CaseManagement.Repositories;

/// <summary>Concrete case repository: generic CRUD + forensic-specific queries.</summary>
public class CaseRepository : GenericRepository<Case>, ICaseRepository
{
    public CaseRepository(AppDbContext context) : base(context) { }

    public async Task<Case?> GetByCaseNumberAsync(string caseNumber)
        => await Set.FirstOrDefaultAsync(c => c.CaseNumber == caseNumber);

    public async Task<Case?> GetWithDetailsAsync(int id)
        => await Set
            .Include(c => c.EvidenceItems)
            .Include(c => c.CustodyRecords)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Case>> GetByStatusAsync(CaseStatus status)
        => await Set.AsNoTracking().Where(c => c.Status == status).ToListAsync();

    public async Task<string> GenerateNextCaseNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"CASE-{year}-";

        // Find the highest sequence already used this year.
        var lastNumber = await Set
            .Where(c => c.CaseNumber.StartsWith(prefix))
            .Select(c => c.CaseNumber)
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync();

        var next = 1;
        if (!string.IsNullOrEmpty(lastNumber))
        {
            var tail = lastNumber[prefix.Length..];
            if (int.TryParse(tail, out var parsed)) next = parsed + 1;
        }

        return $"{prefix}{next:D4}";
    }
}
