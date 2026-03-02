using Microsoft.EntityFrameworkCore;
using Tracking.Processor.Entities;

public class ParcelDbContext : DbContext
{
    public ParcelDbContext(DbContextOptions<ParcelDbContext> options)
        : base(options) { }

    public DbSet<Parcels> Parcels { get; set; }
    public DbSet<ParcelEventEntity> ParcelEvents { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Parcels>(entity =>
        {
            entity.HasKey(p => p.ParcelId);
            entity.Property(p => p.ParcelId).IsRequired().HasMaxLength(100);
            entity.Property(p => p.CurrentState).IsRequired().HasMaxLength(50);
            entity.Property(p => p.LastUpdated).IsRequired();
            entity.Property(p => p.Version).IsRequired();
        });

        modelBuilder.Entity<ParcelEventEntity>(entity =>
        {
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.EventId).IsRequired();
            entity.Property(e => e.ParcelId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProcessedAt).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}