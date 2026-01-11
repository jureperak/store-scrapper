using System.Text.Json.Serialization;

namespace StoreScrapper.Models;

public class ZaraModel
{
    [JsonPropertyName("skusAvailability")]
    public List<SkusAvailabilityZaraModel> SkusAvailability { get; set; } = [];
}

public class SkusAvailabilityZaraModel
{
    [JsonPropertyName("sku")]
    public int Sku { get; set; }

    [JsonPropertyName("availability")]
    public string Availability { get; set; } = string.Empty;
}

public class PullAndBearModel
{
    [JsonPropertyName("stocks")]
    public List<ParentStockPullAndBearModel> Stocks { get; set; } = [];
}

public class ParentStockPullAndBearModel
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [JsonPropertyName("stocks")]
    public List<StockPullAndBearModel> Stocks { get; set; } = [];
}

public class StockPullAndBearModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("availability")]
    public string Availability { get; set; } = string.Empty;

    [JsonPropertyName("typeThreshold")]
    public string TypeThreshold { get; set; } = string.Empty;
}
