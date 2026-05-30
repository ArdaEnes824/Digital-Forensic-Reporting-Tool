using System.Security.Claims;
using DFIR.CaseManagement.Auth;
using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFIR.CaseManagement.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportController(IReportService reports) => _reports = reports;

    [HttpGet("case/{caseId:int}")]
    public async Task<ActionResult<IReadOnlyList<ReportDto>>> GetByCase(int caseId)
        => Ok(await _reports.GetByCaseAsync(caseId));

    /// <summary>Generates a full case report (pdf / html / xlsx) and returns it as a download.</summary>
    [HttpGet("case/{caseId:int}/generate")]
    [Authorize(Policy = Permissions.ReportsGenerate)]
    public async Task<IActionResult> GenerateCaseReport(int caseId, [FromQuery] string format = "pdf")
    {
        var result = await _reports.GenerateCaseReportAsync(caseId, format, CurrentUser());
        return result is null
            ? NotFound(new { message = $"Case {caseId} not found." })
            : File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>Generates an evidence-focused report (pdf / html / xlsx) for a case.</summary>
    [HttpGet("case/{caseId:int}/evidence")]
    [Authorize(Policy = Permissions.ReportsGenerate)]
    public async Task<IActionResult> GenerateEvidenceReport(int caseId, [FromQuery] string format = "pdf")
    {
        var result = await _reports.GenerateEvidenceReportAsync(caseId, format, CurrentUser());
        return result is null
            ? NotFound(new { message = $"Case {caseId} not found." })
            : File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    private string CurrentUser() => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "system";
}
