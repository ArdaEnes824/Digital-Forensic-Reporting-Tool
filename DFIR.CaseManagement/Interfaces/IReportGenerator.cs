using DFIR.CaseManagement.Entities;

namespace DFIR.CaseManagement.Interfaces;

/// <summary>
/// Strategy abstraction. Each concrete report generator (PDF, HTML, ...) is an
/// interchangeable strategy used by the ReportService at runtime.
/// </summary>
public interface IReportGenerator
{
    /// <summary>A short key used to select the strategy, e.g. "pdf" or "html".</summary>
    string Format { get; }

    string ContentType { get; }

    string FileExtension { get; }

    byte[] Generate(Case caseEntity);

    /// <summary>
    /// Evidence-focused variant of the report. Default implementation reuses the full
    /// case report; strategies may override it to emphasise the evidence inventory.
    /// </summary>
    byte[] GenerateEvidenceReport(Case caseEntity) => Generate(caseEntity);
}
