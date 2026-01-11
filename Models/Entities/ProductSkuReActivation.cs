namespace StoreScrapper.Models.Entities;

public class ProductSkuReActivation
{
    public int Id { get; set; }

    public string ReEnableUrl { get; set; } = string.Empty;

    public bool IsUsed { get; set; }
    
    public DateTime ValidTo { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    
    public ProductSku ProductSku { get; set; } = null!;
}
