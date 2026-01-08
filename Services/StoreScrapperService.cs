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
            continueCrawling = await ScrapeStore(
                _zaraAdapter,
                "https://www.zara.com/itxrest/1/catalog/store/11737/product/id/316571537/availability?ajax=true",
                "https://www.zara.com/hr/hr/kratka-jakna-od-umjetnog-krzna-p06318261.html?v1=316571537&utm_campaign=productShare&utm_medium=mobile_sharing_iOS&utm_source=red_social_movil"
            );

            if (continueCrawling)
            {
                Console.WriteLine($"Request #{numberOfRequests++}");
                Thread.Sleep(4000);
            }
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
