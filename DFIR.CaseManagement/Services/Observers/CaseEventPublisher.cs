using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services.Observers;

/// <summary>
/// The Subject in the Observer Pattern. Registered as a singleton; observers attach
/// at startup and are notified whenever the CaseService raises a lifecycle event.
/// </summary>
public class CaseEventPublisher : ICaseSubject
{
    private readonly List<ICaseObserver> _observers = new();
    private readonly object _gate = new();

    public void Attach(ICaseObserver observer)
    {
        lock (_gate)
        {
            if (!_observers.Contains(observer)) _observers.Add(observer);
        }
    }

    public void Detach(ICaseObserver observer)
    {
        lock (_gate) { _observers.Remove(observer); }
    }

    private List<ICaseObserver> Snapshot()
    {
        lock (_gate) { return _observers.ToList(); }
    }

    public void NotifyCreated(Case caseEntity)
    {
        foreach (var o in Snapshot()) o.OnCaseCreated(caseEntity);
    }

    public void NotifyStatusChanged(Case caseEntity, CaseStatus oldStatus, CaseStatus newStatus)
    {
        foreach (var o in Snapshot()) o.OnCaseStatusChanged(caseEntity, oldStatus, newStatus);
    }

    public void NotifyStageChanged(Case caseEntity, WorkflowStage oldStage, WorkflowStage newStage)
    {
        foreach (var o in Snapshot()) o.OnCaseStageChanged(caseEntity, oldStage, newStage);
    }
}
