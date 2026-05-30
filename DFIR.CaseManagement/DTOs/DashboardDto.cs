namespace DFIR.CaseManagement.DTOs;

public record DashboardDto(
    int TotalCases,
    int OpenCases,
    int ClosedCases,
    int TotalEvidence,
    int TotalMalwareAnalyses,
    int HighRiskMalware);
