using Hangfire;
using StoreScrapper.Models.Entities;

namespace StoreScrapper.Jobs;

public class HangfireJobManager
{
    /// <summary>
    /// Schedules the initial job for a product. Job will reschedule itself after each execution.
    /// </summary>
    public void ScheduleStoreJob(Product product)
    {
        if (!product.IsEnabled)
        {
            // Product is disabled, don't schedule anything
            product.HangfireJobId = null;
            return;
        }

        var jobId = $"product-{product.Id}";

        // Schedule the first execution immediately (or after a short delay)
        var scheduledJobId = BackgroundJob.Schedule<StoreScrapingJob>(
            job => job.ExecuteAsync(product.Id),
            TimeSpan.FromSeconds(1) // Start after 1 second
        );

        product.HangfireJobId = jobId;
        Console.WriteLine($"[Product {product.Id}] Initial job scheduled with ID: {scheduledJobId}");
    }

    /// <summary>
    /// Schedules the next execution for a product after the specified delay
    /// </summary>
    public static string ScheduleNextExecution(int productId, int delaySeconds)
    {
        var scheduledJobId = BackgroundJob.Schedule<StoreScrapingJob>(
            job => job.ExecuteAsync(productId),
            TimeSpan.FromSeconds(delaySeconds)
        );

        Console.WriteLine($"[Product {productId}] Next job scheduled in {delaySeconds}s with ID: {scheduledJobId}");
        return scheduledJobId;
    }
}
