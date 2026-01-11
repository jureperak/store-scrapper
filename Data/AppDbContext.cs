using Microsoft.EntityFrameworkCore;
using StoreScrapper.Models.Entities;

namespace StoreScrapper.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Adapter> Adapters { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<ProductSku> ProductSkus { get; set; }

    public DbSet<ProductSkuReActivation> ProductSkuReActivations { get; set; }

    public DbSet<NotificationHistory> NotificationHistory { get; set; }

    public DbSet<JobExecutionLog> JobExecutionLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter to exclude archived products
        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => p.ArchivedAt == null);

        // Seed Adapter data
        modelBuilder.Entity<Adapter>().HasData(
            new Adapter
            {
                Id = 1,
                Name = "Zara",
                CreatedAt = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc)
            },
            new Adapter
            {
                Id = 2,
                Name = "PullAndBear",
                CreatedAt = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
