using Hangfire;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Data;
using StoreScrapper.Models.Entities;
using StoreScrapper.Services;

namespace StoreScrapper.Jobs;

/// <summary>
/// Coordinator job that runs every minute and checks which products need scraping
/// </summary>
public class ScrapingCoordinatorJob
{
    private readonly AppDbContext _dbContext;

    public ScrapingCoordinatorJob(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Runs every minute, checks which products are due for scraping, and enqueues them
    /// </summary>
    [AutomaticRetry(Attempts = 1)]
    public async Task CoordinateAsync()
    {
        var now = DateTime.UtcNow;

        // Find all enabled products
        var enabledProducts = await _dbContext.Products
            .Where(x => x.IsEnabled)
            .ToListAsync();

        var productsToScrape = new List<Product>();

        foreach (var product in enabledProducts)
        {
            // Get last execution for this product
            var lastExecution = await _dbContext.JobExecutionLogs
                .Where(x => x.ProductId == product.Id)
                .OrderByDescending(x => x.ExecutedAt)
                .FirstOrDefaultAsync();

            // Never run before, or due for next check
            if (lastExecution == null)
            {
                productsToScrape.Add(product);
            }
            else
            {
                var nextCheckTime = lastExecution.ExecutedAt.AddSeconds(product.CheckIntervalSeconds);
                if (nextCheckTime <= now)
                {
                    productsToScrape.Add(product);
                }
            }
        }

        if (productsToScrape.Any())
        {
            Console.WriteLine($"[Coordinator] Found {productsToScrape.Count} products due for scraping");

            foreach (var product in productsToScrape)
            {
                // Enqueue individual scraping job
                BackgroundJob.Enqueue<StoreScrapingJob>(job => job.ExecuteAsync(product.Id));
            }
        }
        else
        {
            Console.WriteLine($"[Coordinator] No products due for scraping");
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
