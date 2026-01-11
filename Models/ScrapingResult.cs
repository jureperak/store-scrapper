namespace StoreScrapper.Models;

public class ScrapingResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int ProductSkusFound { get; set; }

    public static ScrapingResult CreateSuccess(int productSkusFound)
    {
        return new ScrapingResult
        {
            Success = true,
            ProductSkusFound = productSkusFound,
        };
    }

    public static ScrapingResult CreateError(string message)
    {
        return new ScrapingResult
        {
            Success = false,
            Message = message
        };
    }

    public static ScrapingResult CreateSkipped(string message)
    {
        return new ScrapingResult
        {
            Success = true,
            Message = message
        };
    }
}
