namespace DFIR.CaseManagement.Entities;

/// <summary>Metadata record for a generated report.</summary>
public class Report : BaseEntity
{
    public int CaseId { get; set; }
    public Case? Case { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Format { get; set; } = "pdf";
    public string GeneratedBy { get; set; } = string.Empty;

    public override string Describe() => $"Report '{Title}' for case {CaseId} ({Format})";
}
