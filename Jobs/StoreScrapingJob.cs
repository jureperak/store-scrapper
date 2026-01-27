using Hangfire;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Data;
using StoreScrapper.Services;

namespace StoreScrapper.Jobs;

/// <summary>
/// Individual product scraping job - runs as recurring job
/// </summary>
public class StoreScrapingJob
{
    private readonly IStoreScrapingService _storeScrapingService;
    private readonly AppDbContext _dbContext;

    public StoreScrapingJob(IStoreScrapingService storeScrapingService, AppDbContext dbContext)
    {
        _storeScrapingService = storeScrapingService;
        _dbContext = dbContext;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(int productId)
    {
        // Get product details to check if still enabled
        var product = await _dbContext.Products
            .Where(x => x.Id == productId)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            Console.WriteLine($"[Product {productId}] Not found");
            return;
        }

        if (!product.IsEnabled)
        {
            Console.WriteLine($"[Product {productId}] Disabled");
            return;
        }

        // Execute the scraping
        var result = await _storeScrapingService.ScrapeStoreAsync(productId);

        if (!result.Success)
        {
            Console.WriteLine($"[Product {productId}] Failed: {result.Message}");
        }
        else if (result.ProductSkusFound > 0)
        {
            Console.WriteLine($"[Product {productId}] Found {result.ProductSkusFound} SKUs, sent notification.");
        }
        else
        {
            Console.WriteLine($"[Product {productId}] Checked, no new SKUs available.");
        }
    }
}
