namespace DFIR.CaseManagement.Entities;

/// <summary>A single hand-off in the chain of custody for a case.</summary>
public class ChainOfCustody : BaseEntity
{
    public string FromPerson { get; set; } = string.Empty;
    public string ToPerson { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int CaseId { get; set; }
    public Case? Case { get; set; }

    public override string Describe()
        => $"Custody {FromPerson} -> {ToPerson} @ {Location} ({TransferDate:yyyy-MM-dd})";
}
