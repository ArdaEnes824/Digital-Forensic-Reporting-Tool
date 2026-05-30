using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services;

/// <summary>Service Layer for evidence items, including cryptographic integrity verification.</summary>
public class EvidenceService : IEvidenceService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<EvidenceService> _logger;

    public EvidenceService(IUnitOfWork uow, ILogger<EvidenceService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<EvidenceDto>> GetAllAsync()
    {
        var items = await _uow.Evidence.GetAllAsync();
        return items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<EvidenceDto>> GetByCaseAsync(int caseId)
    {
        var items = await _uow.Evidence.GetByCaseAsync(caseId);
        return items.Select(Map).ToList();
    }

    public async Task<EvidenceDto?> GetByIdAsync(int id)
    {
        var entity = await _uow.Evidence.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public async Task<EvidenceDto> CreateAsync(EvidenceCreateDto dto)
    {
        var parentCase = await _uow.Cases.GetByIdAsync(dto.CaseId)
            ?? throw new InvalidOperationException($"Case {dto.CaseId} not found.");

        var entity = new Evidence
        {
            EvidenceCode = await _uow.Evidence.GenerateNextEvidenceCodeAsync(parentCase.Id),
            CaseId = parentCase.Id,
            DeviceType = dto.DeviceType,
            Manufacturer = dto.Manufacturer,
            Model = dto.Model,
            SerialNumber = dto.SerialNumber
        };

        // No raw disk image in this flow, so derive a baseline integrity hash from the
        // item's stable identity (code + serial). VerifyHashesAsync re-checks against it.
        entity.GenerateHashesFromText($"{entity.EvidenceCode}|{entity.SerialNumber}");

        await _uow.Evidence.AddAsync(entity);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Registered evidence {Code} for case {CaseId}", entity.EvidenceCode, parentCase.Id);
        return Map(entity);
    }

    public async Task<EvidenceDto?> UpdateAsync(int id, EvidenceCreateDto dto)
    {
        var entity = await _uow.Evidence.GetByIdAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Update failed: evidence {Id} not found", id);
            return null;
        }

        entity.DeviceType = dto.DeviceType;
        entity.Manufacturer = dto.Manufacturer;
        entity.Model = dto.Model;
        entity.SerialNumber = dto.SerialNumber;
        entity.GenerateHashesFromText($"{entity.EvidenceCode}|{entity.SerialNumber}");

        _uow.Evidence.Update(entity);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Updated evidence {Code} (Id {Id})", entity.EvidenceCode, id);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _uow.Evidence.GetByIdAsync(id);
        if (entity is null) return false;

        _uow.Evidence.Remove(entity);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Deleted evidence {Code} (Id {Id})", entity.EvidenceCode, id);
        return true;
    }

    public async Task<bool> VerifyHashesAsync(int id, byte[] data)
    {
        var entity = await _uow.Evidence.GetByIdAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Hash verification failed: evidence {Id} not found", id);
            return false;
        }

        var ok = entity.VerifyIntegrity(data);
        _logger.LogInformation("Hash verification for evidence {Code}: {Result}", entity.EvidenceCode, ok ? "MATCH" : "MISMATCH");
        return ok;
    }

    private static EvidenceDto Map(Evidence e) => new(
        e.Id, e.EvidenceCode, e.DeviceType, e.Manufacturer, e.Model, e.SerialNumber,
        e.SHA256Hash, e.MD5Hash, e.CaseId, e.CreatedDate);
}
