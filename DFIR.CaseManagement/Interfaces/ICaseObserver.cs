using DFIR.CaseManagement.Entities;

namespace DFIR.CaseManagement.Interfaces;

/// <summary>
/// Observer Pattern: observers subscribe to case lifecycle events and react
/// (audit logging, notifications, ...).
/// </summary>
public interface ICaseObserver
{
    void OnCaseCreated(Case caseEntity);
    void OnCaseStatusChanged(Case caseEntity, CaseStatus oldStatus, CaseStatus newStatus);
    void OnCaseStageChanged(Case caseEntity, WorkflowStage oldStage, WorkflowStage newStage);
}

/// <summary>The subject side of the Observer Pattern.</summary>
public interface ICaseSubject
{
    void Attach(ICaseObserver observer);
    void Detach(ICaseObserver observer);
    void NotifyCreated(Case caseEntity);
    void NotifyStatusChanged(Case caseEntity, CaseStatus oldStatus, CaseStatus newStatus);
    void NotifyStageChanged(Case caseEntity, WorkflowStage oldStage, WorkflowStage newStage);
}
