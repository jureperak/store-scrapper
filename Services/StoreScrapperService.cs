using Microsoft.Extensions.Options;
using StoreScrapper.Adapters;
using StoreScrapper.Models;

namespace StoreScrapper.Services;

public class StoreScrapperService
{
    private readonly ZaraAdapter _zaraAdapter;
    private readonly PullAndBearAdapter _pullAndBearAdapter;
    private readonly StoreConfiguration _storeConfiguration;

    public StoreScrapperService(
        ZaraAdapter zaraAdapter,
        PullAndBearAdapter pullAndBearAdapter,
        IOptions<StoreConfiguration> storeConfiguration)
    {
        _zaraAdapter = zaraAdapter;
        _pullAndBearAdapter = pullAndBearAdapter;
        _storeConfiguration = storeConfiguration.Value;
    }

    public async Task RunAsync()
    {
        var numberOfRequests = 1;
        var continueCrawling = true;

        while (continueCrawling)
        {
            foreach (var store in _storeConfiguration.Stores)
            {
                var adapter = GetAdapter(store.Adapter);

                if (adapter == null)
                {
                    Console.WriteLine($"Unknown adapter: {store.Adapter}");
                    continue;
                }

                continueCrawling = await adapter.FetchAndProcessAsync(
                    store.AvailabilityUrl,
                    store.ProductPageUrl,
                    store.NeededProduct,
                    store.ProductsToAvoid
                );

                if (!continueCrawling)
                {
                    break;
                }
            }

            if (continueCrawling)
            {
                Console.WriteLine($"Request #{numberOfRequests++}");
                Thread.Sleep(4000);
            }
        }
    }

    private IStoreAdapter? GetAdapter(string adapterName)
    {
        return adapterName.ToLower() switch
        {
            "zara" => _zaraAdapter,
            "pullandbear" => _pullAndBearAdapter,
            _ => null
        };
    }
}
