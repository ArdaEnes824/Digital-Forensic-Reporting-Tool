using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services;

/// <summary>Service Layer for chain-of-custody hand-off records.</summary>
public class CustodyService : ICustodyService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CustodyService> _logger;

    public CustodyService(IUnitOfWork uow, ILogger<CustodyService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CustodyDto>> GetByCaseAsync(int caseId)
    {
        var records = await _uow.CustodyRecords.FindAsync(c => c.CaseId == caseId);
        return records
            .OrderBy(c => c.TransferDate)
            .Select(Map)
            .ToList();
    }

    public async Task<CustodyDto> CreateAsync(CustodyCreateDto dto)
    {
        _ = await _uow.Cases.GetByIdAsync(dto.CaseId)
            ?? throw new InvalidOperationException($"Case {dto.CaseId} not found.");

        var entity = new ChainOfCustody
        {
            CaseId = dto.CaseId,
            FromPerson = dto.FromPerson,
            ToPerson = dto.ToPerson,
            TransferDate = dto.TransferDate,
            Location = dto.Location,
            Description = dto.Description
        };

        await _uow.CustodyRecords.AddAsync(entity);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Custody hand-off recorded for case {CaseId}: {From} -> {To}",
            dto.CaseId, dto.FromPerson, dto.ToPerson);

        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _uow.CustodyRecords.GetByIdAsync(id);
        if (entity is null) return false;

        _uow.CustodyRecords.Remove(entity);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Deleted custody record {Id}", id);
        return true;
    }

    private static CustodyDto Map(ChainOfCustody c) => new(
        c.Id, c.CaseId, c.FromPerson, c.ToPerson, c.TransferDate, c.Location, c.Description);
}
