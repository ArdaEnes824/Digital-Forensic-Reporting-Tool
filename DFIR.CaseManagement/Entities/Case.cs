namespace DFIR.CaseManagement.Entities;

/// <summary>A forensic investigation case. Aggregate root for evidence and custody records.</summary>
public class Case : BaseEntity
{
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CaseStatus Status { get; set; } = CaseStatus.Open;
    public CasePriority Priority { get; set; } = CasePriority.Medium;

    /// <summary>Current position in the ICAPAIR workflow.</summary>
    public WorkflowStage CurrentStage { get; set; } = WorkflowStage.Identify;

    public string AssignedTo { get; set; } = string.Empty;

    public ICollection<Evidence> EvidenceItems { get; set; } = new List<Evidence>();
    public ICollection<ChainOfCustody> CustodyRecords { get; set; } = new List<ChainOfCustody>();

    public override string Describe() => $"Case {CaseNumber} - {Title} [{Status}]";
}
