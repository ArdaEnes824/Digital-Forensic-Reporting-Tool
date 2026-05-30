using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services.Observers;

/// <summary>A concrete Observer that writes case lifecycle events to the log.</summary>
public class AuditLogObserver : ICaseObserver
{
    private readonly ILogger<AuditLogObserver> _logger;

    public AuditLogObserver(ILogger<AuditLogObserver> logger) => _logger = logger;

    public void OnCaseCreated(Case caseEntity)
        => _logger.LogInformation("[AUDIT] Case created: {Case}", caseEntity.Describe());

    public void OnCaseStatusChanged(Case caseEntity, CaseStatus oldStatus, CaseStatus newStatus)
        => _logger.LogInformation("[AUDIT] Case {Number} status {Old} -> {New}",
            caseEntity.CaseNumber, oldStatus, newStatus);

    public void OnCaseStageChanged(Case caseEntity, WorkflowStage oldStage, WorkflowStage newStage)
        => _logger.LogInformation("[AUDIT] Case {Number} ICAPAIR stage {Old} -> {New}",
            caseEntity.CaseNumber, oldStage, newStage);
}
