using StoreScrapper.Models.Entities;

namespace StoreScrapper.Models.ViewModels;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = null!;
    public List<JobExecutionLog> JobExecutionLogs { get; set; } = new();
    public List<NotificationHistory> NotificationHistory { get; set; } = new();
}
