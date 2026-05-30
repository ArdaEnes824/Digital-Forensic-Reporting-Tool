using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Entities;
using DFIR.CaseManagement.Interfaces;

namespace DFIR.CaseManagement.Services;

/// <summary>Service Layer for the dashboard summary tiles.</summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IUnitOfWork uow, ILogger<DashboardService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<DashboardDto> GetSummaryAsync()
    {
        var totalCases = await _uow.Cases.CountAsync();
        var openCases = await _uow.Cases.CountAsync(c => c.Status != CaseStatus.Closed);
        var closedCases = await _uow.Cases.CountAsync(c => c.Status == CaseStatus.Closed);
        var totalEvidence = await _uow.Evidence.CountAsync();
        var totalMalware = await _uow.MalwareAnalyses.CountAsync();
        var highRiskMalware = await _uow.MalwareAnalyses.CountHighRiskAsync();

        _logger.LogInformation("Dashboard summary computed ({Total} cases, {Evidence} evidence)", totalCases, totalEvidence);

        return new DashboardDto(totalCases, openCases, closedCases, totalEvidence, totalMalware, highRiskMalware);
    }
}
