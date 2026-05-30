using DFIR.CaseManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace DFIR.CaseManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<Evidence> Evidence => Set<Evidence>();
    public DbSet<ChainOfCustody> CustodyRecords => Set<ChainOfCustody>();
    public DbSet<MalwareAnalysis> MalwareAnalyses => Set<MalwareAnalysis>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table-Per-Hierarchy for the User inheritance tree.
        modelBuilder.Entity<User>(b =>
        {
            b.HasDiscriminator<string>("UserType")
             .HasValue<Admin>(Admin.RoleName)
             .HasValue<Analyst>(Analyst.RoleName)
             .HasValue<Viewer>(Viewer.RoleName);

            b.HasIndex(u => u.Username).IsUnique();
            b.Property(u => u.Username).IsRequired().HasMaxLength(50);
            b.Property("PasswordHash").IsRequired();
        });

        modelBuilder.Entity<Case>(b =>
        {
            b.HasIndex(c => c.CaseNumber).IsUnique();
            b.Property(c => c.CaseNumber).IsRequired().HasMaxLength(50);
            b.Property(c => c.Title).IsRequired().HasMaxLength(200);

            b.HasMany(c => c.EvidenceItems)
             .WithOne(e => e.Case)
             .HasForeignKey(e => e.CaseId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(c => c.CustodyRecords)
             .WithOne(cc => cc.Case)
             .HasForeignKey(cc => cc.CaseId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Evidence>(b =>
        {
            b.HasIndex(e => e.EvidenceCode).IsUnique();
            b.Property(e => e.EvidenceCode).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<MalwareAnalysis>(b =>
        {
            b.HasOne(m => m.Case)
             .WithMany()
             .HasForeignKey(m => m.CaseId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Report>(b =>
        {
            b.HasOne(r => r.Case)
             .WithMany()
             .HasForeignKey(r => r.CaseId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
