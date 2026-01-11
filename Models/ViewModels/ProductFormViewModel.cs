using System.ComponentModel.DataAnnotations;

namespace StoreScrapper.Models.ViewModels;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Store Adapter")]
    public string Adapter { get; set; } = "Zara";

    [Required]
    [Display(Name = "Availability URL")]
    [Url]
    public string AvailabilityUrl { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Product Page URL")]
    [Url]
    public string ProductPageUrl { get; set; } = string.Empty;

    [Display(Name = "Enabled")]
    public bool IsEnabled { get; set; } = true;
}
