namespace StoreScrapper.Models;

public class StoreConfiguration
{
    public List<StoreItem> Stores { get; set; } = new();
}

public class StoreItem
{
    public string Adapter { get; set; } = string.Empty;
    public string AvailabilityUrl { get; set; } = string.Empty;
    public string ProductPageUrl { get; set; } = string.Empty;
    public int? NeededProduct { get; set; }
    public List<int> ProductsToAvoid { get; set; } = [];
}
