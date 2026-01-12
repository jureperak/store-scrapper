using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Data;
using StoreScrapper.Models.ViewModels;

namespace StoreScrapper.Controllers;

public class ProductSkuController : Controller
{
    private readonly AppDbContext _dbContext;

    public ProductSkuController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Reactivates a temporarily disabled ProductSku using a reactivation token
    /// </summary>
    /// <param name="token">The reactivation token (GUID)</param>
    /// <returns>Result of the reactivation attempt</returns>
    [HttpGet("ProductSku/{token}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid token)
    {
        // Find the reactivation record by URL (contains the GUID token)
        var reactivation = await _dbContext.ProductSkuReActivations
            .Include(x => x.ProductSku)
            .ThenInclude(x => x.Product)
            .Where(x => x.ReEnableUrl.Contains(token.ToString()))
            .FirstOrDefaultAsync();

        if (reactivation == null)
        {
            return View(new ReactivationResultViewModel
            {
                Success = false,
                Message = "Reactivation token not found.",
                ErrorType = "NotFound"
            });
        }

        // Check if already used
        if (reactivation.IsUsed)
        {
            return View(new ReactivationResultViewModel
            {
                Success = false,
                Message = "This reactivation link has already been used.",
                ErrorType = "AlreadyUsed",
                ProductSkuName = reactivation.ProductSku.Name,
                Sku = reactivation.ProductSku.Sku
            });
        }

        // Check if expired
        if (reactivation.ValidTo < DateTime.UtcNow)
        {
            return View(new ReactivationResultViewModel
            {
                Success = false,
                Message = $"This reactivation link expired on {reactivation.ValidTo.ToLocalTime():g}.",
                ErrorType = "Expired",
                ExpiredAt = reactivation.ValidTo,
                ProductSkuName = reactivation.ProductSku.Name,
                Sku = reactivation.ProductSku.Sku
            });
        }

        // Reactivate the ProductSku
        reactivation.ProductSku.TemporaryDisabled = false;
        reactivation.ProductSku.UpdatedAt = DateTime.UtcNow;

        // Mark reactivation as used
        reactivation.IsUsed = true;
        reactivation.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return View(new ReactivationResultViewModel
        {
            Success = true,
            Message = $"Your product SKU has been reactivated successfully!",
            ProductSkuName = reactivation.ProductSku.Name,
            Sku = reactivation.ProductSku.Sku,
            ProductId = reactivation.ProductSku.ProductId,
            ProductName = reactivation.ProductSku.Product.Name
        });
    }
}
