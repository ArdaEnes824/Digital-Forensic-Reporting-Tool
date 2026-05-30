using System.ComponentModel.DataAnnotations;

namespace DFIR.CaseManagement.DTOs;

public record CustodyDto(
    int Id,
    int CaseId,
    string FromPerson,
    string ToPerson,
    DateTime TransferDate,
    string Location,
    string Description);

public class CustodyCreateDto
{
    [Required]
    public int CaseId { get; set; }

    [Required, StringLength(100)]
    public string FromPerson { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ToPerson { get; set; } = string.Empty;

    public DateTime TransferDate { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}
