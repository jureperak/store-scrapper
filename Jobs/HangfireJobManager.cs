using Hangfire;
using Hangfire.Storage;

namespace StoreScrapper.Jobs;

public static class HangfireJobManager
{
    /// <summary>
    /// Cleans up ALL Hangfire jobs and sets up the coordinator
    /// </summary>
    public static void SetupRecurringJobs()
    {
        Console.WriteLine("[Hangfire] Cleaning up all existing jobs...");

        // Clean up recurring jobs
        using (var connection = JobStorage.Current.GetConnection())
        {
            foreach (var recurringJob in connection.GetRecurringJobs())
            {
                RecurringJob.RemoveIfExists(recurringJob.Id);
                Console.WriteLine($"[Hangfire] Removed recurring job: {recurringJob.Id}");
            }
        }

        // Clean up scheduled jobs
        var monitor = JobStorage.Current.GetMonitoringApi();
        var scheduledJobs = monitor.ScheduledJobs(0, int.MaxValue);
        foreach (var job in scheduledJobs)
        {
            BackgroundJob.Delete(job.Key);
        }
        Console.WriteLine($"[Hangfire] Removed {scheduledJobs.Count} scheduled jobs");

        // Clean up enqueued jobs
        var enqueuedJobs = monitor.EnqueuedJobs("default", 0, int.MaxValue);
        foreach (var job in enqueuedJobs)
        {
            BackgroundJob.Delete(job.Key);
        }
        Console.WriteLine($"[Hangfire] Removed {enqueuedJobs.Count} enqueued jobs");

        // Set up coordinator job - runs every 5 seconds
        RecurringJob.AddOrUpdate<ScrapingCoordinatorJob>(
            "scraping-coordinator",
            job => job.CoordinateAsync(),
            "*/5 * * * * *", // Every 5 seconds
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        Console.WriteLine("[Hangfire] Coordinator job set up - runs every 5 seconds");
    }

    /// <summary>
    /// Pauses all scraping by removing the coordinator job
    /// </summary>
    public static void PauseAllScraping()
    {
        RecurringJob.RemoveIfExists("scraping-coordinator");
        Console.WriteLine("[Hangfire] All scraping paused (coordinator removed)");
    }

    /// <summary>
    /// Resumes scraping by recreating the coordinator job
    /// </summary>
    public static void ResumeAllScraping()
    {
        RecurringJob.AddOrUpdate<ScrapingCoordinatorJob>(
            "scraping-coordinator",
            job => job.CoordinateAsync(),
            "*/5 * * * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        Console.WriteLine("[Hangfire] Scraping resumed (coordinator recreated)");
    }
}
