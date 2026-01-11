using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Data;

namespace StoreScrapper.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductSkuController : ControllerBase
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
    [HttpGet("{token}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid token)
    {
        // Find the reactivation record by URL (contains the GUID token)
        var reactivation = await _dbContext.ProductSkuReActivations
            .Include(x => x.ProductSku)
            .Where(x => x.ReEnableUrl.Contains(token.ToString()))
            .FirstOrDefaultAsync();

        if (reactivation == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Reactivation token not found."
            });
        }

        // Check if already used
        if (reactivation.IsUsed)
        {
            return BadRequest(new
            {
                success = false,
                message = "This reactivation link has already been used."
            });
        }

        // Check if expired
        if (reactivation.ValidTo < DateTime.UtcNow)
        {
            return BadRequest(new
            {
                success = false,
                message = $"This reactivation link expired on {reactivation.ValidTo:u}.",
                expiredAt = reactivation.ValidTo
            });
        }

        // Reactivate the ProductSku
        reactivation.ProductSku.TemporaryDisabled = false;
        reactivation.ProductSku.UpdatedAt = DateTime.UtcNow;

        // Mark reactivation as used
        reactivation.IsUsed = true;
        reactivation.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = $"ProductSku '{reactivation.ProductSku.Name}' (SKU: {reactivation.ProductSku.Sku}) has been successfully reactivated.",
            productSkuId = reactivation.ProductSku.Id,
            productSkuName = reactivation.ProductSku.Name,
            sku = reactivation.ProductSku.Sku
        });
    }
}
