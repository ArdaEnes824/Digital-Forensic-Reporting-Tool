using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DFIR.CaseManagement.Services;

/// <summary>
/// Service Layer for cases. Coordinates the Unit of Work, raises Observer events on
/// create / status-change / stage-change, and logs every mutation.
/// </summary>
public class CaseService : ICaseService
{
    private readonly IUnitOfWork _uow;
    private readonly ICaseSubject _publisher;
    private readonly ILogger<CaseService> _logger;

    public CaseService(IUnitOfWork uow, ICaseSubject publisher, ILogger<CaseService> logger)
    {
        _uow = uow;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CaseDto>> GetAllAsync()
    {
        var cases = await _uow.Cases.Query()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedDate)
            .Select(c => new CaseDto(
                c.Id, c.CaseNumber, c.Title, c.Description, c.Status, c.Priority,
                c.CurrentStage, c.AssignedTo, c.EvidenceItems.Count, c.CreatedDate))
            .ToListAsync();

        _logger.LogInformation("Listed {Count} cases", cases.Count);
        return cases;
    }

    public async Task<CaseDto?> GetByIdAsync(int id)
    {
        var entity = await _uow.Cases.GetWithDetailsAsync(id);
        return entity is null ? null : Map(entity);
    }

    public async Task<CaseDto> CreateAsync(CaseCreateDto dto)
    {
        var entity = new Case
        {
            CaseNumber = await _uow.Cases.GenerateNextCaseNumberAsync(),
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            AssignedTo = dto.AssignedTo,
            Status = CaseStatus.Open,
            CurrentStage = WorkflowStage.Identify
        };

        await _uow.Cases.AddAsync(entity);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Created case {CaseNumber} (Id {Id})", entity.CaseNumber, entity.Id);
        _publisher.NotifyCreated(entity);

        return Map(entity);
    }

    public async Task<CaseDto?> UpdateAsync(int id, CaseUpdateDto dto)
    {
        var entity = await _uow.Cases.GetByIdAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Update failed: case {Id} not found", id);
            return null;
        }

        var oldStatus = entity.Status;

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Status = dto.Status;
        entity.Priority = dto.Priority;
        entity.AssignedTo = dto.AssignedTo;

        _uow.Cases.Update(entity);
        await _uow.SaveChangesAsync();

        if (oldStatus != dto.Status)
        {
            _logger.LogInformation("Case {CaseNumber} status {Old} -> {New}", entity.CaseNumber, oldStatus, dto.Status);
            _publisher.NotifyStatusChanged(entity, oldStatus, dto.Status);
        }

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _uow.Cases.GetByIdAsync(id);
        if (entity is null) return false;

        _uow.Cases.Remove(entity);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Deleted case {CaseNumber} (Id {Id})", entity.CaseNumber, id);
        return true;
    }

    public async Task<CaseDto?> AdvanceStageAsync(int id, WorkflowStage stage)
    {
        var entity = await _uow.Cases.GetByIdAsync(id);
        if (entity is null) return null;

        var oldStage = entity.CurrentStage;
        entity.CurrentStage = stage;
        _uow.Cases.Update(entity);
        await _uow.SaveChangesAsync();

        if (oldStage != stage)
        {
            _logger.LogInformation("Case {CaseNumber} ICAPAIR stage {Old} -> {New}", entity.CaseNumber, oldStage, stage);
            _publisher.NotifyStageChanged(entity, oldStage, stage);
        }

        return await GetByIdAsync(id);
    }

    private static CaseDto Map(Case c) => new(
        c.Id, c.CaseNumber, c.Title, c.Description, c.Status, c.Priority,
        c.CurrentStage, c.AssignedTo, c.EvidenceItems?.Count ?? 0, c.CreatedDate);
}
