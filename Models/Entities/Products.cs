namespace StoreScrapper.Models.Entities;

public class Product
{
    public int Id { get; set; }
    
    public int AdapterId { get; set; }
    
    public string ProductPageUrl { get; set; } = string.Empty;
    
    public string AvailabilityUrl { get; set; } = string.Empty;
    
    public bool IsEnabled { get; set; } = true;
    
    public int CheckIntervalSeconds { get; set; }
    
    public string? HangfireJobId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    public DateTime? ArchivedAt { get; set; }

    // Navigation properties
    
    public Adapter Adapter { get; set; } = null!;

    public List<ProductSku> ProductSkus { get; set; } = [];
}
