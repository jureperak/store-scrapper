using StoreScrapper.Models;

namespace StoreScrapper.Adapters;

public interface IStoreAdapter
{
    Task<AdapterResult> FetchAndProcessAsync(string availabilityUrl, List<int> neededProductSkus);
}
