namespace DFIR.CaseManagement.DTOs;

public record ReportDto(
    int Id,
    int CaseId,
    string Title,
    string Format,
    string GeneratedBy,
    DateTime CreatedDate);
