namespace StoreScrapper.Models.ViewModels;

public class ReactivationResultViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorType { get; set; }
    public string? ProductSkuName { get; set; }
    public int? Sku { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public DateTime? ExpiredAt { get; set; }
}
