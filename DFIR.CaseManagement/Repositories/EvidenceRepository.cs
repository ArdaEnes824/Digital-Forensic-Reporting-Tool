using DFIR.CaseManagement.Data;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DFIR.CaseManagement.Repositories;

/// <summary>Concrete evidence repository: generic CRUD + evidence-specific queries.</summary>
public class EvidenceRepository : GenericRepository<Evidence>, IEvidenceRepository
{
    public EvidenceRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Evidence>> GetByCaseAsync(int caseId)
        => await Set.AsNoTracking().Where(e => e.CaseId == caseId).ToListAsync();

    public async Task<Evidence?> GetByEvidenceCodeAsync(string evidenceCode)
        => await Set.FirstOrDefaultAsync(e => e.EvidenceCode == evidenceCode);

    public async Task<string> GenerateNextEvidenceCodeAsync(int caseId)
    {
        var prefix = $"EVD-{caseId}-";

        var lastCode = await Set
            .Where(e => e.EvidenceCode.StartsWith(prefix))
            .Select(e => e.EvidenceCode)
            .OrderByDescending(c => c)
            .FirstOrDefaultAsync();

        var next = 1;
        if (!string.IsNullOrEmpty(lastCode))
        {
            var tail = lastCode[prefix.Length..];
            if (int.TryParse(tail, out var parsed)) next = parsed + 1;
        }

        return $"{prefix}{next:D4}";
    }
}
