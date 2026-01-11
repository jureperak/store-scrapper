namespace StoreScrapper.Models.Entities;

public class JobExecutionLog
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    public int? NotificationHistoryId { get; set; }
    
    public DateTime ExecutedAt { get; set; }
    
    public bool Success { get; set; }

    public TimeSpan Duration { get; set; }
    
    public string? ErrorMessage { get; set; }

    // Navigation property
    public Product Product { get; set; } = null!;
    
    // product skus found
    public List<ProductSku> ProductSkus { get; set; } = [];
    
    public NotificationHistory? NotificationHistory { get; set; }
}
