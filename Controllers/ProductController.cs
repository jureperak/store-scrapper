using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Data;
using StoreScrapper.Jobs;
using StoreScrapper.Models.Entities;
using StoreScrapper.Models.ViewModels;
using StoreScrapper.Services;

namespace StoreScrapper.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly HangfireJobManager _jobManager;
    private readonly IProductPageScraperService _scraperService;

    public ProductController(AppDbContext dbContext, HangfireJobManager jobManager, IProductPageScraperService scraperService)
    {
        _dbContext = dbContext;
        _jobManager = jobManager;
        _scraperService = scraperService;
    }

    // GET: /Stores
    public async Task<IActionResult> Index()
    {
        var products = await _dbContext.Products
            .Include(x => x.Adapter)
            .Include(x => x.ProductSkus)
            .ThenInclude(x => x.ProductSkuReActivations)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(products);
    }

    // GET: /Stores/Create
    public IActionResult Create()
    {
        return View(new ProductFormViewModel());
    }

    // POST: /Stores/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var adapter = await _dbContext.Adapters
            .Where(x => x.Name == model.Adapter)
            .FirstOrDefaultAsync();

        if (adapter == null)
        {
            ModelState.AddModelError("Adapter", $"Adapter {model.Adapter} not found!");
            return View(model);
        }

        Console.WriteLine($"Creating product with AvailabilityUrl: {model.AvailabilityUrl ?? "NULL"}");

        var product = new Product
        {
            AdapterId = adapter.Id,
            AvailabilityUrl = model.AvailabilityUrl ?? string.Empty,
            ProductPageUrl = model.ProductPageUrl,
            Name = model.ProductName ?? "Unknown Product",
            IsEnabled = model.IsEnabled,
            CheckIntervalSeconds = 5,
            CreatedAt = DateTime.UtcNow,
            ProductSkus = new()
        };

        // Parse and add selected SKUs as ProductSkus
        if (!string.IsNullOrWhiteSpace(model.SelectedSkusJson))
        {
            try
            {
                Console.WriteLine($"SelectedSkusJson received: {model.SelectedSkusJson}");
                var selectedSkus = System.Text.Json.JsonSerializer.Deserialize<List<SkuData>>(model.SelectedSkusJson);
                Console.WriteLine($"Deserialized {selectedSkus?.Count ?? 0} SKUs");
                if (selectedSkus != null)
                {
                    foreach (var skuData in selectedSkus)
                    {
                        Console.WriteLine($"Adding SKU: Name={skuData.Name}, Sku={skuData.Sku}");
                        product.ProductSkus.Add(new ProductSku
                        {
                            Name = skuData.Name,
                            Sku = skuData.Sku,
                            CreatedAt = DateTime.UtcNow,
                            TemporaryDisabled = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing SKUs: {ex.Message}");
                // Ignore JSON parsing errors
            }
        }
        else
        {
            Console.WriteLine("SelectedSkusJson is null or empty");
        }

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        // Schedule Hangfire job
        _jobManager.ScheduleStoreJob(product);
        await _dbContext.SaveChangesAsync();

        TempData["Success"] = "Product created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Stores/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _dbContext.Products
            .Include(x => x.Adapter)
            .Include(x => x.ProductSkus)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        var selectedSkus = product.ProductSkus.Select(s => new SkuData
        {
            Name = s.Name,
            Sku = s.Sku
        }).ToList();

        var viewModel = new ProductFormViewModel
        {
            Id = product.Id,
            Adapter = product.Adapter.Name,
            AvailabilityUrl = product.AvailabilityUrl,
            ProductPageUrl = product.ProductPageUrl,
            ProductName = product.Name,
            IsEnabled = product.IsEnabled,
            SelectedSkusJson = System.Text.Json.JsonSerializer.Serialize(selectedSkus)
        };

        return View(viewModel);
    }

    // POST: /Stores/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var product = await _dbContext.Products
            .Include(x => x.ProductSkus)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        var adapter = await _dbContext.Adapters
            .Where(x => x.Name == model.Adapter)
            .FirstOrDefaultAsync();

        if (adapter == null)
        {
            ModelState.AddModelError("Adapter", $"Adapter {model.Adapter} not found!");
            return View(model);
        }

        // Update store properties
        product.AdapterId = adapter.Id;
        product.AvailabilityUrl = model.AvailabilityUrl ?? product.AvailabilityUrl;
        product.ProductPageUrl = model.ProductPageUrl;
        product.Name = model.ProductName ?? product.Name;
        product.IsEnabled = model.IsEnabled;
        product.UpdatedAt = DateTime.UtcNow;

        // Update ProductSkus
        if (!string.IsNullOrWhiteSpace(model.SelectedSkusJson))
        {
            try
            {
                var selectedSkus = System.Text.Json.JsonSerializer.Deserialize<List<SkuData>>(model.SelectedSkusJson);
                if (selectedSkus != null)
                {
                    var selectedSkuSet = selectedSkus.Select(s => s.Sku).ToHashSet();
                    var existingSkuSet = product.ProductSkus.Select(s => s.Sku).ToHashSet();

                    // Archive ProductSkus not present in selection
                    foreach (var sku in product.ProductSkus)
                    {
                        if (!selectedSkuSet.Contains(sku.Sku) && sku.ArchivedAt == null)
                        {
                            sku.ArchivedAt = DateTime.UtcNow;
                        }
                    }

                    // Add new ProductSkus that are not already present (not archived)
                    foreach (var skuData in selectedSkus)
                    {
                        if (!existingSkuSet.Contains(skuData.Sku))
                        {
                            product.ProductSkus.Add(new ProductSku
                            {
                                Name = skuData.Name,
                                Sku = skuData.Sku,
                                CreatedAt = DateTime.UtcNow,
                                TemporaryDisabled = false
                            });
                        }
                    }
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        // Reschedule Hangfire job with new settings
        _jobManager.ScheduleStoreJob(product);

        await _dbContext.SaveChangesAsync();

        TempData["Success"] = "Product updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Stores/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _dbContext.Products
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync() ;
        
        if (product == null)
        {
            return NotFound();
        }

        // Remove Hangfire job
        _jobManager.RemoveStoreJob(product);
        
        product.IsEnabled = false;
        product.ArchivedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        TempData["Success"] = "Product archived successfully!";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Stores/ToggleEnabled/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleEnabled(int id)
    {
        var product = await _dbContext.Products
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        product.IsEnabled = !product.IsEnabled;
        product.UpdatedAt = DateTime.UtcNow;

        // Update Hangfire job (will add if enabled, remove if disabled)
        _jobManager.ScheduleStoreJob(product);

        await _dbContext.SaveChangesAsync();

        TempData["Success"] = $"Product {(product.IsEnabled ? "enabled" : "disabled")} successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Adapter)
            .Include(x => x.ProductSkus)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        // Get job execution logs
        var jobExecutionLogs = await _dbContext.JobExecutionLogs
            .AsNoTracking()
            .Include(x => x.ProductSkus)
            .Where(x => x.ProductId == id)
            .OrderByDescending(x => x.ExecutedAt)
            .Take(50)
            .ToListAsync();

        // Get notification history
        var notificationHistory = await _dbContext.NotificationHistory
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.ProductSkus)
            .Where(x => x.ProductId == id)
            .OrderByDescending(x => x.SentAt)
            .Take(50)
            .ToListAsync();

        var viewModel = new ProductDetailsViewModel
        {
            Product = product,
            JobExecutionLogs = jobExecutionLogs,
            NotificationHistory = notificationHistory
        };

        return View(viewModel);
    }

    // POST: /Product/ScrapeProductPage
    [HttpPost]
    public async Task<IActionResult> ScrapeProductPage([FromBody] ScrapeProductPageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Url))
        {
            return BadRequest(new { success = false, message = "URL is required" });
        }

        try
        {
            var result = await _scraperService.ScrapeProductPageAsync(request.Url);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Failed to scrape product page"
                });
            }

            Console.WriteLine($"Scraper result - AvailabilityUrl: {result.AvailabilityUrl ?? "NULL"}");

            return Ok(new
            {
                success = true,
                productId = result.ProductId,
                productName = result.ProductName,
                availabilityUrl = result.AvailabilityUrl,
                availableSkus = result.AvailableSkus,
                availableSizes = result.AvailableSizes,
                // Also pass full product name for saving
                fullProductName = result.ProductName
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Server error: {ex.Message}"
            });
        }
    }
}

public class ScrapeProductPageRequest
{
    public string Url { get; set; } = string.Empty;
}

public class SkuData
{
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("sku")]
    public int Sku { get; set; }
}
