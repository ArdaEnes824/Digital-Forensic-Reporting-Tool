using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services;

/// <summary>
/// Service Layer for reporting. Uses the Strategy Pattern: a set of IReportGenerator
/// strategies (PDF / HTML / Excel) is injected and selected at runtime by format key.
/// </summary>
public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ReportService> _logger;
    private readonly IReadOnlyDictionary<string, IReportGenerator> _strategies;

    public ReportService(IEnumerable<IReportGenerator> generators, IUnitOfWork uow, ILogger<ReportService> logger)
    {
        _uow = uow;
        _logger = logger;
        _strategies = generators.ToDictionary(g => g.Format, StringComparer.OrdinalIgnoreCase);
    }

    public Task<(byte[] Content, string ContentType, string FileName)?> GenerateAsync(int caseId, string format, string generatedBy)
        => GenerateCaseReportAsync(caseId, format, generatedBy);

    public async Task<(byte[] Content, string ContentType, string FileName)?> GenerateCaseReportAsync(int caseId, string format, string generatedBy)
        => await BuildAsync(caseId, format, generatedBy, evidenceOnly: false, titlePrefix: "Case Report");

    public async Task<(byte[] Content, string ContentType, string FileName)?> GenerateEvidenceReportAsync(int caseId, string format, string generatedBy)
        => await BuildAsync(caseId, format, generatedBy, evidenceOnly: true, titlePrefix: "Evidence Report");

    public async Task<IReadOnlyList<ReportDto>> GetByCaseAsync(int caseId)
    {
        var reports = await _uow.Reports.GetByCaseAsync(caseId);
        return reports
            .Select(r => new ReportDto(r.Id, r.CaseId, r.Title, r.Format, r.GeneratedBy, r.CreatedDate))
            .ToList();
    }

    private async Task<(byte[] Content, string ContentType, string FileName)?> BuildAsync(
        int caseId, string format, string generatedBy, bool evidenceOnly, string titlePrefix)
    {
        var caseEntity = await _uow.Cases.GetWithDetailsAsync(caseId);
        if (caseEntity is null)
        {
            _logger.LogWarning("Report generation failed: case {Id} not found", caseId);
            return null;
        }

        var key = string.IsNullOrWhiteSpace(format) ? "pdf" : format.Trim().ToLowerInvariant();
        if (!_strategies.TryGetValue(key, out var generator))
            throw new InvalidOperationException($"Unsupported report format '{format}'. Available: {string.Join(", ", _strategies.Keys)}.");

        var content = evidenceOnly ? generator.GenerateEvidenceReport(caseEntity) : generator.Generate(caseEntity);

        var report = new Report
        {
            CaseId = caseEntity.Id,
            Title = $"{titlePrefix} - {caseEntity.CaseNumber}",
            Format = generator.Format,
            GeneratedBy = string.IsNullOrWhiteSpace(generatedBy) ? "system" : generatedBy
        };
        await _uow.Reports.AddAsync(report);
        await _uow.SaveChangesAsync();

        var fileName = $"{caseEntity.CaseNumber}-{(evidenceOnly ? "evidence" : "case")}.{generator.FileExtension}";
        _logger.LogInformation("Generated {Title} as {Format} by {By}", report.Title, generator.Format, report.GeneratedBy);

        return (content, generator.ContentType, fileName);
    }
}
