using LogicLink.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogicLink.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Circuit> Circuits => Set<Circuit>();
    public DbSet<Gate> Gates => Set<Gate>();
    public DbSet<Wire> Wires => Set<Wire>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Circuit>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(120).IsRequired();
            entity.Property(c => c.OwnerName).HasMaxLength(60).IsRequired();
            entity.HasIndex(c => c.IsDeleted);

            entity.HasMany(c => c.Gates)
                  .WithOne(g => g.Circuit)
                  .HasForeignKey(g => g.CircuitId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Wires)
                  .WithOne(w => w.Circuit)
                  .HasForeignKey(w => w.CircuitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Gate>(entity =>
        {
             entity.Property(g => g.Type).HasConversion<string>().HasMaxLength(10);
            entity.Property(g => g.Label).HasMaxLength(60);
        });

        modelBuilder.Entity<Wire>(entity =>
        {
            entity.HasIndex(w => new { w.CircuitId, w.FromGateId });
            entity.HasIndex(w => new { w.CircuitId, w.ToGateId });
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Circuit>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }
}