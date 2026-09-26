using FieldOps.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Data;

// DbContext = a session with the database: tracks changes and saves them
public class FieldOpsDbContext(DbContextOptions<FieldOpsDbContext> options) : DbContext(options)
{
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>(); // = a table

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaintenanceRequest>(e =>
        {
            e.Property(r => r.Title).HasMaxLength(200).IsRequired();
            e.Property(r => r.Equipment).HasMaxLength(100).IsRequired();
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20); // store "Open", not 0
            e.HasIndex(r => r.Status);                                         // fast filtering by status
        });
    }
}