using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services.Observers;

/// <summary>
/// Concrete Observer that turns case lifecycle callbacks into notification events
/// (CaseCreatedEvent / CaseStatusChangedEvent) and dispatches them. Here the dispatch
/// target is the log; in a real deployment it could be email / SignalR / a queue.
/// </summary>
public class NotificationObserver : ICaseObserver
{
    private readonly ILogger<NotificationObserver> _logger;

    public NotificationObserver(ILogger<NotificationObserver> logger) => _logger = logger;

    public void OnCaseCreated(Case caseEntity)
    {
        var evt = CaseCreatedEvent.From(caseEntity);
        Dispatch($"New case opened: {evt.CaseNumber} - {evt.Title}");
    }

    public void OnCaseStatusChanged(Case caseEntity, CaseStatus oldStatus, CaseStatus newStatus)
    {
        var evt = CaseStatusChangedEvent.From(caseEntity, oldStatus, newStatus);
        Dispatch($"Case {evt.CaseNumber} status changed: {evt.OldStatus} -> {evt.NewStatus}");
    }

    public void OnCaseStageChanged(Case caseEntity, WorkflowStage oldStage, WorkflowStage newStage)
        => Dispatch($"Case {caseEntity.CaseNumber} advanced in ICAPAIR workflow: {oldStage} -> {newStage}");

    private void Dispatch(string message)
        => _logger.LogInformation("[NOTIFY] {Message}", message);
}
