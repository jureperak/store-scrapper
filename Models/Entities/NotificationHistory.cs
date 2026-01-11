namespace StoreScrapper.Models.Entities;

public class NotificationHistory
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    public string ProductPageUrl { get; set; } = string.Empty;
    
    public bool EmailSent { get; set; }

    public string? EmailBody { get; set; }
    
    public bool WhatsAppSent { get; set; }
    
    public string? WhatsAppBody { get; set; }
    
    public DateTime SentAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation property
    public Product Product { get; set; } = null!;
    
    public List<ProductSku> ProductSkus { get; set; } = [];
}
