using Hangfire;
using StoreScrapper.Models.Entities;

namespace StoreScrapper.Jobs;

public class HangfireJobManager
{
    public void ScheduleStoreJob(Product product)
    {
        if (!product.IsEnabled)
        {
            RemoveStoreJob(product);
            return;
        }

        var jobId = $"product-{product.Id}";

        // Schedule recurring job based on seconds
        // Convert seconds to appropriate cron expression
        string cronExpression;

        if (product.CheckIntervalSeconds < 60)
        {
            // For intervals less than 1 minute, use seconds (Hangfire supports this via custom storage)
            // We'll use the minimum granularity of 1 minute but note this in logs
            cronExpression = "* * * * *"; // Every minute (minimum for standard cron)
        }
        else if (product.CheckIntervalSeconds % 60 == 0)
        {
            // Evenly divisible by 60, use minute-based cron
            var minutes = product.CheckIntervalSeconds / 60;

            if (minutes < 60)
            {
                cronExpression = $"*/{minutes} * * * *";
            }
            else if (minutes == 60)
            {
                cronExpression = Cron.Hourly();
            }
            else
            {
                // For intervals > 60 minutes, run every hour
                cronExpression = Cron.Hourly();
            }
        }
        else
        {
            // Not evenly divisible by 60, round up to nearest minute
            var minutes = (product.CheckIntervalSeconds + 59) / 60;
            cronExpression = $"*/{minutes} * * * *";
        }

        RecurringJob.AddOrUpdate<StoreScrapingJob>(
            jobId,
            job => job.ExecuteAsync(product.Id),
            cronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            }
        );

        product.HangfireJobId = jobId;
    }

    public void RemoveStoreJob(Product product)
    {
        if (!string.IsNullOrEmpty(product.HangfireJobId))
        {
            RecurringJob.RemoveIfExists(product.HangfireJobId);
            product.HangfireJobId = null;
        }
    }
}
