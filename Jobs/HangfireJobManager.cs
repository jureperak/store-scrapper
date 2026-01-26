using Hangfire;

namespace StoreScrapper.Jobs;

public class HangfireJobManager
{
    /// <summary>
    /// Sets up the recurring coordinator job that manages all product scraping
    /// </summary>
    public static void SetupRecurringJobs()
    {
        // Run coordinator every minute to check which products need scraping
        RecurringJob.AddOrUpdate<ScrapingCoordinatorJob>(
            "scraping-coordinator",
            job => job.CoordinateAsync(),
            Cron.Minutely, // Runs every minute
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        Console.WriteLine("[Hangfire] Coordinator job set up - runs every minute");
    }

    /// <summary>
    /// Pauses all scraping by removing the coordinator job
    /// </summary>
    public static void PauseAllScraping()
    {
        RecurringJob.RemoveIfExists("scraping-coordinator");
        Console.WriteLine("[Hangfire] All scraping paused");
    }

    /// <summary>
    /// Resumes scraping by recreating the coordinator job
    /// </summary>
    public static void ResumeAllScraping()
    {
        SetupRecurringJobs();
        Console.WriteLine("[Hangfire] Scraping resumed");
    }
}
