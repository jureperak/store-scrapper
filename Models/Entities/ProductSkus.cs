namespace StoreScrapper.Models.Entities;

public class ProductSku
{
    public int Id { get; set; }

    public int Sku { get; set; } 
    
    public int ProductId { get; set; }
    
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    
    public Product Product { get; set; } = null!;
    
    public List<NotificationHistory> NotificationHistories { get; set; } = new();
}
