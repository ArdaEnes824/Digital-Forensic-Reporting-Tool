namespace DFIR.CaseManagement.Entities;

public enum CaseStatus
{
    Open = 0,
    InProgress = 1,
    OnHold = 2,
    Closed = 3
}

public enum CasePriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>The ICAPAIR workflow phases.</summary>
public enum WorkflowStage
{
    Identify = 0,
    Collect = 1,
    Acquire = 2,
    Preserve = 3,
    Analyze = 4,
    Interpret = 5,
    Report = 6
}

public enum RiskLevel
{
    Clean = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
