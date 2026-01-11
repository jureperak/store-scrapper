using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Data;
using StoreScrapper.Jobs;
using StoreScrapper.Models.Entities;
using StoreScrapper.Models.ViewModels;

namespace StoreScrapper.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly HangfireJobManager _jobManager;

    public ProductController(AppDbContext dbContext, HangfireJobManager jobManager)
    {
        _dbContext = dbContext;
        _jobManager = jobManager;
    }

    // GET: /Stores
    public async Task<IActionResult> Index()
    {
        var products = await _dbContext.Products
            .Include(x => x.Adapter)
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

        var product = new Product
        {
            AdapterId = adapter.Id,
            AvailabilityUrl = model.AvailabilityUrl,
            ProductPageUrl = model.ProductPageUrl,
            IsEnabled = model.IsEnabled,
            CheckIntervalSeconds = 5,
            CreatedAt = DateTime.UtcNow,
            ProductSkus = new()
        };
        
        // product.ProductSkus.AddRange(model.);

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
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
            
        if (product == null)
        {
            return NotFound();
        }

        var viewModel = new ProductFormViewModel
        {
            Id = product.Id,
            Adapter = product.Adapter.Name,
            AvailabilityUrl = product.AvailabilityUrl,
            ProductPageUrl = product.ProductPageUrl,
            IsEnabled = product.IsEnabled
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
        product.AvailabilityUrl = model.AvailabilityUrl;
        product.ProductPageUrl = model.ProductPageUrl;
        product.IsEnabled = model.IsEnabled;
        product.UpdatedAt = DateTime.UtcNow;

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
}
