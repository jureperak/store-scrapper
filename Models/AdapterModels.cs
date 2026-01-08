namespace StoreScrapper.Models.AdapterModels;

public class ZaraModel
{
    public List<SkusAvailabilityZaraModel> skusAvailability { get; set; }
}

public class SkusAvailabilityZaraModel
{
    public int sku { get; set; }

    public string availability { get; set; }
}

public class PullAndBearModel
{
    public List<ParentStockPullAndBearModel> stocks { get; set; }
}

public class ParentStockPullAndBearModel
{
    public int productId { get; set; }
    public List<StockPullAndBearModel> stocks { get; set; }
}

public class StockPullAndBearModel
{
    public int id { get; set; }
    public string availability { get; set; }
    public string typeThreshold { get; set; }
}
