using System.ComponentModel.DataAnnotations;

namespace DFIR.CaseManagement.DTOs;

public record EvidenceDto(
    int Id,
    string EvidenceCode,
    string DeviceType,
    string Manufacturer,
    string Model,
    string SerialNumber,
    string? SHA256Hash,
    string? MD5Hash,
    int CaseId,
    DateTime CreatedDate);

public class EvidenceCreateDto
{
    [Required]
    public int CaseId { get; set; }

    [Required, StringLength(100)]
    public string DeviceType { get; set; } = string.Empty;

    [StringLength(100)]
    public string Manufacturer { get; set; } = string.Empty;

    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [StringLength(100)]
    public string SerialNumber { get; set; } = string.Empty;
}
