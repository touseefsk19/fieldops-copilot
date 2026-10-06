using FieldOps.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Data;

// DbContext = a session with the database: tracks changes and saves them
public class FieldOpsDbContext(DbContextOptions<FieldOpsDbContext> options) : DbContext(options)
{
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>(); // = a table

    public DbSet<ManualChunk> ManualChunks => Set<ManualChunk>();

    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaintenanceRequest>(e =>
        {
            e.Property(r => r.Title).HasMaxLength(200).IsRequired();
            e.Property(r => r.Equipment).HasMaxLength(100).IsRequired();
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20); // store "Open", not 0
            e.HasIndex(r => r.Status);                                         // fast filtering by status
        });

                modelBuilder.Entity<SparePart>(e =>
        {
            e.Property(p => p.PartNumber).HasMaxLength(30).IsRequired();
            e.HasIndex(p => p.PartNumber).IsUnique();   // one row per part number
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Equipment).HasMaxLength(100);
            e.Property(p => p.Bin).HasMaxLength(20);

            // Sample stock, inserted by the migration
            e.HasData(
                new SparePart { Id = 1, PartNumber = "SK-200", Name = "CP-200 mechanical seal kit", Equipment = "CP-200 pump", Quantity = 3, Bin = "A-12" },
                new SparePart { Id = 2, PartNumber = "CB-45", Name = "Coupling bolt set (M12)", Equipment = "CP-200 pump", Quantity = 12, Bin = "A-14" },
                new SparePart { Id = 3, PartNumber = "BRG-6205", Name = "Pump bearing 6205", Equipment = "CP-200 pump", Quantity = 0, Bin = "A-15" },
                new SparePart { Id = 4, PartNumber = "FAN-FL30", Name = "FL-30 lift motor cooling fan", Equipment = "FL-30 forklift", Quantity = 2, Bin = "C-03" },
                new SparePart { Id = 5, PartNumber = "LOTO-PL", Name = "Personal safety padlock", Equipment = "LOTO", Quantity = 20, Bin = "S-01" });
        });

        modelBuilder.Entity<AuditEntry>(e =>
        {
            e.Property(a => a.User).HasMaxLength(100);
            e.Property(a =>a.Action).HasMaxLength(50);
            e.HasIndex(a => a.AtUtc);
        });


        modelBuilder.Entity<ManualChunk>(e =>
        {
            e.Property(c => c.Source).HasMaxLength(200).IsRequired();
            e.Property(c => c.Section).HasMaxLength(200).IsRequired();
            e.HasIndex(c => c.Source);  // ingest deletes by Source, so make that fast
            e.Property(c => c.Audience).HasMaxLength(20).IsRequired();
        });

    }
}