using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    private readonly AppOptions _appOptions;
    private readonly INotificationService _notificationService;
    private readonly IZaraAdapter _zaraAdapter;
    private readonly IPullAndBearAdapter _pullAndBearAdapter;

    public StoreScrapingService(AppDbContext dbContext, INotificationService notificationService,IZaraAdapter zaraAdapter,IPullAndBearAdapter pullAndBearAdapter, IOptions<AppOptions> appOptions)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _zaraAdapter = zaraAdapter;
        _pullAndBearAdapter = pullAndBearAdapter;
        _appOptions = appOptions.Value;
    }

    public async Task<ScrapingResult> ScrapeStoreAsync(int productId)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 1. Load product from database
            var product = await _dbContext.Products
                .Include(x => x.ProductSkus
                    .Where(y => !y.TemporaryDisabled))
                .ThenInclude(x => x.ProductSkuReActivations
                    .Where(r => !r.IsUsed))
                .Include(x => x.Adapter)
                .Where(x => x.Id == productId)
                .FirstOrDefaultAsync();
            
            if (product == null)
            {
                stopwatch.Stop();
                await LogExecutionAsync(productId, false, "Product not found", null, [], stopwatch.Elapsed);
                return ScrapingResult.CreateError("Product not found");
            }

            if (!product.IsEnabled)
            {
                stopwatch.Stop();
                await LogExecutionAsync(productId, false, "Product is disabled", null, [], stopwatch.Elapsed);
                return ScrapingResult.CreateSkipped("Product is disabled");
            }

            // 2. Get appropriate adapter
            var adapter = GetAdapter(product.Adapter.Name);

            if (adapter == null)
            {
                stopwatch.Stop();
                await LogExecutionAsync(productId, false, $"Unknown adapter: {product.Adapter}", null, [], stopwatch.Elapsed);
                return ScrapingResult.CreateError($"Unknown adapter: {product.Adapter.Name}");
            }

            if(!product.ProductSkus.Any())
            {
                stopwatch.Stop();
                await LogExecutionAsync(productId, false, "There are no active SKUs to check", null, [], stopwatch.Elapsed);
                return ScrapingResult.CreateSkipped("There are no active SKUs to check");
            }

            // 5. Execute scraping
            var result = await adapter.FetchAndProcessAsync(product.AvailabilityUrl, product.ProductSkus.Select(x => x.Sku).ToList());

            if (!result.Success)
            {
                stopwatch.Stop();
                await LogExecutionAsync(productId, false, result.ErrorMessage, null, [], stopwatch.Elapsed);
                return ScrapingResult.CreateError(result.ErrorMessage ?? "Unknown error");
            }
            
            NotificationHistory? notificationHistory = null;

            // 6. Send notifications for new products
            if (result.AvailableProductSkus.Any())
            {
                var productSkusFound = product.ProductSkus
                    .Where(x => result.AvailableProductSkus.Contains(x.Sku))
                    .ToList();
                
                foreach (var productSku in productSkusFound)
                {
                    productSku.TemporaryDisabled = true;
                    productSku.UpdatedAt = DateTime.UtcNow;
                    
                    foreach (var productSkuProductSkuReActivation in productSku.ProductSkuReActivations)
                    {
                        productSkuProductSkuReActivation.IsUsed = true;
                    }
                    
                    productSku.ProductSkuReActivations.Add(new ProductSkuReActivation()
                    {
                        CreatedAt = DateTime.UtcNow,
                        ReEnableUrl = $"{_appOptions.BaseUrl}/productsku/{Guid.NewGuid()}/reactivate",
                        ValidTo = DateTime.UtcNow.AddMinutes(30),
                        IsUsed = false,
                    });
                }
                
                try
                {
                    // Send notification
                    var emailBody = await _notificationService.SendMailAsync(product.Id, productSkusFound);
                    var whatsAppBody = await _notificationService.SendWhatsAppAsync(product.Id, productSkusFound);

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
            stopwatch.Stop();
            await LogExecutionAsync(
                productId,
                true,
                null,
                notificationHistory?.Id,
                product.ProductSkus
                    .Where(x => result.AvailableProductSkus.Contains(x.Sku))
                    .ToList(),
                stopwatch.Elapsed);

            return ScrapingResult.CreateSuccess(result.AvailableProductSkus.Count);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogExecutionAsync(productId, false, ex.Message, null, [], stopwatch.Elapsed);
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

    private async Task LogExecutionAsync(int productId, bool success, string? errorMessage, int? notificationHistoryId, List<ProductSku> productSkusFound, TimeSpan duration)
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
                NotificationHistoryId = notificationHistoryId,
                Duration = duration
            });

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to log execution: {ex.Message}");
        }
    }
}
