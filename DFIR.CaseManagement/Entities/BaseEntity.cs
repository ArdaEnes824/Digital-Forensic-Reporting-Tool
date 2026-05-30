namespace DFIR.CaseManagement.Entities;

/// <summary>
/// Abstract root for every persisted entity. Demonstrates ABSTRACTION:
/// callers depend on this shared shape, never instantiate it directly.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Polymorphic summary used by audit logging. Each entity describes itself.
    /// </summary>
    public abstract string Describe();
}
