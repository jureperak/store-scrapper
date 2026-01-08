namespace StoreScrapper.Adapters;

public interface IStoreAdapter
{
    Task<bool> FetchAndProcessAsync(
        string availabilityUrl,
        string productPageUrl,
        int? neededProduct,
        List<int> productsToAvoid
    );
}
