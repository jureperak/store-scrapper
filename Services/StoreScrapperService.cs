using StoreScrapper.Adapters;

namespace StoreScrapper.Services;

public class StoreScrapperService
{
    private readonly ZaraAdapter _zaraAdapter;
    private readonly PullAndBearAdapter _pullAndBearAdapter;

    public StoreScrapperService(
        ZaraAdapter zaraAdapter,
        PullAndBearAdapter pullAndBearAdapter)
    {
        _zaraAdapter = zaraAdapter;
        _pullAndBearAdapter = pullAndBearAdapter;
    }

    public async Task RunAsync()
    {
        var numberOfRequests = 1;
        var continueCrawling = true;

        while (continueCrawling)
        {
            if (continueCrawling)
            {
                Console.WriteLine($"Request #{numberOfRequests++}");
                Thread.Sleep(4000);
            }
            
            continueCrawling = false;
        }
    }

    private async Task<bool> ScrapeStore(
        IStoreAdapter adapter,
        string availabilityUrl,
        string productPageUrl)
    {
        return await adapter.FetchAndProcessAsync(
            availabilityUrl,
            productPageUrl
        );
    }
}
