using System.ComponentModel.DataAnnotations;

namespace StoreScrapper.Models.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Store Adapter")]
    public string Adapter { get; set; } = "Zara";

    [Display(Name = "Availability URL")]
    public string? AvailabilityUrl { get; set; }

    [Required]
    [Display(Name = "Product Page URL")]
    [Url]
    public string ProductPageUrl { get; set; } = string.Empty;

    [Display(Name = "Product Name")]
    public string? ProductName { get; set; }

    [Display(Name = "Enabled")]
    public bool IsEnabled { get; set; } = true;

    // Selected SKUs with both name and SKU number (to be posted as JSON from client)
    public string? SelectedSkusJson { get; set; }
}
