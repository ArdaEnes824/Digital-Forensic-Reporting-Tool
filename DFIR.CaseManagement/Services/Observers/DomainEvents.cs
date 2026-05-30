using DFIR.CaseManagement.Entities;

namespace DFIR.CaseManagement.Services.Observers;

/// <summary>Raised when a new case is created. Part of the Observer Pattern payloads.</summary>
public record CaseCreatedEvent(int CaseId, string CaseNumber, string Title, DateTime OccurredAtUtc)
{
    public static CaseCreatedEvent From(Case c) => new(c.Id, c.CaseNumber, c.Title, DateTime.UtcNow);
}

/// <summary>Raised when a case's status transitions from one value to another.</summary>
public record CaseStatusChangedEvent(int CaseId, string CaseNumber, CaseStatus OldStatus, CaseStatus NewStatus, DateTime OccurredAtUtc)
{
    public static CaseStatusChangedEvent From(Case c, CaseStatus oldStatus, CaseStatus newStatus)
        => new(c.Id, c.CaseNumber, oldStatus, newStatus, DateTime.UtcNow);
}
