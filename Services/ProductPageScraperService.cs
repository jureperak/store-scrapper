using Microsoft.Playwright;
using System.Text.Json;

namespace StoreScrapper.Services;

public interface IProductPageScraperService
{
    Task<ProductPageScraperResult> ScrapeProductPageAsync(string url);
}

public class ProductPageScraperService : IProductPageScraperService
{
    private readonly ILogger<ProductPageScraperService> _logger;

    public ProductPageScraperService(ILogger<ProductPageScraperService> logger)
    {
        _logger = logger;
    }
    
    public async Task<ProductPageScraperResult> ScrapeProductPageAsync(string url)
    {
        try
        {
            _logger.LogInformation("Starting scrape for URL: {Url}", url);

            var playwright = await Playwright.CreateAsync();

            var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });
            
            _logger.LogInformation("Browser launched for URL: {Url}", url);

            var context = await browser.NewContextAsync(new()
            {
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                    "AppleWebKit/537.36 (KHTML, like Gecko) " +
                    "Chrome/121.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
                Locale = "en-US",
                TimezoneId = "America/New_York"
            });

            await context.AddInitScriptAsync("""
                                             Object.defineProperty(navigator, 'webdriver', {
                                                 get: () => undefined
                                             });
                                             """);

            var page = await context.NewPageAsync();

            // Listen to network requests BEFORE navigating to capture availability URL
            var apiUrls = new List<string>();
            page.RequestFinished += (_, request) =>
            {
                var requestUrl = request.Url;
                if (requestUrl.Contains("/itxrest/") || requestUrl.Contains("availability"))
                {
                    apiUrls.Add(requestUrl);
                    Console.WriteLine($"Captured API URL: {requestUrl}");
                    _logger.LogInformation("Captured API URL: {RequestUrl}", requestUrl);
                }
            };
            
            page.Response += async (_, response) =>
            {
                var url = response.Url;
                if (url.Contains("/itxrest/") || url.Contains("availability"))
                {
                    var headers = await response.AllHeadersAsync();
                    headers.TryGetValue("content-type", out var ct);

                    Console.WriteLine($"API RESP: {response.Status} {ct} {url}");
                    _logger.LogInformation("API RESP: {Status} {ContentType} {Url}", response.Status, ct, url);

                    // Optional: if HTML, dump first chars to prove it's a block page
                    if (ct != null && ct.Contains("text/html"))
                    {
                        var body = await response.TextAsync();
                        Console.WriteLine($"HTML SAMPLE: {body.Substring(0, Math.Min(300, body.Length))}");
                        _logger.LogWarning("HTML SAMPLE: {Sample}", body.Substring(0, Math.Min(300, body.Length)));
                    }
                }
            };

            await page.GotoAsync(url, new()
            {
                WaitUntil = WaitUntilState.Load,
                Timeout = 30000
            });

            await page.WaitForSelectorAsync("body");

            // Give time for API calls to complete after page load
            await Task.Delay(2000);

            // Scrape product information
            var productId = await ScrapeProductIdAsync(page, url);

            // Extract from window.zara.viewPayload
            var zaraPayloadData = await ScrapeFromZaraViewPayloadAsync(page, productId);

            if (zaraPayloadData == null || !zaraPayloadData.Skus.Any())
            {
                await browser.CloseAsync();
                playwright.Dispose();

                return new ProductPageScraperResult
                {
                    Success = false,
                    ErrorMessage = "Failed to extract product data from window.zara.viewPayload"
                };
            }

            // Get availability URL from network requests
            string? availabilityUrl = null;

            if (!string.IsNullOrWhiteSpace(productId))
            {
                availabilityUrl = apiUrls
                    .Where(x => x.Contains(productId))
                    .FirstOrDefault();

                Console.WriteLine($"Found availability URL from network requests with productId: {availabilityUrl ?? "NULL"}");
            }

            // If not found by productId, take first availability URL
            if (string.IsNullOrWhiteSpace(availabilityUrl))
            {
                availabilityUrl = apiUrls.FirstOrDefault();
                Console.WriteLine($"Using first API URL as fallback: {availabilityUrl ?? "NULL"}");
            }

            Console.WriteLine($"Total API URLs captured: {apiUrls.Count}");

            var availableSizes = zaraPayloadData.Skus.Select(s => s.Name).ToList();

            await browser.CloseAsync();
            playwright.Dispose();
            
            _logger.LogInformation("Total API URLs captured: {Count}", apiUrls.Count);

            return new ProductPageScraperResult
            {
                Success = true,
                ProductId = productId,
                ProductName = zaraPayloadData.ProductName,
                AvailabilityUrl = availabilityUrl,
                AvailableSkus = zaraPayloadData.Skus,
                AvailableSizes = availableSizes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping product page: {Message}", ex.Message);

            return new ProductPageScraperResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<ZaraPayloadData?> ScrapeFromZaraViewPayloadAsync(IPage page, string? productId)
    {
        try
        {
            // Execute JavaScript to extract window.zara.viewPayload
            var payloadJson = await page.EvaluateAsync<string>(@"
                () => {
                    if (window.zara && window.zara.viewPayload) {
                        return JSON.stringify(window.zara.viewPayload);
                    }
                    return null;
                }
            ");

            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                return null;
            }

            var payload = JsonSerializer.Deserialize<JsonElement>(payloadJson);

            if (!payload.TryGetProperty("product", out var product))
            {
                return null;
            }

            // Get base product name
            var baseName = product.TryGetProperty("name", out var nameProperty)
                ? nameProperty.GetString() ?? ""
                : "";

            // Navigate to colors array
            if (!product.TryGetProperty("detail", out var detail) ||
                !detail.TryGetProperty("colors", out var colors))
            {
                return null;
            }

            // Find the correct color variant
            JsonElement? correctColor = null;

            if (colors.ValueKind == JsonValueKind.Array)
            {
                foreach (var color in colors.EnumerateArray())
                {
                    // Try to match with productId (from v1 param or html id)
                    if (!string.IsNullOrWhiteSpace(productId) &&
                        color.TryGetProperty("id", out var colorId))
                    {
                        var colorIdStr = colorId.GetString() ?? "";
                        if (colorIdStr == productId || colorIdStr.Contains(productId))
                        {
                            correctColor = color;
                            break;
                        }
                    }
                }

                // If no match by ID, take the first one
                if (correctColor == null && colors.GetArrayLength() > 0)
                {
                    correctColor = colors[0];
                }
            }

            if (correctColor == null)
                return null;

            var colorValue = correctColor.Value;

            // Get color name and append to product name
            var colorName = colorValue.TryGetProperty("name", out var colorNameProperty)
                ? colorNameProperty.GetString() ?? ""
                : "";

            var fullProductName = string.IsNullOrWhiteSpace(colorName)
                ? baseName
                : $"{baseName} - {colorName}";

            // Extract sizes with SKU numbers
            var skus = new List<ProductSkuInfo>();

            if (colorValue.TryGetProperty("sizes", out var sizes) &&
                sizes.ValueKind == JsonValueKind.Array)
            {
                Console.WriteLine($"Found {sizes.GetArrayLength()} sizes in window.zara.viewPayload");
                foreach (var size in sizes.EnumerateArray())
                {
                    var sizeName = size.TryGetProperty("name", out var sizeNameProp)
                        ? sizeNameProp.GetString() ?? ""
                        : "";

                    var skuNumber = 0;
                    if (size.TryGetProperty("sku", out var skuProp))
                    {
                        if (skuProp.ValueKind == JsonValueKind.Number)
                        {
                            skuNumber = skuProp.GetInt32();
                        }
                        else if (skuProp.ValueKind == JsonValueKind.String)
                        {
                            var skuStr = skuProp.GetString() ?? "";
                            int.TryParse(skuStr, out skuNumber);
                        }
                    }

                    Console.WriteLine($"Extracted size: Name='{sizeName}', SKU={skuNumber}");

                    if (!string.IsNullOrWhiteSpace(sizeName))
                    {
                        skus.Add(new ProductSkuInfo
                        {
                            Name = sizeName,
                            Sku = skuNumber
                        });
                    }
                }
            }
            else
            {
                Console.WriteLine("No sizes array found in window.zara.viewPayload");
            }

            var result = new ZaraPayloadData
            {
                ProductName = fullProductName,
                Skus = skus,
                AvailabilityUrl = null  // Will be captured from network requests instead
            };

            Console.WriteLine($"Returning {skus.Count} SKUs from window.zara.viewPayload");

            return result;
        }
        catch (Exception ex)
        {
            // Log error but don't fail the whole scraping
            Console.WriteLine($"Error extracting from window.zara.viewPayload: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    private async Task<string?> ScrapeProductIdAsync(IPage page, string productUrl)
    {
        try
        {
            // First, check if URL contains "v1=" parameter
            if (productUrl.Contains("v1="))
            {
                var uri = new Uri(productUrl);
                var query = uri.Query;
                var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                var v1Value = queryParams["v1"];

                if (!string.IsNullOrWhiteSpace(v1Value))
                {
                    return v1Value;
                }
            }

            // Fallback: Get the HTML tag's id attribute
            var htmlElement = await page.QuerySelectorAsync("html");
            if (htmlElement != null)
            {
                var idAttribute = await htmlElement.GetAttributeAsync("id");
                if (!string.IsNullOrWhiteSpace(idAttribute))
                {
                    // Extract product ID from format "product-496085323"
                    var parts = idAttribute.Split('-');
                    if (parts.Length > 1 && parts[0] == "product")
                    {
                        return parts[1];
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

}

public class SizeComparer : IComparer<string>
{
    private static readonly Dictionary<string, int> SizeOrder = new()
    {
        { "XXXS", 1 },
        { "XXS", 2 },
        { "XS", 3 },
        { "S", 4 },
        { "M", 5 },
        { "L", 6 },
        { "XL", 7 },
        { "XXL", 8 },
        { "XXXL", 9 },
        { "2XL", 8 },
        { "3XL", 9 },
        { "4XL", 10 },
        { "5XL", 11 }
    };

    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        // Normalize to uppercase for comparison
        var xUpper = x.Trim().ToUpper();
        var yUpper = y.Trim().ToUpper();

        // Check if both are in the size order dictionary
        var xHasOrder = SizeOrder.TryGetValue(xUpper, out var xOrder);
        var yHasOrder = SizeOrder.TryGetValue(yUpper, out var yOrder);

        if (xHasOrder && yHasOrder)
        {
            return xOrder.CompareTo(yOrder);
        }

        // If one is a standard size and the other isn't, standard size comes first
        if (xHasOrder) return -1;
        if (yHasOrder) return 1;

        // Try to parse as numbers (for numeric sizes like 32, 34, 36, etc.)
        var xIsNumber = int.TryParse(xUpper, out var xNumber);
        var yIsNumber = int.TryParse(yUpper, out var yNumber);

        if (xIsNumber && yIsNumber)
        {
            return xNumber.CompareTo(yNumber);
        }

        // If one is numeric and the other isn't, numeric comes after letter sizes
        if (xIsNumber) return 1;
        if (yIsNumber) return -1;

        // Fall back to alphabetical comparison
        return string.Compare(xUpper, yUpper, StringComparison.Ordinal);
    }
}

public class ProductPageScraperResult
{
    public bool Success { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? AvailabilityUrl { get; set; }
    public List<ProductSkuInfo> AvailableSkus { get; set; } = new();
    public List<string> AvailableSizes { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class ProductSkuInfo
{
    public int Sku { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ZaraPayloadData
{
    public string ProductName { get; set; } = string.Empty;
    public List<ProductSkuInfo> Skus { get; set; } = new();
    public string? AvailabilityUrl { get; set; }
}
