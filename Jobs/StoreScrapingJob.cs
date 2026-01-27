using Hangfire;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Data;
using StoreScrapper.Services;

namespace StoreScrapper.Jobs;

/// <summary>
/// Coordinator job that runs every 5 seconds and enqueues all enabled products
/// </summary>
public class ScrapingCoordinatorJob
{
    private readonly AppDbContext _dbContext;

    public ScrapingCoordinatorJob(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Runs every 5 seconds, enqueues enabled products that have active SKUs
    /// </summary>
    [AutomaticRetry(Attempts = 1)]
    public async Task CoordinateAsync()
    {
        // Only get products that are enabled AND have at least one active SKU
        var productsToScrape = await _dbContext.Products
            .Where(x => x.IsEnabled)
            .Where(x => x.ProductSkus.Any(sku => !sku.TemporaryDisabled))
            .Select(x => x.Id)
            .ToListAsync();

        if (productsToScrape.Any())
        {
            Console.WriteLine($"[Coordinator] Enqueuing {productsToScrape.Count} products with active SKUs");

            foreach (var productId in productsToScrape)
            {
                // Enqueue job for immediate execution
                BackgroundJob.Enqueue<StoreScrapingJob>(job => job.ExecuteAsync(productId));
            }
        }
        else
        {
            Console.WriteLine("[Coordinator] No products with active SKUs to scrape");
        }
    }
}

/// <summary>
/// Individual product scraping job - executed by coordinator
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
