using Hangfire;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Data;

namespace StoreScrapper.Jobs;

public class HangfireJobManager
{
    private readonly AppDbContext _dbContext;

    public HangfireJobManager(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Sets up individual recurring jobs for each enabled product
    /// </summary>
    public async Task SetupRecurringJobsAsync()
    {
        Console.WriteLine("[Hangfire] Cleaning up old recurring jobs...");

        // Remove ALL existing recurring jobs (clean slate on startup)
        var allProducts = await _dbContext.Products.ToListAsync();
        foreach (var product in allProducts)
        {
            RemoveProductJob(product.Id);
        }

        // Also remove any old coordinator job if it exists
        RecurringJob.RemoveIfExists("scraping-coordinator");

        Console.WriteLine("[Hangfire] Setting up recurring jobs for enabled products...");

        var enabledProducts = allProducts.Where(x => x.IsEnabled).ToList();

        foreach (var product in enabledProducts)
        {
            ScheduleProductJob(product.Id, product.CheckIntervalSeconds);
        }

        Console.WriteLine($"[Hangfire] Set up {enabledProducts.Count} recurring jobs");
    }

    /// <summary>
    /// Schedules or updates a recurring job for a specific product
    /// </summary>
    public static void ScheduleProductJob(int productId, int intervalSeconds)
    {
        var jobId = $"product-{productId}";

        // Generate cron expression based on interval
        var cronExpression = GenerateCronExpression(intervalSeconds);

        RecurringJob.AddOrUpdate<StoreScrapingJob>(
            jobId,
            job => job.ExecuteAsync(productId),
            cronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        Console.WriteLine($"[Product {productId}] Recurring job scheduled - runs every {intervalSeconds}s");
    }

    /// <summary>
    /// Removes a product's recurring job
    /// </summary>
    public static void RemoveProductJob(int productId)
    {
        var jobId = $"product-{productId}";
        RecurringJob.RemoveIfExists(jobId);
        Console.WriteLine($"[Product {productId}] Recurring job removed");
    }

    /// <summary>
    /// Pauses all scraping by removing all product jobs
    /// </summary>
    public static async Task PauseAllScrapingAsync(AppDbContext dbContext)
    {
        var products = await dbContext.Products.ToListAsync();
        foreach (var product in products)
        {
            RemoveProductJob(product.Id);
        }
        Console.WriteLine("[Hangfire] All scraping paused");
    }

    /// <summary>
    /// Resumes scraping by recreating all jobs
    /// </summary>
    public async Task ResumeAllScrapingAsync()
    {
        await SetupRecurringJobsAsync();
        Console.WriteLine("[Hangfire] Scraping resumed");
    }

    /// <summary>
    /// Generates a cron expression based on interval in seconds
    /// </summary>
    private static string GenerateCronExpression(int intervalSeconds)
    {
        return intervalSeconds switch
        {
            < 60 when 60 % intervalSeconds == 0 => $"*/{intervalSeconds} * * * * *", // Every N seconds (if divides 60)
            60 => Cron.Minutely(),
            120 => "*/2 * * * *",
            180 => "*/3 * * * *",
            300 => "*/5 * * * *",
            600 => "*/10 * * * *",
            _ => Cron.Minutely() // Default fallback
        };
    }
}
