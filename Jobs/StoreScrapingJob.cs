using Hangfire;
using StoreScrapper.Services;

namespace StoreScrapper.Jobs;

public class StoreScrapingJob
{
    private readonly IStoreScrapingService _storeScrapingService;

    public StoreScrapingJob(IStoreScrapingService storeScrapingService)
    {
        _storeScrapingService = storeScrapingService;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(int productId)
    {
        var result = await _storeScrapingService.ScrapeStoreAsync(productId);

        if (!result.Success)
        {
            Console.WriteLine($"[Product {productId}] Failed: {result.Message}");
        }
        else if (result.ProductSkusFound > 0)
        {
            Console.WriteLine($"[Product {productId}] Found {result.ProductSkusFound} SKUs, sent notification.");
        }
    }
}
