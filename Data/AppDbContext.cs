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
    
    public DbSet<NotificationHistory> NotificationHistory { get; set; }
    
    public DbSet<JobExecutionLog> JobExecutionLogs { get; set; }
}
