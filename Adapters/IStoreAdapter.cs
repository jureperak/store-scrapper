using Microsoft.Extensions.Options;
using StoreScrapper.Models;

namespace StoreScrapper.Adapters;

public interface IStoreAdapter
{
    Task<bool> FetchAndProcessAsync(
        string availabilityUrl,
        string productPageUrl
    );
}
