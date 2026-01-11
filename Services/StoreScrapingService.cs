using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Adapters;
using StoreScrapper.Data;
using StoreScrapper.Models;
using StoreScrapper.Models.Entities;

namespace StoreScrapper.Services;

public interface IStoreScrapingService
{
    Task<ScrapingResult> ScrapeStoreAsync(int productId);
}

public class StoreScrapingService : IStoreScrapingService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IZaraAdapter _zaraAdapter;
    private readonly IPullAndBearAdapter _pullAndBearAdapter;

    public StoreScrapingService(AppDbContext dbContext,INotificationService notificationService,IZaraAdapter zaraAdapter,IPullAndBearAdapter pullAndBearAdapter)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _zaraAdapter = zaraAdapter;
        _pullAndBearAdapter = pullAndBearAdapter;
    }

    public async Task<ScrapingResult> ScrapeStoreAsync(int productId)
    {
        try
        {
            // 1. Load product from database
            var product = await _dbContext.Products
                .Include(p => p.ProductSkus)
                .Include(p => p.Adapter)
                .Where(x => x.Id == productId)
                .FirstOrDefaultAsync();
            
            if (product == null)
            {
                return ScrapingResult.CreateError("Product not found");
            }

            if (!product.IsEnabled)
            {
                return ScrapingResult.CreateSkipped("Product is disabled");
            }

            // 2. Get appropriate adapter
            var adapter = GetAdapter(product.Adapter.Name);
            
            if (adapter == null)
            {
                await LogExecutionAsync(productId, false, $"Unknown adapter: {product.Adapter}", null, []);
                return ScrapingResult.CreateError($"Unknown adapter: {product.Adapter.Name}");
            }

            // 3. Check if products were already notified
            var alreadyNotified = await _dbContext.NotificationHistory
                .Where(n => n.ProductId == product.Id)
                .Where(n => n.SentAt.AddMinutes(3) > DateTime.UtcNow)
                .AnyAsync();
            
            if(alreadyNotified)
            {
                return ScrapingResult.CreateSkipped("Products were already notified recently");
            }

            // 5. Execute scraping
            var result = await adapter.FetchAndProcessAsync(product.AvailabilityUrl, product.ProductSkus.Select(x => x.Sku).ToList());

            if (!result.Success)
            {
                await LogExecutionAsync(productId, false, result.ErrorMessage, null, []);
                return ScrapingResult.CreateError(result.ErrorMessage ?? "Unknown error");
            }
            
            NotificationHistory? notificationHistory = null;

            // 6. Send notifications for new products
            if (result.AvailableProductSkus.Any())
            {
                var skusToNotify = product.ProductSkus
                    .Where(x => result.AvailableProductSkus.Contains(x.Sku))
                    .Select(x => (x.Sku, x.Name))
                    .ToList();
                
                try
                {
                    // Send notification
                    var emailBody = await _notificationService.SendMailAsync(product.ProductPageUrl, skusToNotify);
                    var whatsAppBody = await _notificationService.SendWhatsAppAsync(product.ProductPageUrl, skusToNotify);

                    // Record in database
                    notificationHistory = new NotificationHistory
                    {
                        ProductId = product.Id,
                        ProductSkus = product.ProductSkus
                            .Where(x => result.AvailableProductSkus.Contains(x.Sku))
                            .ToList(),
                        ProductPageUrl = product.ProductPageUrl,
                        EmailSent = true,
                        EmailBody = emailBody,
                        WhatsAppSent = true,
                        WhatsAppBody = whatsAppBody,
                        SentAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    await _dbContext.NotificationHistory.AddAsync(notificationHistory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification for product {product.Id}: {ex.Message}");
                }

                await _dbContext.SaveChangesAsync();
            }

            // 7. Log execution
            await LogExecutionAsync(
                productId, 
                true, 
                null, 
                notificationHistory?.Id, 
                product.ProductSkus
                    .Where(x => result.AvailableProductSkus.Contains(x.Sku))
                    .ToList());

            return ScrapingResult.CreateSuccess(result.AvailableProductSkus.Count);
        }
        catch (Exception ex)
        {
            await LogExecutionAsync(productId, false, ex.Message, null, []);
            return ScrapingResult.CreateError($"Exception: {ex.Message}");
        }
    }

    private IStoreAdapter? GetAdapter(string adapterName)
    {
        return adapterName.ToLower() switch
        {
            "zara" => _zaraAdapter,
            "pullandbear" => _pullAndBearAdapter,
            _ => null
        };
    }

    private async Task LogExecutionAsync(int productId, bool success, string? errorMessage, int? notificationHistoryId, List<ProductSku> productSkusFound)
    {
        try
        {
            await _dbContext.JobExecutionLogs.AddAsync(new JobExecutionLog
            {
                ProductId = productId,
                ProductSkus = productSkusFound,
                ExecutedAt = DateTime.UtcNow,
                Success = success,
                ErrorMessage = errorMessage,
                NotificationHistoryId = notificationHistoryId
            });

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to log execution: {ex.Message}");
        }
    }
}
