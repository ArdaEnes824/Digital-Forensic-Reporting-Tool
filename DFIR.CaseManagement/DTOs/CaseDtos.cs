using System.ComponentModel.DataAnnotations;
using DFIR.CaseManagement.Entities;

namespace DFIR.CaseManagement.DTOs;

public record CaseDto(
    int Id,
    string CaseNumber,
    string Title,
    string Description,
    CaseStatus Status,
    CasePriority Priority,
    WorkflowStage CurrentStage,
    string AssignedTo,
    int EvidenceCount,
    DateTime CreatedDate);

public class CaseCreateDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public CasePriority Priority { get; set; } = CasePriority.Medium;
    public string AssignedTo { get; set; } = string.Empty;
}

public class CaseUpdateDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public CaseStatus Status { get; set; }
    public CasePriority Priority { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
}
